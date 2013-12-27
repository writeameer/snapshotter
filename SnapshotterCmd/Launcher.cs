using System;
using System.Linq;
using Cloudoman.AwsTools.Snapshotter;
using Cloudoman.AwsTools.Snapshotter.Models;

namespace Cloudoman.AwsTools.SnapshotterCmd
{
	public class Launcher
	{
	    readonly string _backupName;

	    public Launcher(string backupName)
	    {
            _backupName = backupName;
	    }

	    public void StartRestore()
	    {
            //var restoreManager = new RestoreManager(new RestoreRequest());
            //restoreManager.GetAllSnapshots();
	    }

        public void StartBackup()
        {
            var backupManager = new BackupManager(_backupName);
            backupManager.StartBackup();
        }

        public void List()
        {
            //var restoreManager = new RestoreManager(new RestoreRequest());
            //var snapshotsInfo = restoreManager.GetAllSnapshots().ToArray();


            //if (snapshotsInfo.Length == 0)
            //{
            //    Console.WriteLine();
            //    Console.WriteLine("No snapshots were found with the AWS resource tag:{0}\nPlease note that backup names are case-sensitive.\n", _backupName);
            //    return;
            //}

            //var heading = String.Format("|{0,-30}|{1,-10}|{2,-10}|{3,-10}|{4,-10}|{5,-15}|", 
            //    "TimeStamp", "BackupName", "Device", "Drive", "ServerName", "SnapshotId");

            //Console.WriteLine( " " + new String('-',90) + " ");
            //Console.WriteLine(heading);
            //Console.WriteLine("|" + new String('-', 90) + "|");

            //snapshotsInfo.OrderBy(x => x.TimeStamp).ToList()
            //.ForEach(x =>
            //{
            //    var output = String.Format("|{0,-30}|{1,-10}|{2,-10}|{3,-10}|{4,-10}|{5,-15}|", 
            //        x.TimeStamp, x.BackupName, x.DeviceName, x.Drive, x.ServerName, x.SnapshotId);
            //    Console.WriteLine(output);
            //});

            //Console.WriteLine();
        }
    }




}
