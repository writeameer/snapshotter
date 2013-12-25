using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.EC2.Model;

namespace Cloudoman.AwsTools
{
    public static class Extensions
    {

        /// <summary>
        /// Returns the value of the tag with maching key
        /// </summary>
        /// <param name="tags"></param>
        /// <param name="key">Name of the Tag's Key For e.g "Date"</param>
        /// <returns></returns>
        public static string Get(this List<Tag> tags, string key)
        {
            return tags.Where(t => t.Key == key).Select(d => d.Value).FirstOrDefault();
        }
    }
}
