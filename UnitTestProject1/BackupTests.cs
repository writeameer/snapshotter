﻿using System;
using System.Linq;
using Cloudoman.AwsTools.Snapshotter.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Cloudoman.AwsTools.Snapshotter.Models;

namespace Cloudoman.AwsTools.Snapshotter.Tests
{
    [TestClass]
    public class BackupTests
    {
        [TestMethod]
        public void BackupVolumes()
        {
            var backupRequest = new BackupRequest { BackupName = "ProdWeb" };
            var backupManager = new BackupManager(backupRequest);
            backupManager.StartBackup();
        }

        [TestMethod]
        public void RestoreVolumes()
        {
            //var rm = new RestoreManager(new RestoreRequest());
            //var snapshotsInfo = rm.GetAllSnapshots();
            //var restoreSet = snapshotsInfo.Where(x => x.TimeStamp == "Wed, 25 Dec 2013 04:45:31 GMT");
            //restoreSet.ToList().ForEach(x =>
            //{
            //    Console.WriteLine(x.BackupName);
            //    Console.WriteLine(x.DeviceName);
            //    Console.WriteLine(x.Drive);
            //    Console.WriteLine(x.ServerName);
            //    Console.WriteLine(x.SnapshotId);
            //    Console.WriteLine(x.TimeStamp);
            //});

        }

        [TestMethod]
        public void GetScsiTargets()
        {
            AwsDevices.GetScsiId();
        }

        [TestMethod]
        public void GetServerName()
        {
            Console.WriteLine(InstanceInfo.ServerName);
        }

        [TestMethod]
        public void GetWindowsHostName()
        {
            Console.WriteLine(System.Net.Dns.GetHostName());
        }

        [TestMethod]
        public void GetVolumetoDeviceMapping()
        {
            var bm = new BackupManager(new BackupRequest());
            bm.GetDrivetoAwsDeviceMapping();
        }
    }
}
