using System;
using System.Linq;
using Cloudoman.DiskTools;
using NUnit.Framework;

namespace Cloudoman.AwsTools.Test
{
    public class DiskPartTests
    {
        private readonly DiskPart _diskPart;

        public DiskPartTests()
        {
            _diskPart = new DiskPart();
        }

        [Test]
        public void ListDisks()
        {
            var disks = _diskPart.ListDisk();
            disks.ToList().ForEach(x =>
            {
                Console.WriteLine("Num:" + x.Num);
                Console.WriteLine("Status:" + x.Status);
                Console.WriteLine("Size:" + x.Size);
                Console.WriteLine("Free:" + x.Free);
                Console.WriteLine("Dyn:" + x.Dyn);
                Console.WriteLine("Gpt:" + x.Gpt);

            });
            
        }

        [Test]
        public void ListVolumes()
        {
            var volumes = _diskPart.ListVolume();
            volumes.ToList().ForEach(x =>
            {
                Console.WriteLine("Num:" + x.Num);
                Console.WriteLine("Letter:" + x.Letter);
                Console.WriteLine("Label:" + x.Label);
                Console.WriteLine("FileSystem:" + x.FileSystem);
                Console.WriteLine("Type:" + x.Type);
                Console.WriteLine("Size:" + x.Size);
                Console.WriteLine("Status:" + x.Status);
                Console.WriteLine("Info:" + x.Info);
                Console.WriteLine("-------------------------------");
            });
            
        }

        [Test]
        public void OnlineDisk()
        {
            var firstDisk = _diskPart.ListDisk().FirstOrDefault();
            var status = firstDisk != null && _diskPart.OnlineDisk(firstDisk.Num);
            Assert.True(status);
        }

        [Test]
        public void AssignDriveLetter()
        {
            var bootVolume = _diskPart.ListVolume().FirstOrDefault(x => x.Info == "Boot");
            var status = bootVolume != null && _diskPart.AssignDriveLetter(bootVolume.Num, bootVolume.Letter);
            Assert.False(status);
        }
    }
}
