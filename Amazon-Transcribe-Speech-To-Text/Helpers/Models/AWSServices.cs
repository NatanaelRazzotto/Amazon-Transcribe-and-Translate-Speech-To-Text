using Amazon;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Amazon.S3;
using Amazon.TranscribeService;
using Amazon.TranscribeService.Model;
using Amazon_Transcribe_Speech_To_Text.Helpers.Interface;
using System;
using System.Net;
using System.Threading.Tasks;

namespace AWS_Rekognition_Objects.Helpers.Model
{
    public class AWSServices
    {
        AWSCredentials awsCredentials;
        AmazonS3Client s3Client;
        IController controller;
        private static readonly RegionEndpoint region = RegionEndpoint.USEast1;      

        public AWSServices(IController controller) {
           // bucketNameInput = "unibrasil-transcriberazz-input";
            //JobName = "Transcribe-Reuniao-20210917100919.json";
            this.controller = controller;
        }
           
        private bool GetCredentialsAWS()//
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
           
    }
}
