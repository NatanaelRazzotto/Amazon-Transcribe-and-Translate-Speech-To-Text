using Amazon;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Amazon.Translate;
using Amazon.Translate.Model;
using Amazon_Transcribe_Speech_To_Text.Helpers.Interface;
using System;
using System.Collections.Generic;
using System.IO;
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
      //  private AWSUtil awsUtilService;
        public AWSTranslateService(IController controller) {
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
        public bool TranscribeClient()
        {
            if (GetCredentialsAWS())
            {
                transcribeClient = new AmazonTranslateClient(awsCredentials, region);
                return true;
            }
            return false;
        }
        public async Task<TranslateTextResponse> requestExecuteTranslate(string translate, AWSUtil awsUtilService) {

            TranslateTextResponse translateTextResponse = new TranslateTextResponse();
            AWSS3Service awsS3Service = new AWSS3Service();
            byte[] byteArray = Encoding.UTF8.GetBytes(translate);
            awsUtilService.FolderActual = await awsS3Service.UploadFileFromS3(byteArray, "txt", awsUtilService);
            StartTextTranslationJobResponse startTextTranslationJobResponse = await ExecuteJobTranslate(awsUtilService);

            if (startTextTranslationJobResponse.HttpStatusCode == System.Net.HttpStatusCode.OK)
            {
                bool runningJobStatus = true;
                int incrementProgree = 0;
                while (runningJobStatus)
                {
                    TextTranslationJobProperties textTranslationJobProperties = await GetDescribeJobTranslate(startTextTranslationJobResponse);
                    if (textTranslationJobProperties.JobStatus == JobStatus.COMPLETED)
                    {
                        string pathURL = Path.GetFileName(textTranslationJobProperties.OutputDataConfig.S3Uri);
                        translateTextResponse.TranslatedText = await awsS3Service.DownloadFileAsync(pathURL, awsUtilService);
                        incrementProgree = 100;
                        runningJobStatus = false;
                    }
                    else if (textTranslationJobProperties.JobStatus == JobStatus.SUBMITTED)
                    {
                        controller.ViewStatusofTranslateJob(textTranslationJobProperties, incrementProgree);
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
                    else if (textTranslationJobProperties.JobStatus == JobStatus.IN_PROGRESS)
                    {
                        controller.ViewStatusofTranslateJob(textTranslationJobProperties, incrementProgree);
                        if (incrementProgree > 80)
                        {
                            incrementProgree = 0;
                        }
                        else
                        {
                            incrementProgree += 5;
                        }

                        await Task.Delay(9000);
                    }

                }
            }

            return translateTextResponse;

        }
        

        [Obsolete ("Method substituido por jobs")]
        public async Task<TranslateTextResponse> PreparToTranslateLimited(string translate, string SourceLanguage, string TargetLanguage) {

            TranslateTextResponse translateTextResponse = new TranslateTextResponse();

            string[] codLanguage = SourceLanguage.Split("-");

            int lenghtStringTranslate = translate.Length;
            if (lenghtStringTranslate >= 5000)
            {                
                string[] translateText = translate.Split(".");
                foreach (string trecho in translateText)
                {
                    if (!String.IsNullOrEmpty(trecho))
                    {
                        TranslateTextResponse textResponse = await TranslateText(trecho, codLanguage[0], TargetLanguage);
                        translateTextResponse.TranslatedText += textResponse.TranslatedText;
                    }
                }
                return translateTextResponse;
            }
            else
            {
                translateTextResponse = await TranslateText(translate, codLanguage[0], TargetLanguage);
                return translateTextResponse;
            }
        }
        public async Task<TranslateTextResponse> TranslateText(string translate, string SourceLanguage, string TargetLanguage)
        {
            try
            {
                if (TranscribeClient())
                {
                    TranslateTextRequest translateTextRequest = new TranslateTextRequest
                    {
                        Text = translate,
                        SourceLanguageCode = SourceLanguage,//"pt"
                        TargetLanguageCode = TargetLanguage,//"en"
                    };

                    return await transcribeClient.TranslateTextAsync(translateTextRequest);
                }
                return null;
            }
            catch (Exception)
            {
                throw new ApplicationException("Erro ao Trasncrever o texto");
            }

        }

        public async Task<StartTextTranslationJobResponse> ExecuteJobTranslate(AWSUtil awsUtilProperts)
        {
            try
            {
                if (TranscribeClient())
                {
                    StartTextTranslationJobRequest startTextTranslationJobRequest = new StartTextTranslationJobRequest
                    {
                        DataAccessRoleArn = "arn:aws:iam::308881334884:role/service-role/AmazonTranslateServiceRole-AWSTranscriptionAndTranslates",
                        InputDataConfig = new InputDataConfig
                        {
                            S3Uri = $"s3://{awsUtilProperts.BucketNameInput}/{awsUtilProperts.FolderActual}",
                            ContentType = "text/plain"
                        },
                        OutputDataConfig = new OutputDataConfig { S3Uri = $"s3://{awsUtilProperts.BucketNameOutput}/{awsUtilProperts.FolderActual}" },
                        JobName = $"Translate-Text-{DateTime.Now.ToString("yyyymmddhhmmss")}",
                        SourceLanguageCode = awsUtilProperts.IdiomaEntrada,
                        TargetLanguageCodes = new List<string>() { awsUtilProperts.IdiomaSaida },
                    };

                    StartTextTranslationJobResponse startTextTranslationJobResponse = await transcribeClient.StartTextTranslationJobAsync(startTextTranslationJobRequest);
                    return startTextTranslationJobResponse;
                }
                return null;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<TextTranslationJobProperties> GetDescribeJobTranslate(StartTextTranslationJobResponse startTextTranslationJobResponse)
        {
            try
            {
                if (TranscribeClient())
                {
                    DescribeTextTranslationJobRequest TranslationJobRequest = new DescribeTextTranslationJobRequest
                    {
                        JobId = startTextTranslationJobResponse.JobId,
                    };
                    DescribeTextTranslationJobResponse TranslationJobResponse = await transcribeClient.DescribeTextTranslationJobAsync(TranslationJobRequest);
                    return TranslationJobResponse.TextTranslationJobProperties;
                }
                return null;
            }
            catch (Exception)
            {
                throw;
            }
        }

    }
}
