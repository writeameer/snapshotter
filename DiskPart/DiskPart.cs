using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Cloudoman.Diskpart;
using Cloudoman.DiskPart.Commands;
using Cloudoman.Diskpart.Models;

namespace Cloudoman.DiskPart
{
    public class DiskPart
    {
        readonly ProcessStartInfo _psInfo = new ProcessStartInfo
        {
            FileName = @"c:\windows\System32\WindowsPowerShell\v1.0\powershell.exe",
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false
        };


        public IEnumerable<Volume> ListVolume()
        {
            var rawOutput = RunCommand("list volume");
            var count = rawOutput.Count();

            return rawOutput
                       .Skip(5) // Skip lines from top
                       .Take(count - 5 - 1) //Take All except last line
                       .Where(x => !x.Contains("---")) // Ignore header formatting
                       .Select(x => new Volume
                       {
                           Num = int.Parse(x.Substring(2,10).Split(' ')[1]),
                           Letter = x.Substring(14,3).Trim().NullIfEmpty(),
                           Label = x.Substring(19, 11).Trim().NullIfEmpty(),
                           FileSystem = x.Substring(32, 4).Trim().NullIfEmpty(),
                           Type = x.Substring(39, 10).Trim().NullIfEmpty(),
                           Size = x.Substring(51, 7).Trim().NullIfEmpty(),
                           Status = x.Substring(60, 9).Trim().NullIfEmpty(),
                           Info = x.Substring(69, 8).Trim().NullIfEmpty()
                       });
        }

        public IEnumerable<Disk> ListDisk()
        {
            var rawOutput = RunCommand("list disk");
            var count = rawOutput.Count();

            return rawOutput
                       .Skip(5) // Skip lines from top
                       .Take(count - 5 - 1) //Take All except last line
                       .Where(x => !x.Contains("---")) // Ignore line header formatting
                       .Select(x => new Disk
                       {
                           Num = int.Parse(x.Substring(2, 8).Split(' ')[1]),
                           Status = x.Substring(12,13).Trim().NullIfEmpty(),
                           Size = x.Substring(27,7).Replace(" ","").Trim().NullIfEmpty(),
                           Free = x.Substring(36,7).Replace(" ","").Trim().NullIfEmpty(),
                           Dyn = x.Substring(45,3).Trim().NullIfEmpty(),
                           Gpt = x.Substring(50,2).Trim().NullIfEmpty()
                       });
        }

        public string[] RunCommand(string command)
        {
            var cmd = "\"Write-Output \"\"" + command + "\"\"\" | diskpart";
            var arguments = String.Format(" -command {0}", cmd);
            _psInfo.Arguments = arguments;

            // Get ouput from diskpart
            var process = Process.Start(_psInfo);
            process.WaitForExit();

            var rawOutput = 
                process.StandardOutput.ReadToEnd()
                       .Split(new [] {"\n", "\r\n"}, StringSplitOptions.RemoveEmptyEntries);

            Console.WriteLine("-----------------------------");
            rawOutput.ToList().ForEach(Console.WriteLine);
            Console.WriteLine("-----------------------------\n\n");
            return rawOutput;
        }
    }
}
