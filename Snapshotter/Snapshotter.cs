using System;
using System.Linq;
using Alphaleonis.Win32.Vss;
using Amazon.EC2;
using Amazon.EC2.Model;
using Amazon.Util;
using Cloudoman.AwsTools.Helpers;

namespace Cloudoman.AwsTools
{
	public class Snapshotter
	{
	    readonly string _backupName;

	    public Snapshotter(string backupName)
	    {
            _backupName = backupName;
	    }

	    public void StartRestore()
	    {
	        var restoreManager = new RestoreManager(_backupName);
	        restoreManager.ListSnapshots();
	    }

        public void StartBackup()
        {
            var backupManager = new BackupManager(_backupName);
            backupManager.StartBackup();
        }

        public void List()
        {
            var restoreManager = new RestoreManager(_backupName);
            restoreManager.ListSnapshots();
            var snapshotsInfo = restoreManager.ListSnapshots();


            var heading = String.Format("|{0,-30}|{1,-10}|{2,-10}|{3,-10}|{4,-10}|{5,-15}|", 
                "TimeStamp", "BackupName", "Device", "Drive", "ServerName", "SnapshotId");
            Console.WriteLine();
            Console.WriteLine(" " + new String('-',90) + " ");
            Console.WriteLine(heading);
            Console.WriteLine("|" + new String('-', 90) + "|");

            snapshotsInfo.OrderBy(x => x.TimeStamp).ToList()
            .ForEach(x =>
            {
                var output = String.Format("|{0,-30}|{1,-10}|{2,-10}|{3,-10}|{4,-10}|{5,-15}|", 
                    x.TimeStamp, x.BackupName, x.DeviceName, x.Drive, x.ServerName, x.SnapshotId);
                Console.WriteLine(output);
            });

            Console.WriteLine();
        }
    }




}
