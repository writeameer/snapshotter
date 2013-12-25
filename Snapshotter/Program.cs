using System;
using Amazon.EC2.Model;
using Cloudoman.AwsTools.Helpers;
using Cloudoman.AwsTools.Powerargs;
using PowerArgs;

namespace Cloudoman.AwsTools
{
    class Program
    {
        static void Main(string[] args)
        {

            Logger.Info("Job Started", "main");

            // Create Snapshots
            try
            {
                // Get arguments if any
                var parsed = Args.Parse<MyArgs>(args);
                var operation = parsed.Operation.ToString().ToLower();
                var backupName = parsed.BackupName;

                // Create Snapshotter object 

                var snapShotter = new Snapshotter(backupName);

                // Run backup or restore
                switch (operation)
                {
                    case "backup":
                        snapShotter.StartBackup();
                        break;
                    case "restore":
                        snapShotter.StartRestore();
                        break;
                }

            }
            catch (Exception ex)
            {
                if (ex is UnexpectedArgException || ex is MissingArgException)
                    ArgUsage.GetStyledUsage<MyArgs>().Write();
                else
                    Logger.Error(ex.ToString(), "main");
            }


            Logger.Info("Job Ended", "main");
		}

    }
}
