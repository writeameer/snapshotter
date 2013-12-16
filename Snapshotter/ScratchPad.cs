using System;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using Amazon.EC2.Model;
using Cloudoman.DiskPart;

namespace CloudomanUtils
{
    public class ScratchPad
    {
        public ScratchPad()
        {


            var allDevices = Enumerable.Range('f', 'p'-'f' +1 ).Select(x => "xvd" + (char) x);
            var usedDevices = new[] {"xvdf", "xvdg","svdp"};

            var a = new BlockDeviceMapping();
            
            var freeDevices = allDevices.Except(usedDevices);

            freeDevices.ToList().ForEach(Console.WriteLine);
            Environment.Exit(0);

            var diskPart = new DiskPart();
            var disks = diskPart.ListDisk();

            //diskPart.OnlineDisk(2);
            diskPart.AssignDriveLetter(3, "H");

            
            disks.ToList().ForEach(x =>
            {
                Console.WriteLine("Num:" + x.Num);
                Console.WriteLine("Status:"+ x.Status);
                Console.WriteLine("Size:" + x.Size);
                Console.WriteLine("Free:" + x.Free) ;
                Console.WriteLine("Dyn:"+ x.Dyn);
                Console.WriteLine("Gpt:" + x.Gpt);

            });

            var volumes = diskPart.ListVolume();
            volumes.ToList().ForEach(x =>
            {
                Console.WriteLine("Num:" + x.Num);
                Console.WriteLine("Letter:"+ x.Letter);
                Console.WriteLine("Label:" + x.Label);
                Console.WriteLine("FileSystem:" + x.FileSystem) ;
                Console.WriteLine("Type:"+ x.Type);
                Console.WriteLine("Size:" + x.Size);
                Console.WriteLine("Status:" + x.Status) ;
                Console.WriteLine("Info:" + x.Info) ;
                Console.WriteLine("-------------------------------");
            });

            
        }

    }

}
