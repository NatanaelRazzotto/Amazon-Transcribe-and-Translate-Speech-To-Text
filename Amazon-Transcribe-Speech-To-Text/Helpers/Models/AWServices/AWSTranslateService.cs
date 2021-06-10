using Amazon;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Amazon.Translate;
using Amazon.Translate.Model;
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
        public bool TranscribeClient()
        {
            if (GetCredentialsAWS())
            {
                transcribeClient = new AmazonTranslateClient(awsCredentials, region);
                return true;
            }
            return false;
        }
        public async Task<TranslateTextResponse> PreparToTranslate(string translate, string SourceLanguage, string TargetLanguage) {

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


    }
}
