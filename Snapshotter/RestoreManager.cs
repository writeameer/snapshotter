using System.Collections.Generic;
using Amazon.EC2.Model;
using Cloudoman.AwsTools.Snapshotter.Helpers;
using Cloudoman.AwsTools.Snapshotter.Models;

namespace Cloudoman.AwsTools.Snapshotter
{
    public class RestoreManager
    {
        private readonly string _backupName;
        public RestoreManager(string backupName)
        {
            _backupName = backupName;
        }

        public List<SnapshotInfo> ListSnapshots()
        {
            var filters = new List<Filter> {
                new Filter
                {
                    Name = "tag-key",
                    Value = new List<string> { "BackupName" }
                },
                new Filter
                {
                    Name = "tag-value",
                    Value = new List<string> { _backupName }
                }
            };

            var request = new DescribeSnapshotsRequest { Filter = filters };
            var snapshots = AwsHelper.Ec2Client.DescribeSnapshots(request).DescribeSnapshotsResult.Snapshot;

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
            return snapshotsInfo;
        }

    }
}
