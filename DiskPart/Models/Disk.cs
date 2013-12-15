using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cloudoman.Diskpart.Models
{
    public class Disk
    {
        public int Num { get; set; }
        public string Status { get; set; }
        public string Size { get; set; }
        public string Free { get; set; }
        public string Dyn { get; set; }
        public string Gpt { get; set; }
    }
}
