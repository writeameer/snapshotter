using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Cloudoman.AwsTools.Snapshotter;

namespace Cloudoman.AwsTools.Snapshotter.Tests
{
    [TestClass]
    public class RestoreTests
    {
        [TestMethod]
        public void StartRestore()
        {
            var restoreManager = new RestoreManager();
            restoreManager.StartRestore();


        }
    }
}
