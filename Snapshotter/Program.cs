using System;
using PowerArgs;

namespace CloudomanUtils
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

                var operation = parsed.Operation.ToString();
                var backupName = parsed.BackupName;

                // Create Snapshotter object 

                var snapShotter = new Snapshotter(backupName);

                // Run backup or restore
                if (operation == "backup")
                    snapShotter.DoBackup();
                else
                {
                    //snapShotter.DoRestore();
                }

            }
            catch (Exception ex)
            {
                if (ex is ArgumentException || ex is UnexpectedArgException || ex is MissingArgException)
                    ArgUsage.GetStyledUsage<MyArgs>().Write();
                else
                    Logger.Error(ex.ToString(), "main");
            }


            Logger.Info("Job Ended", "main");
		}

    }
}
