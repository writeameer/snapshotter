using System;
using System.Linq;
using Cloudoman.DiskTools;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cloudoman.AwsTools.Snapshotter.Tests
{
    [TestClass]
    public class DiskPartTests
    {
        private readonly DiskPart _diskPart;

        public DiskPartTests()
        {
            _diskPart = new DiskPart();
        }

        [TestMethod]
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

        [TestMethod]
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

        [TestMethod]
        public void OnlineDisk()
        {
            var firstDisk = _diskPart.ListDisk().FirstOrDefault();
            if (firstDisk == null) Assert.Fail();
            var response = _diskPart.OnlineDisk(firstDisk.Num);
            response.Output.Dump();
            
            Assert.IsTrue(response.Status);
        }

        [TestMethod]
        public void AssignDriveLetter()
        {
            var bootVolume = _diskPart.ListVolume().FirstOrDefault(x => x.Info == "Boot");
            if (bootVolume != null)
            {
                var response = _diskPart.AssignDriveLetter(bootVolume.Num, bootVolume.Letter);
                response.Output.Dump();
                Assert.IsFalse(response.Status);
            }
        }
    }
}
