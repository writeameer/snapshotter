using System;
using System.Diagnostics;
using System.Linq;
using Cloudoman.DiskPart;

namespace CloudomanUtils
{
    public class ScratchPad
    {
        public ScratchPad()
        {
            var diskPart = new DiskPart();
            var output = diskPart.RunCommand("list disk");
            output.ToList().ForEach(Console.WriteLine);
   
            

        }
    }
}
