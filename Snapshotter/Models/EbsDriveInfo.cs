using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cloudoman.AwsTools.Snapshotter.Models
{
    public class EbsDriveInfo
    {
        public string Drive { get; set; }


        public string VolumeNumber { get; set; }
        public string DiskNumber { get; set; }

        /// <summary>
        /// The VolumeId of the EBS Volume attached as a drive
        /// </summary>
        public string VolumeId { get; set; }

        /// <summary>
        /// The AWS DeviceName used to attach the EBS Volume. For e.g. /dev/xvdh
        /// </summary>
        public string DeviceName { get; set; }
    }
}
