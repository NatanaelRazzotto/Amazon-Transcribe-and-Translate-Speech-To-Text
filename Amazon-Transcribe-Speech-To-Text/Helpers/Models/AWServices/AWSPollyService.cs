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
        public async Task<List<Voice>> DefinedVoices(string idiomaIdentificado) {

            switch (idiomaIdentificado)
            {
                case "en":
                    return await GetLocutorVoice(LanguageCode.EnUS);
                case "ar":
                    return await GetLocutorVoice(LanguageCode.Arb);
                case "de":
                    return await GetLocutorVoice(LanguageCode.DeDE);
                case "es":
                    return await GetLocutorVoice(LanguageCode.EsES);
                case "fr":
                    return await GetLocutorVoice(LanguageCode.FrFR);
                case "it":
                    return await GetLocutorVoice(LanguageCode.ItIT);
                case "ja":
                    return await GetLocutorVoice(LanguageCode.JaJP);
                case "pt":
                    return await GetLocutorVoice(LanguageCode.PtBR);
                case "ru":
                    return await GetLocutorVoice(LanguageCode.RuRU);
                case "tr":
                    return await GetLocutorVoice(LanguageCode.TrTR);
                default:
                    return null;
            }
        }
        public async Task<List<Voice>> GetLocutorVoice(LanguageCode idiomaIdentificado) {
            try
            {

                if (GetPollyClient()) {
                    DescribeVoicesRequest describeVoicesRequest = new DescribeVoicesRequest
                    {
                        LanguageCode = idiomaIdentificado,
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
                   
                    string path = $@"..\..\..\Audios\Traduzidos\MediaPolly-{Guid.NewGuid()}.mp3";
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
                throw new ApplicationException(e.Message);
            }
        }

    }
}
