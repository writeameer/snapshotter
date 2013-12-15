using System;

namespace Cloudoman.Diskpart
{
    public static class Extensions
    {
        public static string NullIfEmpty(this string str)
        {
            return string.IsNullOrEmpty(str) ? "null" : str;
        }
    }
}
