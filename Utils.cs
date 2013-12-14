using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.EC2.Model;
using Amazon.EC2;
using Amazon;
using System.Net;

namespace CloudomanUtils
{
    public class Utils
    {
        static string _instanceId;
        static string _ec2Region;

        static public string InstanceId
        {
            get
            {
                if (_instanceId == null) _instanceId = new WebClient().DownloadString("http://169.254.169.254/latest/meta-data/instance-id");
                return _instanceId;
            }
        }

        static public string Ec2Region
        {
            get
            {
                if (_ec2Region != null) return _ec2Region;
                _ec2Region = (new WebClient()).DownloadString("http://169.254.169.254/latest/meta-data/placement/availability-zone");
                _ec2Region = "https://ec2." + _ec2Region.Remove(_ec2Region.Length - 1) + ".amazonaws.com";
                return _ec2Region;
            }
        }



        public static string GetServerTag(AmazonEC2 ec2Client, string tagName)
        {
            var filters = new List<Filter>{
                new Filter {
                    Name = "resource-type",
                    Value = new List<string> { "instance" }
                },

                new Filter {
                    Name = "resource-id",
                    Value = new List<string> { InstanceId }
                },

                new Filter{
                    Name = "key",
                    Value = new List<string> { tagName }
                }
            };

            var tags = ec2Client.DescribeTags(new DescribeTagsRequest { Filter = filters}).DescribeTagsResult.ResourceTag;

            return tags.Count == 0 ? null : tags[0].Value;
        }

 

        public static List<Volume> GetMyVolumes(AmazonEC2 ec2Client)
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

            var volumes = ec2Client.DescribeVolumes(request).DescribeVolumesResult.Volume;

            if (volumes.Count != 0) return volumes;
            Logger.Info("No attached volumes were found", "GetMyVolumes");
            return null;
        }

        public static List<Snapshot> GetMySnapshots(AmazonEC2 ec2Client, string serverName)
        {
            var filters = new List<Filter> {
                new Filter
                {
                    Name = "tag-key",
                    Value = new List<string> { "ServerName" }
                },
                new Filter
                {
                    Name = "tag-value",
                    Value = new List<string> { serverName }
                }
            };

            var request = new DescribeSnapshotsRequest { Filter = filters };
            var snapshots = ec2Client.DescribeSnapshots(request).DescribeSnapshotsResult.Snapshot;

            if (snapshots.Count != 0) return snapshots;
            Logger.Info("No snapshots found", "GetMySnapshots");
            return null;
        }
    }
}
