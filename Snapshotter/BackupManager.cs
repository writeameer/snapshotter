﻿using System;
using System.Collections.Generic;
using System.Linq;
using Alphaleonis.Win32.Vss;
using Amazon.EC2.Model;
using Amazon.Util;
using Cloudoman.AwsTools.Snapshotter.Helpers;
using Cloudoman.AwsTools.Snapshotter.Models;

namespace Cloudoman.AwsTools.Snapshotter
{
    public class BackupManager
    {
        readonly string _backupName;
        List<VolumeInfo> _volumesInfo;
        IVssImplementation _vssImplementation;
        IVssBackupComponents _vssBackupComponents;


        public BackupManager(BackupRequest request)
        {
            // Get Backup Name from Request or from Instance NAME tag
            _backupName = request.BackupName ?? InstanceInfo.ServerName;
            if ( String.IsNullOrEmpty(_backupName))
            {
                _backupName = System.Net.Dns.GetHostName();
                var message = "When a backupname is not provided, it's defauted to this EC2 Instances's 'Name' tag .";
                message += "Unable to determing either. Falling back to this EC2 Instance's hostname.";

                Logger.Info(message, "BackupManager");
                Logger.Info("Backup name:" + _backupName, "BackupManager");
            }
        }

        public void StartBackup()
        {

            // Get Volumes attached to local instance 
            // other than the boot volume
            var volumes = InstanceInfo.GetMyVolumes();
            _volumesInfo = volumes.Where(v => v.Attachment[0].Device != "/dev/sda1").Select(x => new VolumeInfo
            {
                VolumeId = x.Attachment[0].VolumeId,
                DeviceName = x.Attachment[0].Device,
                Drive = x.Tag.Where(t => t.Key == "Drive").Select(d => d.Value).FirstOrDefault(),
                ServerName = InstanceInfo.ServerName,
                BackupName = _backupName,
                TimeStamp = AWSSDKUtils.FormattedCurrentTimestampRFC822
            }).ToList();


            // Check pre-requisites before intiating backup
            if (!CheckBackupPreReqs())
            {
                Logger.Error("Pre-requisites not met, exitting.", "SnapshotBackup");
                return;
            }

            // Snapshot volumes
            Logger.Info("Job Started", "BackupManager");
            BackupVolumes();
            Logger.Info("Job Ended", "BackupManager");
        }

        bool CheckBackupPreReqs()
        {

            // Check instance has EBS volumes to snapshot
            // excluding boot volume
            if (!_volumesInfo.Any())
            {
                Logger.Error("No EBS volumes excluding boot drive were found for snapshotting.\nExitting.", "CheckBackupPreReqs");
                return false;
            }

            // Ensure all volumes for this server have resources tags
            // identifying their drive letters
            var missingDriveLetters = _volumesInfo.Where(x => String.IsNullOrEmpty(x.Drive));
            if (missingDriveLetters.Count() > 0)
            {
                var volumes = string.Join(",", missingDriveLetters.Select(x => x.VolumeId));
                Logger.Error("All volumes must be tagged with EC2 resource tags marking their drive letter. For E.g. Key='Drive', Value='H'.", "CheckBackupPreReqs");
                Logger.Error("The following volumes:", "SnapshotBackup");
                Logger.Error(volumes, "SnapshotBackup");
                Logger.Error(" do not contain EC2 resource tags marking their drive letter.\nExitting.", "CheckBackupPreReqs");
                return false;
            }

            if (_backupName == null)
            {
                Logger.Error("A BackupName for the snapshots MUST be specified. Exitting...", "CheckBackupPreReqs");
            }
            return true;
        }


        void SnapshotVolume(VolumeInfo backupVolumeInfo)
        {


            try
            {
                // Create Snapshot Request
                var fullDescription = String.Format("DeviceName:{0}, Drive:{1}", backupVolumeInfo.DeviceName, backupVolumeInfo.Drive);
                var request = new CreateSnapshotRequest
                {
                    VolumeId = backupVolumeInfo.VolumeId,
                    Description = fullDescription
                };

                // Create Snapshot
                var response = InstanceInfo.Ec2Client.CreateSnapshot(request);
                var snapshotId = response.CreateSnapshotResult.Snapshot.SnapshotId;

                Logger.Info("Created Snapshot:" + snapshotId + " for Volume Id:" + backupVolumeInfo.VolumeId, "SnapShotVolume");

                // Create Tag Request
                var tagRequest = new CreateTagsRequest
                {
                    ResourceId = new List<string> { snapshotId },
                    Tag = new List<Tag>{
                        new Tag {Key = "TimeStamp", Value = backupVolumeInfo.TimeStamp},
                        new Tag {Key = "ServerName", Value = backupVolumeInfo.ServerName},
                        new Tag {Key = "VolumeId", Value = backupVolumeInfo.VolumeId},
                        new Tag {Key = "InstanceId", Value = InstanceInfo.InstanceId},
                        new Tag {Key = "DeviceName", Value = backupVolumeInfo.DeviceName},
                        new Tag {Key = "Drive", Value = backupVolumeInfo.Drive},
                        new Tag {Key = "Name", Value = "Snapshotter Backup: " + _backupName},
                        new Tag {Key = "BackupName", Value = _backupName}
                    }
                };

                // Tag Snapshot
                InstanceInfo.Ec2Client.CreateTags(tagRequest);
                Logger.Info("Server " + InstanceInfo.ServerName + ":" + InstanceInfo.InstanceId + " Volume Id:" + backupVolumeInfo.VolumeId + " was snapshotted and tagged.", "SnapShotVolume");
            }
            catch (Exception e)
            {
                Logger.Error(e.StackTrace, "SnapshotVolume");
            }


        }

        void BackupVolumes()
        {
            // Backup Each Volume
            _volumesInfo.ForEach(x =>
            {
                // Snapshot Volume
                var driveName = x.Drive + ":\\";
                StartVssBackup(driveName);
                SnapshotVolume(x);
                AbortVssBackup();
            });
        }

        void StartVssBackup(string driveName)
        {
            // Use Shadow Copy Service to create consistent filesystem snapshot
            _vssImplementation = VssUtils.LoadImplementation();
            _vssBackupComponents = _vssImplementation.CreateVssBackupComponents();
            _vssBackupComponents.InitializeForBackup(null);
            _vssBackupComponents.SetBackupState(false, false, VssBackupType.Full, false);
            _vssBackupComponents.StartSnapshotSet();
            _vssBackupComponents.AddToSnapshotSet(driveName);
            _vssBackupComponents.PrepareForBackup();
            _vssBackupComponents.DoSnapshotSet(); 
        }

        void AbortVssBackup()
        {
            _vssBackupComponents.AbortBackup();
        }
    }
}
