using Cloudoman.AwsTools.Snapshotter.Models;
using System;
using System.Management;
using System.Linq;
using System.Collections.Generic;
using Amazon.EC2.Model;
namespace Cloudoman.AwsTools.Snapshotter.Helpers
{
    public static class AwsDevices
    {
         
        public static readonly IEnumerable<AwsDeviceMapping> AwsDeviceMappings;
        static AwsDevices()
        {
            AwsDeviceMappings = GetAwsDeviceMapping();
        }

        static int GetScsiTargetId(string awsDevice)
        {

            // AWS Maps devices to SCSITargetId like this:
            // AWS Device| Location (Windows Disk Property)
            // xvdb | Target ID 1
            // xvdc | Target ID 2
            // xvdd | Target ID 3

            if (awsDevice == "/dev/sda1") return 0;
            var ScsiId = awsDevice[awsDevice.Length - 1];

            return (ScsiId - 97);
        }

        static int GetPhysicalDisk(string awsDevice)
        {
            var scsiTargetId = GetScsiTargetId(awsDevice);

            var query = new SelectQuery("Select DeviceId From Win32_DiskDrive where SCSITargetId =" + scsiTargetId);
            var searcher = new ManagementObjectSearcher(query);
            var collection = searcher.Get();

            int disk = 0;
            foreach (var item in collection)
            {
                var deviceId = item["DeviceId"].ToString();

                // The WMI field deviceID normally looks like "\\.\PHYSICALDRIVE2"
                // Extract the disk number only
                disk = int.Parse(deviceId.Replace(@"\\.\PHYSICALDRIVE", ""));
            }

            return disk;
        }

        /// <summary>
        /// Returns the Windows Physical Disk Number attached to a given AWS Ebs Volume
        /// </summary>
        /// <param name="volume">EBS Volume ID</param>
        /// <returns></returns>
        static int GetDiskFromAwsVolume(Volume volume)
        {
            var device = volume.Attachment[0].Device;

            // AWS is inconsistent. Responds with "/dev/sda1" for root awsDevice
            // but with only "xvdf" or "xvdg" for other devices
            // Prefixing all with  "/dev/" for consistency
            device = device.Contains("/dev/") ? device : "/dev/" + device;

            // Get Windows DiskInfo(DeviceId) from AWSDevice(SCSITargetId) Win32_DiskDrive WMI counter
            var scsiTargetId = GetScsiTargetId(device);
            var query = new SelectQuery("Select DeviceId From Win32_DiskDrive where SCSITargetId ="+ scsiTargetId);
            var searcher = new ManagementObjectSearcher(query);
            var collection = searcher.Get();

            int disk=0;
            foreach (var item in collection)
            {
                var deviceId = item["DeviceId"].ToString();

                // The WMI field deviceID normally looks like "\\.\PHYSICALDRIVE2"
                // Extract the disk number only
                disk = int.Parse(deviceId.Replace(@"\\.\PHYSICALDRIVE",""));
            }

            return disk;
        }


        static IEnumerable<AwsDeviceMapping> GetAwsDeviceMapping()
        {
            var volumes = InstanceInfo.Volumes;
            var diskPart = new DiskTools.DiskPart();
            var awsDeviceMappings = volumes.Select(x => new AwsDeviceMapping
            {
                Device = x.Attachment[0].Device,
                VolumeId = x.VolumeId,
                DiskNumber = GetDiskFromAwsVolume(x),
                VolumeNumber = diskPart.DiskDetail(GetDiskFromAwsVolume(x)).Volume.Num,
                Drive = diskPart.DiskDetail(GetDiskFromAwsVolume(x)).Volume.Letter
            });

            return awsDeviceMappings;
        }

    }


}
