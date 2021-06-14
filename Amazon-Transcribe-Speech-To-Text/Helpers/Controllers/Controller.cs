using Amazon.Polly.Model;
using Amazon.TranscribeService;
using Amazon.TranscribeService.Model;
using Amazon.Translate.Model;
using Amazon_Transcribe_Speech_To_Text.Helpers.Interface;
using Amazon_Transcribe_Speech_To_Text.Helpers.Models.AWServices;
using Amazon_Transcribe_Speech_To_Text.Helpers.Models.Entity;
using Amazon_Transcribe_Speech_To_Text.Helpers.Models.Entity.TranscribedEntitys;
using AWS_Rekognition_Objects.Helpers.Model;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Amazon_Transcribe_Speech_To_Text.Helpers.Models
{
    public class Controller : IController
    {
        private IViewTranscribe formTranscribe;
        private AWSServices awsServices;
        private AWSS3Service awsS3Service;
        private AWSTranscribeService transcribeService;
        private AWSUtil awsUtil;
        private TranscriptionJob transcriptionJob;
        private PlayerMedia playerMedia;
        private SpeechToText speechToText;
        private AWSPollyService awsPollyService;
        private List<string> LanguageCodes = new List<string>() { "en", "ar", "cs", "de", "es", "fr", "it",
                                                                    "ja", "pt", "ru", "tr", "zh", "zh-TW" };


        public Controller(IViewTranscribe formTranscribe)
        {
            this.formTranscribe = formTranscribe;
           // this.awsServices = new AWSServices(this);
            this.playerMedia = new PlayerMedia(this);
            this.awsS3Service = new AWSS3Service();
            this.transcribeService = new AWSTranscribeService(this);
            this.awsUtil = new AWSUtil();
        }
        public List<string> getLoadS3ListBuckets() {
           return awsS3Service.S3ListBuckets();        
        }

        public async void setFileFromBucket(string fileName)
        {
            bool insertInBucket= true;
            awsUtil.FileNameActual = fileName;
            if (awsS3Service.checkFileOnBucket(awsUtil))
            {
                string messageAlert = $"Arquivo ja consta no bucket: {awsUtil.BucketNameInput}, deseja substituilo ?";
                if (MessageBox.Show(messageAlert, "Atenção", MessageBoxButtons.YesNo) == DialogResult.No)
                {
                    insertInBucket = false;
                }
            }
            if (insertInBucket)
            {
                await formTranscribe.displayStatusProgressFile(true);
                
                bool checkTerminateUpload = await awsS3Service.UploadAudioFromS3(awsUtil);
                if (checkTerminateUpload)
                {
                    await formTranscribe.displayStatusProgressFile(false, awsUtil.FileNameActual);
                }
            }
        }

        public void displayParametersInitials(TimeSpan totalTime, List<Entity.Transcript> contentText, TranscriptionJob transcriptionJob)
        {
            formTranscribe.displayTotalTime(totalTime);
            formTranscribe.bindTextContent(contentText);
            formTranscribe.bindMenuTranslate(transcriptionJob.LanguageCode, LanguageCodes);

        }

        public async Task displayParametersCurrents(TimeSpan currentAudio, Item item, Segment segment) {
            formTranscribe.displayStatusCurrentProgress(currentAudio);
            formTranscribe.displayTrancribe(item, segment);
        }

        public bool setFromNameBuckets(string bucketInput, string bucketOutput)
        {
            awsUtil.setBucktes(bucketInput, bucketOutput);
            //awsServices.setBucktes(bucketInput, bucketOutput);
            return updateComboFileOnBucket(bucketInput);

        }

        public async void setFromListJobs()
        {
            ListTranscriptionJobsResponse jobsResponse = await transcribeService.getListJobsTranscribe();
            List<TranscriptionJobSummary> jobsSummary = jobsResponse.TranscriptionJobSummaries;
            if (jobsSummary.Count != 0)
            {
                bool validate = formTranscribe.updateComboNameJobs(jobsSummary);
            }
        }
        public bool updateComboFileOnBucket(string bucket)
        {
            //string bucket = awsServices.getBucketInput();
            List<string> nameAudios = awsS3Service.audioInputBucketNames(awsUtil);
            if (nameAudios.Count != 0)
            {
                bool validation = formTranscribe.updateComboNameAudios(bucket, nameAudios);
                return validation;
            }
            else
            {
                return false;
            }
        }

        public void setFileFromAnalize(string file)
        {
            awsUtil.FileNameActual = file;
            formTranscribe.releaseTranscript(file);
        }

        public async void executeTranscribeToS3()
        {
           await transcribeService.requestExecuteTranscribe(awsUtil);
        }

        public void ViewStatusofTranscriptJob(TranscriptionJob transcriptionJobNew, int incrementProgrees)
        {
            this.transcriptionJob = transcriptionJobNew;
            formTranscribe.setJobProperties(transcriptionJobNew, incrementProgrees);
        }
        public void ViewStatusofTranslateJob(TextTranslationJobProperties transcriptionJob, int incrementProgrees)
        {
            formTranscribe.setJobPropertiesTranslate(transcriptionJob, incrementProgrees);
        }

        public void TranscribeObject()
        {
            speechToText = new SpeechToText(this,awsUtil,transcriptionJob);

        }
        public async void getTranscriptInformation(string selectTranscribe)
        {
            GetTranscriptionJobResponse getTranscriptionJob = await transcribeService.requestGetPropertsTranscribe(selectTranscribe);
            if (getTranscriptionJob.HttpStatusCode == System.Net.HttpStatusCode.OK)
            {
                transcriptionJob = getTranscriptionJob.TranscriptionJob;
                string transcriptURL = transcriptionJob.Transcript.TranscriptFileUri;
                awsUtil.JobName = Path.GetFileName(transcriptURL);
                string mediaURL = transcriptionJob.Media.MediaFileUri;
                awsUtil.FileNameActual = Path.GetFileName(mediaURL);

                //speechToText = new SpeechToText(this, awsUtil);
            }
        }

        #region controlesAudio
        public  void setPlayMedia()
        {
            speechToText.controlExecutePlayer();
        }
        public void setPlayMediaPolly()
        {
            speechToText.controlExecutePlayerPolly();
        }
        public void definedPositionAudioMilisseconds(double timeSelect)
        {
            speechToText.defineNewCurrentTimeMilisseconds(timeSelect);
        }

        internal void setModifyContent(double valueStart, double valueEnd, string content)
        {
            Item itemNew = speechToText.addContentItem(valueStart, valueEnd, content);
            if (itemNew != null)
            {
                if (itemNew.alternatives.Count != 0)
                {
                    formTranscribe.displayTrancribe(itemNew);
                }
            }
        }

        public void setRemoveContentSelect(double valueStart, double valueEnd, int indexSelect)
        {            
            Item item = speechToText.actualizarContentTempo(valueStart, valueEnd, indexSelect);
            //Item item = speechToText.removeContentItem(valueStart, valueEnd, indexSelect);
            if (item != null)
            {
                if (item.alternatives.Count != 0)
                {
                    formTranscribe.displayTrancribe(item);
                }
            }

        }

        public void genarateNewContent() {
            List<Entity.Transcript> contentText = speechToText.regenerate();
            formTranscribe.bindTextContent(contentText);
        }

        public void TranslateFromIdioma(int selectIdiomaTranscribe)
        {
            string selectedIdioma = LanguageCodes.ElementAt(selectIdiomaTranscribe);
            speechToText.TranslateTextFromAudio(selectedIdioma);
        }

        public void setTranscribedEditTranslator(string translatedText)
        {
            formTranscribe.bindTextTranslator(translatedText);
        }

        public async Task<List<Voice>> setFromListVoices()
        {
           return await speechToText.getCallsPollyAsync();
        }

        public void setFromVoicesAsync(string text)
        {
            speechToText.CallTranslatePolly(text);

        }

        public async void trackAudio()
        {
           await speechToText.trackAudio();
        }






        #endregion
    }
}
