using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Cloudoman.AwsTools.Snapshotter;
using Cloudoman.AwsTools.Snapshotter.Models;


namespace Cloudoman.AwsTools.Snapshotter.Tests
{
    [TestClass]
    public class RestoreTests
    {
        [TestMethod]
        public void ListSnapshots()
        {
            var request = new RestoreRequest();
            request.BackupName = "web";
            var restoreManager = new RestoreManager(request);
            restoreManager.List();

        }
    }
}
