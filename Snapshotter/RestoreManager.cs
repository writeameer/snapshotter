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
        private readonly string _timeStamp;
        private IEnumerable<SnapshotInfo> _snapshotsInfo;

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
            _snapshotsInfo = GetAllSnapshots();
            if (_snapshotsInfo.ToList().Count == 0 )
            {
                var message = "No snapshots were found for BackupName:" + _backupName; 
                Logger.Error(message, "RestoreManager");
                throw new System.ApplicationException(message);
            }

            // Get Snapshot timestamp from Request or default to latest
            _timeStamp = request.TimeStamp ?? (GetLatestSnapshotTimeStamp());
            if (_timeStamp == null)
            {
                var message = "No timestamp was explicitly provided. Unable to determine the timestamp of the latest snapshot. Exitting.";
                Logger.Error(message, "RestoreMenager");
            }

        }


        IEnumerable<SnapshotInfo> GetAllSnapshots()
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
                BackupName = x.Tag.Get("BackupName"),
                DeviceName = x.Tag.Get("DeviceName"),
                ServerName = x.Tag.Get("ServerName"),
                Drive = x.Tag.Get("Drive"),
                TimeStamp = x.Tag.Get("TimeStamp"),
            }));

            snapshotsInfo = snapshotsInfo.OrderByDescending(x => Convert.ToDateTime(x.TimeStamp)).ToList();
            return snapshotsInfo;
        }

        public string GetLatestSnapshotTimeStamp()
        {
            var newest = _snapshotsInfo.Max(x => Convert.ToDateTime(x.TimeStamp));
            return _snapshotsInfo
                        .Where(x => Convert.ToDateTime(x.TimeStamp) == newest)
                        .Select(x => x.TimeStamp).FirstOrDefault();
        }

        public IEnumerable<SnapshotInfo> GetSnapshots()
        {
            var newest = _snapshotsInfo.Max(x => Convert.ToDateTime(x.TimeStamp));
            return _snapshotsInfo.Where(x => Convert.ToDateTime(x.TimeStamp) == newest);
        }

        public IEnumerable<SnapshotInfo> GetSnapshots(string timeStamp)
        {
            return _snapshotsInfo.Where(x => Convert.ToDateTime(x.TimeStamp) == Convert.ToDateTime(timeStamp));
        }

        public void StartRestore()
        {
            Logger.Info("Starting Restore", "RestoreManager.StartRestore");
            Logger.Info("Backup Name:" + _backupName, "RestoreManager.StartRestore");

            // Find Snapshots to Restore
            var snapshots = GetSnapshots();
            snapshots.ToList().ForEach(x => Logger.Info(x.ToString(), "RestoreManager.StartRestore"));
        }

        public void List()
        {
            Logger.Info("Listing Snaphshots", "RestoreManager.List");
            Logger.Info("Backup Name:" + _backupName, "RestoreManager.List");
            Console.WriteLine(new SnapshotInfo().FormattedHeader);
            _snapshotsInfo.ToList().ForEach(Console.WriteLine);
        }

    }
}
