using System;
using Cloudoman.AwsTools.SnapshotterCmd.Powerargs;
using PowerArgs;


namespace Cloudoman.AwsTools.SnapshotterCmd
{
    class Program
    {
        static void Main(string[] args)
        {
            

            // Create Snapshots
            try
            {
                // Get arguments if any
                var parsed = Args.Parse<MyArgs>(args);
                var operation = parsed.Operation.ToString().ToLower();
                var backupName = parsed.BackupName;

                // Create Snapshotter object 

                var snapShotter = new Launcher(backupName);

                // Run backup or restore
                switch (operation)
                {
                    case "backup":
                        snapShotter.StartBackup();
                        break;
                    case "restore":
                        snapShotter.StartRestore();
                        break;

                    case "list":
                        snapShotter.List();
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


        }
    }
}
