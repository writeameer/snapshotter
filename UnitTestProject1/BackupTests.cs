using System;
using Cloudoman.AwsTools;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestProject1
{
    [TestClass]
    public class BackupTests
    {
        [TestMethod]
        public void BackupVolumes()
        {
            var snapShotter = new Snapshotter("ProdWeb");
            snapShotter.StartBackup();
        }

        [TestMethod]
        public void RestoreVolumes()
        {
            var snapShotter = new Snapshotter("ProdWeb");
            snapShotter.StartRestore();
        }
    }
}
