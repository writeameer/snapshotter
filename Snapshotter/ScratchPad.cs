using System;
using System.Linq;
using Amazon;
using Amazon.EC2;
using Cloudoman.AwsTools.Helpers;
using Cloudoman.DiskTools;

namespace Cloudoman.AwsTools
{
    public class ScratchPad
    {
        public ScratchPad()
        {

            var snapshotter = new AwsTools.Snapshotter("web.prod");

            snapshotter.List();

            Environment.Exit(0);

            var ec2Config = new AmazonEC2Config { ServiceURL = Utils.Ec2Region };
            var ec2Client = AWSClientFactory.CreateAmazonEC2Client(ec2Config);

            var devices = Utils.GetFreeDevices(ec2Client);
            devices.ToList().ForEach(Console.WriteLine);

            
            Environment.Exit(0);

            var diskPart = new DiskPart();
            var disks = diskPart.ListDisk();

            //diskPart.OnlineDisk(2);
            diskPart.AssignDriveLetter(3, "H");

            


            
        }

    }

}
