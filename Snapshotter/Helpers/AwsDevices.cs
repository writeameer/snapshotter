using Cloudoman.AwsTools.Snapshotter.Models;
using System;
using System.Management;
using System.Linq;
using System.Collections.Generic;
namespace Cloudoman.AwsTools.Snapshotter.Helpers
{
    public static class AwsDevices
    {
         
        static EbsDriveInfo ebsDriveInfo;
        static Dictionary<string,string> DeviceToDrive= new Dictionary<string,string>();

        static AwsDevices()
        {
            var volume = InstanceInfo.GetMyVolumes();
            var a = volume.ForEach(x => x.Attachment.Aggregate(d => d.Device));

            
        }

        public static string GetAwsDeviceFromScsiTargetId(int id)
        {
            if (id == 0) return "/dev/sda1";
            return "/dev/xvd" + (char)(id + 97);
        }

        
        public static string GetScsiId()
        {
            var query = new SelectQuery("Select DeviceId,SCSITargetId From win32_DiskDrive where SCSITargetId < 16");
            var searcher = new ManagementObjectSearcher(query);
            var collection = searcher.Get();

            foreach (var item in collection)
            {
                var diskId = item["DeviceId"].ToString().Replace(@"\\.\PHYSICALDRIVE","");
                var awsDeviceId = GetAwsDeviceFromScsiTargetId(int.Parse(item["SCSITargetId"].ToString()));
                Console.WriteLine("{0} {1}", diskId, awsDeviceId);
            }
            return null;
        }

        public static string GetDriveFromVolumeId(VolumeInfo volumeInfo)
        {
            
            var diskPart = new DiskTools.DiskPart();
            var volume = diskPart.ListVolume().Where(x => x.Letter == volumeInfo.Drive).FirstOrDefault();
            var windowsDisk = diskPart.VolumeDetail(volume.Num).Disk.Num;



            //var volumeDetail = diskPart.VolumeDetail();

            return volumeInfo.DeviceName;
        }

    }


}
