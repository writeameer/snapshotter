using NUnit.Framework;

namespace Cloudoman.AwsTools.Test
{
    class SnapshoterTests
    {
        private readonly Snapshotter _snapshotter;
        public SnapshoterTests()
        {
            _snapshotter = new Snapshotter("web.prod");
        }

        [Test]
        public void ListSnapshots()
        {
            _snapshotter.List();
        }
    }
}
