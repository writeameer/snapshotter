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
	}




}
