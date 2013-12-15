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
                           Label = x.Substring(19, 11).NullIfEmpty(),
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
                       .Select(x => x.Replace("Disk", ""))
                       .Select(x => Regex.Replace(x, @"[\s](?<size>[A-Z]*B)", match => match.Groups["size"].Value))
                       .Select(x => x.TrimStart())
                       .Select(x => Regex.Replace(x, @"[ ]{1,}", ","))
                       .Select(x => x.Split(','))
                       .Select(x => new Disk
                       {
                           Num = int.Parse(x[0]),
                           Status = x[1],
                           Size = x[2],
                           Free = x[3]
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
