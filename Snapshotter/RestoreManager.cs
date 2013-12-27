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
                var message = "No snapshots were found for BackupName:" + _backupName + " and timestamp: " + _timeStamp; 
                Logger.Error(message, "RestoreManager");
                throw new System.ApplicationException(message);
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
                ServerName = x.Tag.Get("ServerName"),
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
            return _allSnapshots
                        .Where(x => Convert.ToDateTime(x.TimeStamp) == newest)
                        .Select(x => x.TimeStamp).FirstOrDefault();
        }

        string GetOldestSnapshotTimeStamp()
        {
            var newest = _allSnapshots.Min(x => Convert.ToDateTime(x.TimeStamp));
            return _allSnapshots
                        .Where(x => Convert.ToDateTime(x.TimeStamp) == newest)
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
            Logger.Info("Starting Restore", "RestoreManager.StartRestore");
            Logger.Info("Backup Name:" + _backupName, "RestoreManager.StartRestore");

            // Find Snapshots to Restore
            var snapshots = GetSnapshotSet();
            snapshots.ToList().ForEach(x => Logger.Info(x.ToString(), "RestoreManager.StartRestore"));
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

    }
}
