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
            var query = new SelectQuery("Select DeviceId,SCSITargetId From win32_DiskDrive where SCSITargetId < 16");
            var searcher = new ManagementObjectSearcher(query);
            var collection = searcher.Get();

            foreach (var item in collection)
            {
                var diskId = item["DeviceId"].ToString().Replace(@"\\.\PHYSICALDRIVE","");
                var awsDeviceId = GetDeviceFromScsiId(int.Parse(item["SCSITargetId"].ToString()));
                Console.WriteLine("{0} {1}", diskId, awsDeviceId);
            }
            return null;
        }
    }


}
