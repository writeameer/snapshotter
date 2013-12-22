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
    }
}
