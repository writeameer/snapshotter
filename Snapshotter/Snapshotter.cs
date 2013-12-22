using System;
using System.Linq;
using Amazon.EC2.Model;
using Amazon.EC2;
using System.Collections.Generic;
using Amazon;
using Alphaleonis.Win32.Vss;
using Amazon.Util;

namespace CloudomanUtils
{
	public class Snapshotter
	{
        readonly AmazonEC2 _ec2Client;
        readonly string _snapshotName;
        readonly string _instanceId;
        readonly string _serverNameTag;
        readonly string _backupName;
        List<VolumeInfo> _backupVolumes;
        List<SnapshotInfo> _snapShots;

        public Snapshotter(string backupName)
	    {
            // Create EC2 Client using IAM creds if none found in app.config
            var ec2Config = new AmazonEC2Config { ServiceURL = Utils.Ec2Region };
            _ec2Client = AWSClientFactory.CreateAmazonEC2Client(ec2Config);

            // Initalize locals
            _instanceId = new System.Net.WebClient().DownloadString("http://169.254.169.254/latest/meta-data/instance-id");
            _serverNameTag = Utils.GetServerTag(_ec2Client, "Name");
            _snapshotName = "Snapshotter Backup: " + _serverNameTag;
            _backupName = backupName;
	    }

        public void DoBackup()
        {

            // Get Info on volumes to be backed up
            var volumes = Utils.GetMyVolumes(_ec2Client);
            _backupVolumes = volumes.Where(v => v.Attachment[0].Device != "/dev/sda1").Select(x => new VolumeInfo
            {
                VolumeId = x.Attachment[0].VolumeId,
                Device = x.Attachment[0].Device,
                Drive = x.Tag.Where(t => t.Key == "Drive").Select(d => d.Value).FirstOrDefault(),
                ServerName = _serverNameTag,
                BackupName = _backupName,
                TimeStamp =  AWSSDKUtils.FormattedCurrentTimestampRFC822 
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
            var snapShots = Utils.GetMySnapshots(_ec2Client, _serverNameTag);
            _snapShots = snapShots
                .Where ( x => x.Status == "completed")
                .Select( x => new SnapshotInfo {
                SnapshotId = x.SnapshotId,
                Drive = x.Tag.Where(t => t.Key == "Drive").Select(d => d.Value).FirstOrDefault(),
                Device = x.Tag.Where(t => t.Key == "DeviceName").Select(d => d.Value).FirstOrDefault()
            }).ToList();

            _snapShots.ToList().ForEach(x => Console.WriteLine(x.SnapshotId + " " + x.Device + " " + x.Drive));
        }


        bool CheckBackupPreReqs()
        {

            // Ensure the instance has a "Name" tag for identifying server
            if (String.IsNullOrEmpty(_serverNameTag))
            {
                Logger.Error("This Instance must be tagged with a server name before it's volumes can be snapshotted.\nExitting.", "CheckBackupPreReqs");
                return false;
            }

            // Check instance has EBS volumes to snapshot
            // excluding boot volume
            if (_backupVolumes.Count() == 0)
            {
                Logger.Error("No EBS volumes excluding boot drive were found for snapshotting.\nExitting.", "CheckBackupPreReqs");
                return false;
            }

            // Ensure all volumes for this server have resources tags
            // identifying their drive letters
            var missingDriveLetters = _backupVolumes.Where(x => String.IsNullOrEmpty(x.Drive));
            if (missingDriveLetters.Count() > 0)
            {
                var volumes = string.Join(",", missingDriveLetters.Select(x => x.VolumeId));
                Logger.Error("All volumes must be tagged with EC2 resource tags marking their drive letter. For E.g. Key='Drive', Value='H'.", "CheckBackupPreReqs");
                Logger.Error("The following volumes:", "SnapshotBackup");
                Logger.Error( volumes, "SnapshotBackup");
                Logger.Error(" do not contain EC2 resource tags marking their drive letter.\nExitting.", "CheckBackupPreReqs");
                return false;
            }

            if (_backupName == null)
            {
                Logger.Error("A BackupName for the snapshots MUST be specified. Exitting...","CheckBackupPreReqs");
            }
            return true;
        }


        void SnapshotVolume(VolumeInfo backupVolumeInfo)
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
                
                Logger.Info("Created Snapshot:" + snapshotId + " for Volume Id:" +  backupVolumeInfo.VolumeId , "SnapShotVolume");

                // Tag Snapshot
                var tagRequest = new CreateTagsRequest {
                    ResourceId = new List<string> {snapshotId},
                    Tag = new List<Tag>{
                        new Tag {Key = "TimeStamp", Value = backupVolumeInfo.TimeStamp},
                        new Tag {Key = "ServerName", Value = backupVolumeInfo.ServerName},
                        new Tag {Key = "VolumeId", Value = backupVolumeInfo.VolumeId},
                        new Tag {Key = "InstanceId", Value = _instanceId},
                        new Tag {Key = "DeviceName", Value = backupVolumeInfo.Device},
                        new Tag {Key = "Drive", Value = backupVolumeInfo.Drive},
                        new Tag {Key = "Name", Value = _snapshotName},
                        new Tag {Key = "BackupName", Value = _backupName}
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

            _backupVolumes.ForEach(x => {

                var driveName = x.Drive + ":\\";

                // Use Shadow Copy Service to create consistent filesystem snapshot
                var vssImplementation = Alphaleonis.Win32.Vss.VssUtils.LoadImplementation();
                var vss = vssImplementation.CreateVssBackupComponents();
                vss.InitializeForBackup(null);
                vss.SetBackupState(false, false, VssBackupType.Full, false);
                vss.StartSnapshotSet();
                vss.AddToSnapshotSet(driveName);
                vss.PrepareForBackup();
                vss.DoSnapshotSet();

                // Snapshot Volume
                SnapshotVolume(x);

                // Abort VSS Backup
                vss.AbortBackup();
            });
        }


	}




}
