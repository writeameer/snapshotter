using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CloudomanUtils
{
    public class DiskPart
    {
        public void ListDisks() {

        }

        public void RunCommand()
        {
            var pi = new System.Diagnostics.ProcessStartInfo();
            pi.FileName = @"C:\Windows\system32\diskpart.exe";

        }
    }
}
