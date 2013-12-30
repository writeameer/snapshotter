using System;
using System.Linq;

namespace Cloudoman.DiskTools
{
    public static class Extensions
    {
        public static string NullIfEmpty(this string str)
        {
            return string.IsNullOrEmpty(str) ? "null" : str;
        }

        public static void Dump(this string[] str)
        {
            str.ToList().ForEach(Console.WriteLine);
        }

        public static bool GetBool(this string[] rawOutput, string key)
        {
            var row = rawOutput.FirstOrDefault(x => x.ToLower().Contains(key.ToLower()));
            if (row != null)
            {
                var info = row;
                info = info.Split(':')[1].Trim();
                return info == "Yes";
            }
            return false;
        }

        public static string GetString(this string[] rawOutput, string key)
        {
            var row = rawOutput.FirstOrDefault(x => x.ToLower().Contains(key.ToLower()));
            if (row != null)
            {
                var info = row;
                info = info.Split(':')[1].Trim();
                return info;
            }
            return "null";
        }

    }
}
