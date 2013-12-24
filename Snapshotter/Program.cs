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

            Func<string> getInstanceId = () => "ameer";

            var b = getInstanceId;

            Console.WriteLine(b);

            Environment.Exit(0);

            var a = Utils.Ec2Client;

            var something = a.DescribeKeyPairs(new DescribeKeyPairsRequest());
            something.DescribeKeyPairsResult.KeyPair.ForEach(x => Console.WriteLine(x.KeyName));

            Environment.Exit(0);

            Logger.Info("Job Started", "main");

            // Create Snapshots
            try
            {
                // Get arguments if any
                var parsed = Args.Parse<MyArgs>(args);
                var operation = parsed.Operation.ToString().ToLower();
                var backupName = parsed.BackupName;

                // Create Snapshotter object 

                var snapShotter = new AwsTools.Snapshotter(backupName);

                // Run backup or restore
                switch (operation)
                {
                    case "backup":
                        snapShotter.DoBackup();
                        break;
                    case "restore":
                        snapShotter.DoRestore();
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
