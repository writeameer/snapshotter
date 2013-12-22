using PowerArgs;

namespace CloudomanUtils
{

    public class MyArgs
    {
        [ArgRequired]
        [ArgDescription("Operation is either 'backup' or 'restore'")]
        public Operation Operation { get; set; }

        [ArgRequired]
        [ArgDescription("A name for your backup. For e.g. ProdDatabase. This name is used to tag your snapshots.")]
        public string BackupName { get; set; }
    }
}
