using PowerArgs;

namespace CloudomanUtils.powerargs
{

    public class MyArgs
    {

        [DefaultValue("backup")]
        [ArgDescription("EC2 Region Endpoint. Check http://docs.aws.amazon.com/general/latest/gr/rande.html for a full listing")]
        public Ec2Region Action { get; set; }


        [DefaultValue("ec2.us-east-1.amazonaws.com")]
        [ArgDescription("EC2 Region Endpoint. Check http://docs.aws.amazon.com/general/latest/gr/rande.html for a full listing")]
        public Ec2Region ServiceEndpoint { get; set; }


		[DefaultValue("Automated Snapshot Tool")]
		[ArgDescription("Description for the Amazon EBS snapshot")]
		public string SnapshotDescription { get; set; }
    }
}
