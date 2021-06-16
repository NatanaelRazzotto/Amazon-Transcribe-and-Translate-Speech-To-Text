using Amazon.SQS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amazon_Transcribe_Speech_To_Text.Helpers.Models.AWServices
{
    public class AWSSQS
    {
        public void execute() {
            var sqsConfig = new AmazonSQSConfig();
            sqsConfig.ServiceURL = "http://sqs.us-west-2.amazonaws.com";
            var sqsClient = new AmazonSQSClient(sqsConfig);
        }

    }
}
