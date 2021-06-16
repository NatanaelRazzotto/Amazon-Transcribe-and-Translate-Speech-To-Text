using Amazon;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Amazon.TranscribeService;
using Amazon.TranscribeService.Model;
using Amazon_Transcribe_Speech_To_Text.Helpers.Interface;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Amazon_Transcribe_Speech_To_Text.Helpers.Models.AWServices
{
    public class AWSTranscribeService : AWSService
    {
        private AWSCredentials awsCredentials;
        private AmazonTranscribeServiceClient transcribeClient;
        private static readonly RegionEndpoint region = RegionEndpoint.USEast1;
        private IController controller;

        public AWSTranscribeService(IController controller) {
            this.controller = controller;
        }

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
                transcribeClient = new AmazonTranscribeServiceClient(awsCredentials, region);
                return true;
            }
            return false;
        }

        public async Task requestExecuteTranscribe(AWSUtil awsUtilProperts)
        {
            AmazonTranscribeServiceClient TranscribeServiceClient = new AmazonTranscribeServiceClient(awsCredentials, region);

            StartTranscriptionJobResponse jobTranscribe = await ExecuteTranscribe(TranscribeServiceClient, awsUtilProperts);

            if (jobTranscribe.HttpStatusCode == HttpStatusCode.OK)
            {
                GetTranscriptionJobRequest jobStatus = new GetTranscriptionJobRequest()
                {
                    TranscriptionJobName = jobTranscribe.TranscriptionJob.TranscriptionJobName
                };

                GetTranscriptionJobResponse jobDataTranscribe;
                bool runningJobStatus = true;
                int incrementProgree = 0;
                while (runningJobStatus)
                {
                    jobDataTranscribe = await TranscribeServiceClient.GetTranscriptionJobAsync(jobStatus);
                    TranscriptionJob transcriptionJob = jobDataTranscribe.TranscriptionJob;
                    if (transcriptionJob.TranscriptionJobStatus == TranscriptionJobStatus.COMPLETED)
                    {
                        awsUtilProperts.JobName = extractFileKey(transcriptionJob.Transcript.TranscriptFileUri);
                        incrementProgree = 100;
                        controller.ViewStatusofTranscriptJob(transcriptionJob, incrementProgree);
                        runningJobStatus = false;
                    }
                    else
                    {
                        controller.ViewStatusofTranscriptJob(transcriptionJob, incrementProgree);
                        if (incrementProgree > 80)
                        {
                            incrementProgree = 0;
                        }
                        else
                        {
                            incrementProgree += 5;
                        }

                        await Task.Delay(5000);
                    }
                }
            }
        }
        public async Task<GetTranscriptionJobResponse> requestGetPropertsTranscribe(string transcribeJobName)
        {
            try
            {
                if (TranscribeClient())
                {
                    GetTranscriptionJobRequest jobStatus = new GetTranscriptionJobRequest()
                    {
                        TranscriptionJobName = transcribeJobName,
                    };
                    GetTranscriptionJobResponse jobDataTranscribe = await transcribeClient.GetTranscriptionJobAsync(jobStatus);
                    return jobDataTranscribe;
                }
                return null;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<StartTranscriptionJobResponse> ExecuteTranscribe(AmazonTranscribeServiceClient TranscribeClient, AWSUtil awsUtilProperts)
        {

            StartTranscriptionJobRequest JobRequest = new StartTranscriptionJobRequest
            {
                Media = new Media { MediaFileUri = $"s3://{awsUtilProperts.BucketNameInput}/{awsUtilProperts.FileNameActual}" },
                OutputBucketName = awsUtilProperts.BucketNameOutput,
                IdentifyLanguage = true,
               // LanguageCode = LanguageCode.PtBR,
                MediaFormat = MediaFormat.Mp3,
                Settings = new Settings { MaxAlternatives = 3, ShowAlternatives = true },//MaxSpeakerLabels = 3, ShowSpeakerLabels = true
                TranscriptionJobName = $"Transcribe-MediaFile-{DateTime.Now.ToString("yyyymmddhhmmss")}"
            };

            StartTranscriptionJobResponse jobResponse = await TranscribeClient.StartTranscriptionJobAsync(JobRequest);

            return jobResponse;

        }

        public async Task<ListTranscriptionJobsResponse> getListJobsTranscribe()
        {

           // AmazonTranscribeServiceClient TranscribeClient = new AmazonTranscribeServiceClient(awsCredentials, region);

            if (TranscribeClient())
            {
                ListTranscriptionJobsRequest request = new ListTranscriptionJobsRequest
                {
                    Status = TranscriptionJobStatus.COMPLETED,
                    MaxResults = 10
                };
                ListTranscriptionJobsResponse response = await transcribeClient.ListTranscriptionJobsAsync(request); // token
                return response;
            }
            return null;

        }


        private string extractFileKey(string mediaFileUri)
        {
            string[] nameFileSplit = mediaFileUri.Split('/');
            string nomeFile = nameFileSplit[nameFileSplit.Length - 1];
            return nomeFile;
        }
    }
}
