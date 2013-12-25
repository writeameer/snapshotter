using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cloudoman.AwsTools.Helpers;
using Cloudoman.AwsTools.Models;
using Amazon.EC2.Model;

namespace Cloudoman.AwsTools
{
    public class RestoreManager
    {
        private string _backupName;
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
            var snapshots = Utils.Ec2Client.DescribeSnapshots(request).DescribeSnapshotsResult.Snapshot;

            var snapshotsInfo = new List<SnapshotInfo>();
            snapshots.ForEach(x => snapshotsInfo.Add(new SnapshotInfo
            {
                SnapshotId = x.SnapshotId,
                BackupName = _backupName,
                Device = x.Tag.Get("Device"),
                ServerName = x.Tag.Get("ServerName"),
                Drive = x.Tag.Get("Drive"),
                TimeStamp = x.Tag.Get("TimeStamp"),
            }));
            return snapshotsInfo;
        }

    }
}
