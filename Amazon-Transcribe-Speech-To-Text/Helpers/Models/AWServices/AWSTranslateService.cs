using Amazon;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Amazon.Translate;
using Amazon_Transcribe_Speech_To_Text.Helpers.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amazon_Transcribe_Speech_To_Text.Helpers.Models.AWServices
{
    class AWSTranslateService : AWSService
    {
        private AWSCredentials awsCredentials;
        private AmazonTranslateClient transcribeClient;
        private static readonly RegionEndpoint region = RegionEndpoint.USEast1;
        private IController controller;
        public bool GetCredentialsAWS()
        {
            CredentialProfileStoreChain credentialProfileChain = new CredentialProfileStoreChain();
            if (credentialProfileChain.TryGetAWSCredentials("AWS-Educate-profileD", out awsCredentials))
            {
                return true;
            }
            else
            {
                throw new ApplicationException("Erro obtendo credenciais");
            }
        }
        private bool TranscribeClient()
        {
            if (GetCredentialsAWS())
            {
                transcribeClient = new AmazonTranslateClient(awsCredentials, region);
                return true;
            }
            return false;
        }
    }
}
