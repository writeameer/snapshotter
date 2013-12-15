using PowerArgs;

namespace CloudomanUtils
{

    public class MyArgs
    {
        [ArgRequired]
        [ArgDescription("Operation is either 'backup' or 'restore'")]
        public Operation Operation { get; set; }
    }
}
