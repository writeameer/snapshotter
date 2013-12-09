using System;
using System.Linq;
using Amazon.EC2.Model;
using Amazon.EC2;
using System.Collections.Generic;
using Amazon;
using System.Net;
using System.Text;
using AlphaShadow.Commands;
using AlphaShadow.Options;
using AlphaShadow;
using Alphaleonis.Win32.Vss;


namespace CloudomanUtils
{
	public class Snapshotter
	{
        readonly AmazonEC2 _ec2Client;
        readonly string _snapshotName;
        readonly string _instanceId;
        readonly string _serverNameTag;
        List<BackupVolumeInfo> _backupVolumeInfo;

        public Snapshotter()
	    {
            // Create EC2 Client using IAM creds if none found in app.config
            var ec2Config = new AmazonEC2Config { ServiceURL = Utils.Ec2Region };
            _ec2Client = AWSClientFactory.CreateAmazonEC2Client(ec2Config);

            // Initalize locals
            _instanceId = new System.Net.WebClient().DownloadString("http://169.254.169.254/latest/meta-data/instance-id");
            _serverNameTag = Utils.GetServerTag(_ec2Client, "Name");
            _snapshotName = "Snapshotter Backup: " + _serverNameTag;
	    }

        public void DoBackup()
        {

            // Get Info on volumes to be backed up
            var volumes = Utils.GetMyVolumes(_ec2Client);
            _backupVolumeInfo = volumes.Where(v => v.Attachment[0].Device != "/dev/sda1").Select(x => new BackupVolumeInfo
            {
                Device = x.Attachment[0].Device,
                VolumeId = x.Attachment[0].VolumeId,
                Drive = x.Tag.Where(t => t.Key == "Drive").Select(d => d.Value).FirstOrDefault()
            }).ToList();


            // Check pre-requisites before intiating backup
            if (!CheckBackupPreReqs())
            {
                Logger.Error("Pre-requisites not met, exitting.", "SnapshotBackup");
                return;
            }

            // Snapshot volumes
            BackupStuff();
        }

        public void List()
        {
            var request = new DescribeSnapshotsRequest();

        }
        bool CheckBackupPreReqs()
        {

            // Ensure the instance has a "Name" tag for identifying server
            if (String.IsNullOrEmpty(_serverNameTag))
            {
                Logger.Error("This Instance must be tagged with a server name before it's volumes can be snapshotted.\nExitting.", "SnapshotBackup");
                return false;
            }

            // Check instance has EBS volumes to snapshot
            // excluding boot volume
            if (_backupVolumeInfo.Count() == 0)
            {
                Logger.Error("No EBS volumes excluding boot drive were found for snapshotting.\nExitting.", "SnapshotBackup");
                return false;
            }

            // Ensure all volumes for this server have resources tags
            // identifying their drive letters
            var missingDriveLetters = _backupVolumeInfo.Where(x => String.IsNullOrEmpty(x.Drive));
            if (missingDriveLetters.Count() > 0)
            {
                var volumes = string.Join(",", missingDriveLetters.Select(x => x.VolumeId));
                Logger.Error("All volumes must be tagged with EC2 resource tags marking their drive letter. For E.g. Key='Drive', Value='H'.", "SnapshotBackup");
                Logger.Error("The following volumes:", "SnapshotBackup");
                Logger.Error( volumes, "SnapshotBackup");
                Logger.Error(" do not contain EC2 resource tags marking their drive letter.\nExitting.", "SnapshotBackup");
                return false;
            }

            return true;
        }


        void SnapshotVolume(BackupVolumeInfo backupVolumeInfo, string timeStamp)
        {


            try
            {
                // Create snapshot
                var fullDescription = String.Format("ServerName:{0}, DeviceName:{1}", _serverNameTag, backupVolumeInfo.Device);
                var request = new CreateSnapshotRequest
                {
                    VolumeId = backupVolumeInfo.VolumeId,
                    Description = fullDescription
                };

                var response = _ec2Client.CreateSnapshot(request);
                var snapshotId = response.CreateSnapshotResult.Snapshot.SnapshotId;

                // Tag Snapshot
                var tagRequest = new CreateTagsRequest {
                    ResourceId = new List<string> {snapshotId},
                    Tag = new List<Tag>{
                        new Tag {Key = "TimeStamp", Value = timeStamp},
                        new Tag {Key = "ServerName", Value = _serverNameTag},
                        new Tag {Key = "VolumeId", Value = backupVolumeInfo.VolumeId},
                        new Tag {Key = "InstanceId", Value = _instanceId},
                        new Tag {Key = "DeviceName", Value = backupVolumeInfo.Device},
                        new Tag {Key = "Drive", Value = backupVolumeInfo.Drive},
                        new Tag {Key = "Name", Value = _snapshotName}
                    }
                };

                _ec2Client.CreateTags(tagRequest);
                Logger.Info("Server " + _serverNameTag + ":" + _instanceId + " Volume Id:" + backupVolumeInfo.VolumeId + " was snapshotted and tagged.", "SnapShotVolume");
            }
            catch (Exception e)
            {
                Logger.Error(e.StackTrace, "SnapshotVolume");
            }


        }

        void BackupStuff()
        {
            
            // Create Timestamp for Backup Set
            var timeStamp = DateTime.UtcNow.ToString("yyyyMMddHHmmssffff");

            _backupVolumeInfo.ToList().ForEach(x => {

                var driveName = x.Drive + ":\\";

                // Use Shadow Copy Service to create consistent filesystem snapshot
                var vssImplementation = Alphaleonis.Win32.Vss.VssUtils.LoadImplementation();
                var vss = vssImplementation.CreateVssBackupComponents();
                vss.InitializeForBackup(null);
                vss.SetBackupState(false, false, VssBackupType.Full, false);
                vss.StartSnapshotSet();
                vss.AddToSnapshotSet(driveName);
                vss.PrepareForBackup();

                // Snapshot Volume
                SnapshotVolume(x, timeStamp);

                // Abort VSS Backup
                vss.AbortBackup();
            });
        }


	}




}
