using System.Collections.Generic;
using Amazon.EC2.Model;
using Cloudoman.AwsTools.Snapshotter.Helpers;
using Cloudoman.AwsTools.Snapshotter.Models;
using System;
using System.Linq;
using System.Threading;
using Cloudoman.DiskTools;


namespace Cloudoman.AwsTools.Snapshotter
{
    public class RestoreManager
    {
        private readonly string _backupName;
        private readonly string _timeStamp;
        private IEnumerable<SnapshotInfo> _allSnapshots;

        public RestoreManager(RestoreRequest request)
        {
            
            // Get Backup Name from Request or from Instance NAME tag
            _backupName = request.BackupName ?? InstanceInfo.ServerName;
            if (_backupName == null)
            {
                var message = "The backup name defauts to this EC2 Instances's 'Name' tag.";
                message += "Please explicitly provide a backup name OR tag this EC2 instance with a name";
                message += "from the AWS Console or using the AWS API.";

                Logger.Error(message, "RestoreManager");
                throw new System.ApplicationException(message);
            }


            // Get Snapshots with given backup name
            _allSnapshots = GetAllSnapshots();
            if (_allSnapshots.ToList().Count == 0 )
            {
                var message = "No snapshots were found for BackupName:" + _backupName + " and timestamp: " + _timeStamp + ".Exitting"; 
                Logger.Info(message, "RestoreManager");
                return;
            }

            // Get Snapshot timestamp from Request or default to latest in _snapshotInfo
            _timeStamp = request.TimeStamp;
            if (String.IsNullOrEmpty(_timeStamp)) _timeStamp = null;
            if (_timeStamp == null && GetLatestSnapshotTimeStamp() == null)
            {
                var message = "No timestamp was explicitly provided. Unable to determine the timestamp of the latest snapshot. Exitting.";
                Logger.Error(message, "RestoreManager");
            }

        }


        IEnumerable<SnapshotInfo> GetAllSnapshots()
        {

            // Find EC2 Snapshots based for given BackupName
            var filters = new List<Filter> {
                new Filter {Name = "tag:BackupName", Value = new List<string> { _backupName }},
            };

            var request = new DescribeSnapshotsRequest { Filter = filters };
            var snapshots = InstanceInfo.Ec2Client.DescribeSnapshots(request).DescribeSnapshotsResult.Snapshot;

            // Generate List<SnapshotInfo> from EC2 Snapshots
            var snapshotsInfo = new List<SnapshotInfo>();
            snapshots.ForEach(x => snapshotsInfo.Add(new SnapshotInfo
            {
                SnapshotId = x.SnapshotId,
                BackupName = x.Tag.Get("BackupName"),
                DeviceName = x.Tag.Get("DeviceName"),
                Hostname = x.Tag.Get("HostName"),
                Drive = x.Tag.Get("Drive"),
                TimeStamp = x.Tag.Get("TimeStamp"),
            }));

            // Order Descending by date
            snapshotsInfo = snapshotsInfo.OrderByDescending(x => Convert.ToDateTime(x.TimeStamp)).ToList();
            return snapshotsInfo;
        }

        string GetLatestSnapshotTimeStamp()
        {
            var newest = _allSnapshots.Max(x => Convert.ToDateTime(x.TimeStamp));

            var something = _allSnapshots
                        .Where(x => Convert.ToDateTime(x.TimeStamp) == newest)
                        .Select(x => x.TimeStamp).FirstOrDefault();
            return something;
        }

        string GetOldestSnapshotTimeStamp()
        {
            var oldest = _allSnapshots.Min(x => Convert.ToDateTime(x.TimeStamp));
            return _allSnapshots
                        .Where(x => Convert.ToDateTime(x.TimeStamp) == oldest)
                        .Select(x => x.TimeStamp).FirstOrDefault();
        }

        /// <summary>
        /// Retrieves a set of snapshots filtered by BackupName for a given timestamp.
        /// The timestamp is passed via the construcstor or defaulted to latest timestamp.
        /// </summary>
        /// <returns></returns>
        public List<SnapshotInfo> GetSnapshotSet()
        {
            // Filter list by timestamp and order ascending
            var timeStamp = _timeStamp ?? GetLatestSnapshotTimeStamp();
            return _allSnapshots
                    .Where(x => Convert.ToDateTime(x.TimeStamp) == Convert.ToDateTime(timeStamp))
                    .OrderByDescending(x => Convert.ToDateTime(x.TimeStamp)).ToList();
        }

        public void StartRestore()
        {
            Logger.Info("Restore Started", "RestoreManager.StartRestore");
            Logger.Info("Backup Name:" + _backupName, "RestoreManager.StartRestore");

            // Find Snapshots to Restore
            var snapshots = GetSnapshotSet();
            snapshots.ToList().ForEach(x =>
            {
                Console.WriteLine(x.ToString());
                RestoreVolume(x);
            });

            Logger.Info("Restore Ended", "RestoreManager.StartRestore");
        }

        public void RestoreVolume(SnapshotInfo snapshot)
        {
            
            Logger.Info("Restore Volume:" + snapshot.SnapshotId,"RestoreVolume");

            // Create New Volume and Tag it
            var volumeId = CreateVolume(snapshot);
            TagVolume(snapshot, volumeId);

            // Detach Volume if any
            DetachVolume(snapshot);

            //Attach new volume
            AttachVolume(snapshot, volumeId);
 
            // Set Delete on termination to TRUE for restored volume
            SetDeleteOnTermination(snapshot.DeviceName, true);
        }


        /// <summary>
        /// Lists snapshots matching timestamp passed in via constructor.
        /// List all available snapshots when timestamp was omitted
        /// </summary>
        public void List()
        {
            Logger.Info("Listing Snaphshots", "RestoreManager.List");
            Logger.Info("Backup Name:" + _backupName, "RestoreManager.List");
            Console.WriteLine(new SnapshotInfo().FormattedHeader);
            if (_timeStamp == null)
                _allSnapshots.ToList().ForEach(Console.WriteLine);
            else
                GetSnapshotSet().ToList().ForEach(Console.WriteLine);
        }

        void SetDeleteOnTermination(string DeviceName, bool deleteOnTermination)
        {
            Logger.Info("SetDeleteOnTermination " + DeviceName + " to " + deleteOnTermination, "SetDeleteOnTermination");

            try
            {
                var modifyAttrRequest = new ModifyInstanceAttributeRequest
                {
                    InstanceId = InstanceInfo.InstanceId,
                    BlockDeviceMapping = new List<InstanceBlockDeviceMappingParameter>
                    {
                        new InstanceBlockDeviceMappingParameter{
                            DeviceName="xvdf",
                            Ebs = new InstanceEbsBlockDeviceParameter{DeleteOnTermination = true,VolumeId="vol-c32e2eea"}
                        }
                    }
                };

                var response = InstanceInfo.Ec2Client.ModifyInstanceAttribute(modifyAttrRequest);
            }
            catch (Amazon.EC2.AmazonEC2Exception ex)
            {
                Logger.Error("Error setting DeleteOnTermination flag for:" + DeviceName, "SetDeleteOnTermination");
                Logger.Error("Exception:" + ex.Message + "\n" + ex.StackTrace, "RestoreVolume");
            }
            
        }

        string CreateVolume(SnapshotInfo snapshot)
        {
            Logger.Info("Creating Volume for snapshot :" + snapshot.SnapshotId, "CreateVolume");

            string volumeId = "";
            try
            {
                var createVolumeRequest = new CreateVolumeRequest
                {
                    SnapshotId = snapshot.SnapshotId,
                    AvailabilityZone = InstanceInfo.AvailabilityZone,
                };


                var volume = InstanceInfo.Ec2Client.CreateVolume(createVolumeRequest).CreateVolumeResult.Volume;
                volumeId = volume.VolumeId;
                Logger.Info("Created Volume:" + volumeId, "RestoreVolume");

            }
            catch (Amazon.EC2.AmazonEC2Exception ex)
            {
                Logger.Error("Could not create volume.", "RestoreVolume");
                Logger.Error("Exception:" + ex.Message + "\n" + ex.StackTrace, "RestoreVolume");
                return null;
            }

            return volumeId;
        }

        void TagVolume(SnapshotInfo snapshot, string volumeId)
        {
                        // Tag restored volume
            try
            {
                Logger.Info("Tagging restored volume with backup metadata", "RestoreVolume");
                var tagRequest = new CreateTagsRequest
                {
                    ResourceId = new List<string> { volumeId },
                    Tag = new List<Tag>{
                        new Tag {Key = "TimeStamp", Value = snapshot.TimeStamp},
                        new Tag{Key="HostName", Value=InstanceInfo.HostName},
                        new Tag{Key="SnapshotId", Value=snapshot.SnapshotId},
                        new Tag{Key="InstanceID", Value=InstanceInfo.InstanceId},
                        new Tag{Key="DeviceName", Value=snapshot.DeviceName},
                        new Tag{Key="DeviceName", Value=snapshot.Drive},
                        new Tag{Key="Name", Value="Snapshotter Restore: " + _backupName},
                        new Tag{Key="BackupName", Value=_backupName},
                    }
                };

                InstanceInfo.Ec2Client.CreateTags(tagRequest);

            }
            catch (Amazon.EC2.AmazonEC2Exception ex)
            {
                Logger.Error("Error tagging volume:" + volumeId,"RestoreVolume");
                Logger.Error("Exception:" + ex.Message + "\n" + ex.StackTrace, "RestoreVolume");
            }


        }

        void DetachVolume(SnapshotInfo snapshot)
        {
            // Detach any existing volumes from requested device if 
            // it is not free
            var currentVolume = InstanceInfo.Volumes.Where(x => x.Attachment[0].Device == snapshot.DeviceName).FirstOrDefault();

            
            if (currentVolume != null)
            {
                try
                {

                    Logger.Info("Requested device:" + snapshot.DeviceName + " is already attached to a volume" + currentVolume.VolumeId , "DetachVolume");

                    // Take Windows Physical Disk Offline
                    OfflineDrive(snapshot);

                    Logger.Info("Detaching Volume", "RestoreVolume");

                    var detachRequest = new DetachVolumeRequest
                    {
                        InstanceId = InstanceInfo.InstanceId,
                        VolumeId = currentVolume.VolumeId,
                        Device = snapshot.DeviceName,
                        Force = true
                    };

                    var response = InstanceInfo.Ec2Client.DetachVolume(detachRequest);

                    Logger.Info("Detached Volume:" + currentVolume.VolumeId + " Drive:" + snapshot.Drive, "RestoreVolume");
                }
                catch (Amazon.EC2.AmazonEC2Exception ex)
                {
                    Logger.Error("Error while detaching existing volume", "RestoreVolume");
                    Logger.Error("Exception:" + ex.Message + "\n" + ex.StackTrace, "RestoreVolume");
                    return;
                }
            }

            WaitAttachmentStatus(currentVolume.VolumeId, status: "available");
        }

        void OfflineDrive(SnapshotInfo snapshot)
        {
            Logger.Info("Taking disk offline before detaching device:" + snapshot.DeviceName, "OfflineDrive");
            // Find the Windows physical disk of the EBS volume
            var diskPart = new DiskPart();
            var volume = diskPart.ListVolume().Where(x => x.Letter == snapshot.Drive).FirstOrDefault();
            var disk = diskPart.VolumeDetail(volume.Num).Disk;
            
            // Run Sync to commit pending changes
            var response = diskPart.OfflineDisk(disk.Num);
            if (!response.Status)
            {
                Logger.Error("Error taking disk offline", "OfflineDrive");
                Logger.Error("Diskpart Output:" + response.Output, "OfflineDrive");
            }
            Logger.Info("Disk was taken offline", "OffineDrive");
        }


        void AttachVolume(SnapshotInfo snapshot, string volumeId)
        {
            // Attach volume to EC2 Instance
            try
            {
                Logger.Info("Attaching Volume to Instance", "RestoreVolume");
                var attachRequest = new AttachVolumeRequest
                {
                    InstanceId = InstanceInfo.InstanceId,
                    VolumeId = volumeId,
                    Device = snapshot.DeviceName
                };

                var result = InstanceInfo.Ec2Client.AttachVolume(attachRequest).AttachVolumeResult;
                Logger.Info("Attached Volume:" + volumeId, "RestoreVolume");
                Logger.Info("Attachment result:" + result.Attachment.AttachTime, "RestoreVolume");
            }
            catch (Amazon.EC2.AmazonEC2Exception ex)
            {
                Logger.Error("Error attaching volume.", "RestoreVolume");
                Logger.Error("Exception:" + ex.Message + "\n" + ex.StackTrace, "RestoreVolume");
            }

            WaitAttachmentStatus(volumeId, status: "in-use");
        }

        string AttachmentStatus (string volumeId)
        {
            
            var volume = InstanceInfo.Ec2Client.DescribeVolumes(new DescribeVolumesRequest { VolumeId = new List<string>{volumeId}})
                                     .DescribeVolumesResult.Volume.FirstOrDefault();
            var status = volume.Status ;
            Logger.Info("Volume :" + volumeId + " status:" + status,"IsAttached" );
            return status;
        }

        public void WaitAttachmentStatus(string volumeId, string status)
        {
            int retry = 12;
            int waitInSeconds = 10;
            string currentStatus = null;

            for (int i = 1; i <= retry; i++ )
            {
                currentStatus = AttachmentStatus(volumeId);
                if (currentStatus != status)
                {
                    Logger.Info("Attachment status:" + currentStatus + " ,Sleep "+ waitInSeconds + " seconds.", "WaitAttachmentStatus");
                    Thread.Sleep(waitInSeconds * 1000);
                }
                else
                    return;
            }
            
            var message ="Volume attachment status still: " + currentStatus + ".Exitting.";
            Logger.Error(message, "WaitForAttachment");
            throw new System.ApplicationException(message);
        }

    }
}
