using Amazon;
using Amazon.Polly;
using Amazon.Polly.Model;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Amazon.S3;
using Amazon.Translate.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amazon_Transcribe_Speech_To_Text.Helpers.Models.AWServices
{
    public class AWSPollyService
    {
        private AWSCredentials awsCredentials;
        private AmazonPollyClient PollyClient;
        private static readonly RegionEndpoint region = RegionEndpoint.USEast1;

        public AWSPollyService()
        {
            GetCredentialsAWS();
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
        private bool GetPollyClient()
        {
            if (GetCredentialsAWS())
            {
                PollyClient = new AmazonPollyClient(awsCredentials, region);
                return true;
            }
            return false;
        }
        public async Task<List<Voice>> GetLocutorVoice(string idiomaIdentificado) {
            try
            {

                if (GetPollyClient()) {
                    DescribeVoicesRequest describeVoicesRequest = new DescribeVoicesRequest
                    {
                        LanguageCode = LanguageCode.PtBR,
                    };
                    DescribeVoicesResponse describeVoicesResponse = await PollyClient.DescribeVoicesAsync(describeVoicesRequest);
                    List<Voice> voices = describeVoicesResponse.Voices;
                    return voices;
                }
            }
            catch (Exception)
            {
                throw new ApplicationException("Erro ao obter Locutores ");
            }
            return null;
        }
        public async Task<string> CallVoicePolly(AWSUtil awsUtil,string voiceIDSelected, TranslateTextResponse translateTextResponse)
        {
            try
            {
                if (GetPollyClient())
                {
                    SynthesizeSpeechRequest synthesizeSpeechRequest = new SynthesizeSpeechRequest {
                        Text = translateTextResponse.TranslatedText,
                        TextType = "text",
                        OutputFormat = "mp3",
                        SampleRate = "8000",
                        VoiceId = voiceIDSelected,
                    };
                    SynthesizeSpeechResponse synthesizeSpeechResponse = await PollyClient.SynthesizeSpeechAsync(synthesizeSpeechRequest);
                   
                    string path = $@"..\..\..\Audios\Traduzidos\MediaPolly-Translate-{awsUtil.FileNameActual}";
                    using (var FileStream = File.Create(path))
                    {
                        synthesizeSpeechResponse.AudioStream.CopyTo(FileStream);
                        FileStream.Flush();
                        FileStream.Close();
                    }
                    return path;
                }
                return "";
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                //    throw new ApplicationException("Erro ao obter Locutores ");
                throw new ApplicationException(e.Message);
            }
        }

    }
}
