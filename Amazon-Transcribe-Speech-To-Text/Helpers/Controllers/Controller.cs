﻿using Amazon.TranscribeService;
using Amazon.TranscribeService.Model;
using Amazon_Transcribe_Speech_To_Text.Helpers.Interface;
using Amazon_Transcribe_Speech_To_Text.Helpers.Models.AWServices;
using Amazon_Transcribe_Speech_To_Text.Helpers.Models.Entity;
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

        private PlayerMedia playerMedia;
        private SpeechToText speechToText;

        public Controller(IViewTranscribe formTranscribe)
        {
            this.formTranscribe = formTranscribe;
           // this.awsServices = new AWSServices(this);
            this.playerMedia = new PlayerMedia(this);
            this.awsS3Service = new AWSS3Service();
            this.transcribeService = new AWSTranscribeService();
            this.awsUtil = new AWSUtil();
        }
        public List<string> getLoadS3ListBuckets() {
           return awsS3Service.S3ListBuckets();        
        }

        public async void setFileFromBucket(string fileName)
        {
            bool insertInBucket= true;
            awsUtil.NewFileName = fileName;
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

        public void displayParametersInitials(TimeSpan totalTime, List<Entity.Transcript> contentText)
        {
            formTranscribe.displayTotalTime(totalTime);
            formTranscribe.bindTextContent(contentText);
        }

        public async Task displayParametersCurrents(TimeSpan currentAudio, Item item) {
            formTranscribe.displayStatusCurrentProgress(currentAudio);
            formTranscribe.displayTrancribe(item);
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

        public void ViewStatusofTranscriptJob(TranscriptionJob transcriptionJob, int incrementProgrees)
        {
            formTranscribe.setJobProperties(transcriptionJob, incrementProgrees);
        }

        public void TranscribeObject()
        {
            speechToText = new SpeechToText(this, awsS3Service,transcribeService,awsUtil);

        }

        #region controlesAudio
        public async void setPlayMedia()
        {
            speechToText.controlExecutePlayer();
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
            Item item = speechToText.removeContentItem(valueStart, valueEnd, indexSelect);
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


        #endregion
    }
}