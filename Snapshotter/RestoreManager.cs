using System.Collections.Generic;
using Amazon.EC2.Model;
using Cloudoman.AwsTools.Snapshotter.Helpers;
using Cloudoman.AwsTools.Snapshotter.Models;
using System;
using System.Linq;

namespace Cloudoman.AwsTools.Snapshotter
{
    public class RestoreManager
    {
        private readonly string _backupName;
        private List<SnapshotInfo> _snapshotsInfo;

        public RestoreManager() : this(backupName:InstanceInfo.ServerName) { }

        public RestoreManager(string backupName) : this (backupName, "") {}


        public RestoreManager (string backupName, string timeStamp)
        {
            if (backupName == null)
            {
                var message = "The backup name defauts to this EC2 Instances's 'Name' tag.";
                message += "Please explicitly provide a backup name OR tag this EC2 instance with a name";
                message += "from the AWS Console or using the AWS API.";

                Logger.Error(message, "RestoreManager");
                throw new System.ApplicationException("message");
            }

            _backupName = backupName;
            _snapshotsInfo = ListSnapshots();
        }
        public List<SnapshotInfo> ListSnapshots()
        {
            var filters = new List<Filter> {
                new Filter {Name = "tag-key", Value = new List<string> { "BackupName" }},
                new Filter {Name = "tag-value",Value = new List<string> { _backupName }
                }
            };

            var request = new DescribeSnapshotsRequest { Filter = filters };
            var snapshots = InstanceInfo.Ec2Client.DescribeSnapshots(request).DescribeSnapshotsResult.Snapshot;

            var snapshotsInfo = new List<SnapshotInfo>();
            

            snapshots.ForEach(x => snapshotsInfo.Add(new SnapshotInfo
            {
                SnapshotId = x.SnapshotId,
                BackupName = _backupName,
                DeviceName = x.Tag.Get("DeviceName"),
                ServerName = x.Tag.Get("ServerName"),
                Drive = x.Tag.Get("Drive"),
                TimeStamp = x.Tag.Get("TimeStamp"),
            }));

            snapshotsInfo = snapshotsInfo.OrderByDescending(x => Convert.ToDateTime(x.TimeStamp)).ToList();
            return snapshotsInfo;
        }

        public List<SnapshotInfo> GetSnapshots()
        {
            var newest = _snapshotsInfo.Max(x => Convert.ToDateTime(x.TimeStamp));
            return _snapshotsInfo.Where(x => Convert.ToDateTime(x.TimeStamp) == newest).ToList();
        }

        public List<SnapshotInfo> GetSnapshots(string timeStamp)
        {
            return _snapshotsInfo.Where(x => Convert.ToDateTime(x.TimeStamp) == Convert.ToDateTime(timeStamp)).ToList();
        }

        public void StartRestore()
        {
            Logger.Info("Starting Restore" ,"StartRestore");
            Logger.Info("Backup Name:" + _backupName, "StartRestore");
            var snapshotInfo = GetSnapshots("Thu, 26 Dec 2013 01:18:28 GMT");
            snapshotInfo.ForEach( x => Logger.Info(x.ToString(),"StartRestore"));
        }

    }
}
