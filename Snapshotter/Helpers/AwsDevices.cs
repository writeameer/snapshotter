using System;
using System.Management;

namespace Cloudoman.AwsTools.Snapshotter.Helpers
{
    public static class AwsDevices
    {
        public static string GetDeviceFromScsiId(int id)
        {
            if (id == 0) return "/dev/sda1";
            return "/dev/xvd" + (char)(id + 97);
        }

        public static string GetScsiId()
        {
            var query = new SelectQuery("Select DeviceId,SCSITargetId From win32_DiskDrive");
            var searcher = new ManagementObjectSearcher(query);
            var collection = searcher.Get();

            foreach (var item in collection)
            {
                var deviceId = item["DeviceId"].ToString();
                var scsiTargetId = item["SCSITargetId"].ToString();
                Console.WriteLine("{0} {1}",deviceId, scsiTargetId);
            }
            return null;
        }
    }


}
