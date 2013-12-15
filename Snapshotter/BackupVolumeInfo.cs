using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CloudomanUtils
{
    public class VolumeInfo
    {
        public string VolumeId { get; set; }
        public string Device { get; set; }
        public string Drive { get; set; }
        public string TimeStamp { get; set; }
        public string AwsStartTime { get;set;}
    }
}
