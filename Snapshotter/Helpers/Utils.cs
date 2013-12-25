using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Amazon;
using Amazon.EC2;
using Amazon.EC2.Model;

namespace Cloudoman.AwsTools.Helpers
{
    public class Utils
    {

        static readonly WebClient Web = new WebClient();

        public static readonly AmazonEC2 Ec2Client;
        public static readonly string InstanceId;
        public static readonly string Ec2Region;
        public static readonly string ServerName;

        static Utils()
        {
            InstanceId = GetInstanceId();
            Ec2Region = GetEc2Region();

            var ec2Config = new AmazonEC2Config { ServiceURL = Ec2Region };
            Ec2Client = AWSClientFactory.CreateAmazonEC2Client(ec2Config);
            ServerName = GetInstanceTag("Name");

        }

        static private readonly Func<string> GetInstanceId = () => Web.DownloadString("http://169.254.169.254/latest/meta-data/instance-id");

        static private readonly Func<string> GetEc2Region = () =>
        {
            var availabilityZone =
                Web.DownloadString("http://169.254.169.254/latest/meta-data/placement/availability-zone");
            return "https://ec2." + availabilityZone.Remove(availabilityZone.Length - 1) + ".amazonaws.com";
        };

        public static readonly Func<string,string> GetInstanceTag = (x) =>
        {
            var filters = new List<Filter>
            {
                new Filter {Name = "resource-type", Value = new List<string> {"instance"}},
                new Filter {Name = "resource-id", Value = new List<string> {InstanceId}},
                new Filter
                {
                    Name = "key",
                    Value = new List<string> {x}
                }
            };
            var tags = Ec2Client.DescribeTags(new DescribeTagsRequest {Filter = filters}).DescribeTagsResult.ResourceTag;
            return tags.Count == 0 ? null : tags[0].Value;
        };

   
        public static List<Volume> GetMyVolumes()
        {
            // Find volumes attached to me
            var filter = new Filter
            {
                Name = "attachment.instance-id",
                Value = new List<string> { InstanceId }
            };

            var request = new DescribeVolumesRequest
            {
                Filter = new List<Filter> { filter }
            };

            var volumes = Ec2Client.DescribeVolumes(request).DescribeVolumesResult.Volume;

            if (volumes.Count != 0) return volumes;
            Logger.Info("No attached volumes were found", "GetMyVolumes");
            return null;
        }

        public static List<Snapshot> GetMySnapshots(AmazonEC2 ec2Client, string backupName)
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
                    Value = new List<string> { backupName }
                }
            };

            var request = new DescribeSnapshotsRequest { Filter = filters };
            var snapshots = ec2Client.DescribeSnapshots(request).DescribeSnapshotsResult.Snapshot;

            if (snapshots.Count != 0) return snapshots;
            Logger.Info("No snapshots found", "GetMySnapshots");
            return null;
        }

        public static IEnumerable<string> GetFreeDevices(AmazonEC2 ec2Client)
        {
            // All devices are xvdf-xvdp
            var allDevices = Enumerable.Range('f', 'p' - 'f' + 1).Select(x => "xvd" + (char)x);

            var request = new DescribeInstancesRequest {InstanceId = new List<string>{InstanceId}};

            return allDevices.Except(
                    ec2Client.DescribeInstances(request)
                        .DescribeInstancesResult
                        .Reservation.First()
                        .RunningInstance.First()
                        .BlockDeviceMapping
                        .Select(x => x.DeviceName)
            );
        }

    }
}
