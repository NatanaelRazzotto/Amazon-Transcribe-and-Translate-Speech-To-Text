using Amazon.Polly.Model;
using Amazon.TranscribeService.Model;
using Amazon.Translate.Model;
using Amazon_Transcribe_Speech_To_Text.Helpers.Interface;
using Amazon_Transcribe_Speech_To_Text.Helpers.Models.AWServices;
using Amazon_Transcribe_Speech_To_Text.Helpers.Models.Entity.TranscribedEntitys;
using Amazon_Transcribe_Speech_To_Text.Helpers.Models.Entity.TranscribedEntitys.segments;
using NAudio.Wave;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Amazon_Transcribe_Speech_To_Text.Helpers.Models.Entity
{
    public class SpeechToText
    {
        IController Controller;
        PlayerMedia playerMedia;
        PlayerMedia playerMediaPolly;
        Transcribed transcribed;
        
        private AWSS3Service awsS3Service;
        private AWSTranscribeService awsTranscribeService;
        private AWSTranslateService translateService;
        private AWSUtil awsUtil;
        private TranscriptionJob transcriptionJob;
        private TranslateTextResponse translateTextResponse;
        private AWSPollyService awsPollyService;
        private Task taskControle;

        public SpeechToText(Controller Controller, AWSUtil awsUtil, TranscriptionJob transcriptionJob) {
            this.Controller = Controller;
            this.awsUtil = awsUtil;
            this.awsS3Service = new AWSS3Service();
            this.awsTranscribeService = new AWSTranscribeService(Controller);
            this.transcriptionJob = transcriptionJob;

            this.playerMedia = new PlayerMedia(this.Controller);
            setNewFileFromExecuteTrasncribe();
        }
        public async void setNewFileFromExecuteTrasncribe() {

            bool checkFile = await playerMedia.newFileAudio(awsUtil);
            if (checkFile) 
            {
                string jsonString = await awsS3Service.getObjectTranscribeS3(awsUtil);
                if (!String.IsNullOrEmpty(jsonString))
                {
                    transcribed = JsonConvert.DeserializeObject<Transcribed>(jsonString);
                    TimeSpan TotalTime = playerMedia.getTotalTimeAudio();
                    ResultsTranscribed results = transcribed.results;
                    setAverageConfidenceItem(results.items);
                    Controller.displayParametersInitials(TotalTime, results.transcripts, transcriptionJob);
                }
            }
        }
        public void setAverageConfidenceItem(List<Item> items) {
            foreach (Item itemElement in items)
            {
                float confidences = 0;
                foreach (Alternatives itemAlternative in itemElement.alternatives)
                {
                    confidences += itemAlternative.confidence;
                }
                itemElement.averageConfidence = (confidences / itemElement.alternatives.Count);
            }
        }

        public async void controlExecutePlayer()
        {
            if (!playerMedia.checkExecute())
            {
                playerMedia.clickPlay();
            }
            else
            {
                playerMedia.clickPaused();
            }

            await trackAudio();
        }
        public void defineNewCurrentTimeMilisseconds(double time)
        {
            TimeSpan newTime = TimeSpan.FromMilliseconds(time);
            playerMedia.trackAudioPlay(newTime);
        }
       /* public async void StartViewDetalhesTranscription()
        {
            //taskControle = Task.Run(() => trackAudio());
            //taskControle.Wait
        }*/

        public async Task trackAudio()
        {
            bool playMedia = true;
            PlaybackState statePlayback = playerMedia.getPlaybackState();

            while (playMedia)
            {
                if (statePlayback == PlaybackState.Playing)
                {
                    await Task.Delay(1000);
                }
                else if (statePlayback == PlaybackState.Paused)
                {
                    await Task.Delay(2000);
                }
                else if (statePlayback == PlaybackState.Stopped)
                {
                    await Task.Delay(5000);
                }
                TimeSpan currentAudio = playerMedia.getCurrentTime();
                Item item = getTextFromSpeach(currentAudio);
                Segment segment = getSegmentFromTime(currentAudio);
                if ((segment != null) && (item != null))
                {
                    Controller.displayParametersCurrents(currentAudio, item, segment);
                }

            }

        }
        public Item getTextFromSpeach(TimeSpan timeCurrent) {
            List<Item> items = transcribed.results.items;
            Item itemSelect = items.Find(x => (x.start_time <= timeCurrent.TotalSeconds) && (x.end_time >= timeCurrent.TotalSeconds));
            return itemSelect;

        }
        public Segment getSegmentFromTime(TimeSpan timeCurrent)
        {
            List<Segment> Segments = transcribed.results.Segments;
            Segment segmentContent = Segments.Find(x => (x.start_time <= timeCurrent.TotalSeconds) && (x.end_time >= timeCurrent.TotalSeconds));
            return segmentContent;
        }
        public List<Transcript> regenerate()
        {
            List<Item> items = transcribed.results.items;
            string content = "";
            foreach (Item item in items)
            {               
                if (item.alternatives.Count > 0)
                {
                    Alternatives alternative;
                    alternative = item.alternatives.Find(x => x.changed.Equals(true));
                    if (alternative == null)
                    {
                        alternative = searchMaxConfidence(item);
                    }
                    content = $"{content} {alternative.content}";
                }
                else
                {
                    content = content + item.alternatives.ElementAt(0).content;
                }
            }
            Transcript transcript = new Transcript();
            transcript.transcript = content;
            transcribed.results.transcripts.Clear();
            transcribed.results.transcripts.Add(transcript);
            return transcribed.results.transcripts;
        }
        public Alternatives searchMaxConfidence(Item item) {
            float confidences = 0;
            //int index = -1;
            Alternatives alternative = null;
            for (int a = 0; a < item.alternatives.Count; a++)
            {
                if (item.alternatives.ElementAt(a).confidence > confidences)
                {
                    alternative = item.alternatives.ElementAt(a);
                    confidences = alternative.confidence;
                }
            }
            if (alternative == null)
            {
                alternative = item.alternatives.ElementAt(0);
            }
            return alternative;
        }
        public AlternativeSegment SearchContentTempo(double valueStart, double valueEnd, int indexSelect)
        {
            List<Item> items = transcribed.results.items;
            List<Segment> Segments = transcribed.results.Segments;
            Segment segmentContent = Segments.Find(x => (x.start_time <= valueStart) && (x.end_time >= valueEnd));
            AlternativeSegment alternativeSegment = segmentContent.alternatives.ElementAt(indexSelect);
            return alternativeSegment;

        }
        public Item addContentItem(double valueStart, double valueEnd, string content)
        {
            List<Item> items = transcribed.results.items;
            Item itemSelect = items.Find(x => (x.start_time == valueStart) && (x.end_time == valueEnd));
            if (!itemSelect.Equals(null))
            {
               Alternatives checkAlternative = itemSelect.alternatives.Find(x => x.changed.Equals(true));
                if (checkAlternative == null)
                {
                    Alternatives alternative = new Alternatives
                    {
                        confidence = 1,
                        content = content,
                        changed = true
                    };
                    // itemSelect.alternatives.Insert(0, alternative);
                    itemSelect.alternatives.Clear();
                    itemSelect.alternatives.Add(alternative);
                }
                else
                {
                    checkAlternative.content = content;
                }             

            }
            
            return itemSelect;
        }

        public Segment actualizarContentTempo(double valueStart, double valueEnd, int indexSelect)
        {
            List<Item> items = transcribed.results.items;
            List<Segment> Segments = transcribed.results.Segments;
            Segment segmentContent = Segments.Find(x => (x.start_time <= valueStart) && (x.end_time >= valueEnd));
            AlternativeSegment alternativeSegment = segmentContent.alternatives.ElementAt(indexSelect);


            foreach (var alternative in alternativeSegment.items)
            {
                foreach (Item item in items)
                {
                    if ((item.start_time == alternative.start_time) && (item.end_time == alternative.end_time))
                    {
                        
                        item.type = alternative.type;
                        //item.averageConfidence = alternative.confidence;
                        item.alternatives.ElementAt(0).confidence = alternative.confidence;
                        item.alternatives.ElementAt(0).content = alternative.content;
                        item.alternatives.ElementAt(0).changed = true;

                    }
                }
            }
            return segmentContent;
        }
        public Segment GetFromTempDetails(double valueStart, double valueEnd, int indexSelect)
        {
            List<Item> items = transcribed.results.items;
            List<Segment> Segments = transcribed.results.Segments;
            Segment segmentContent = Segments.Find(x => (x.start_time <= valueStart) && (x.end_time >= valueEnd));
            return segmentContent;

        }

        public async void CallTranslatePolly(string SelectedVoice)
        {
            this.playerMediaPolly = new PlayerMedia(this.Controller);
            string routePath = await  awsPollyService.CallVoicePolly(awsUtil, SelectedVoice, translateTextResponse);
            if (!String.IsNullOrEmpty(routePath))
            {
                bool validate = await playerMediaPolly.newFileAudioPolly(routePath);
            }
        }
        public void controlExecutePlayerPolly()
        {
            if (!playerMediaPolly.checkExecute())
            {
                playerMediaPolly.clickPlay();
            }
            else
            {
                playerMediaPolly.clickPaused();
            }
        }
        public async void TranslateTextFromAudio(string selectedIdioma)
        {
            Transcript results = transcribed.results.transcripts.ElementAt(0);

            translateService = new AWSTranslateService(Controller);
            translateTextResponse = await PreparToTranslate(results.transcript, transcriptionJob.LanguageCode, selectedIdioma);
            Controller.setTranscribedEditTranslator(translateTextResponse);
            awsPollyService = new AWSPollyService();
            List<Voice> voices = await awsPollyService.DefinedVoices(translateTextResponse.TargetLanguageCode);
            Controller.setFromListVoices(voices);

        }


        public async Task<TranslateTextResponse> PreparToTranslate(string translate, string SourceLanguage, string TargetLanguage)
        {

            TranslateTextResponse translateResponse = new TranslateTextResponse();

            string[] codLanguage = SourceLanguage.Split("-");
            int lenghtStringTranslate = translate.Length;
            awsUtil.IdiomaEntrada = codLanguage[0];
            awsUtil.IdiomaSaida = TargetLanguage;

            if (lenghtStringTranslate >= 5000)
            {
                translateResponse = await translateService.requestExecuteTranslate(translate, awsUtil);
                return translateResponse;
            }
            else
            {
                translateResponse = await translateService.TranslateText(translate, codLanguage[0], TargetLanguage);
                //Controller.ViewStatusofTranslateJob(translateResponse, 100);
                return translateResponse;
            }
        }

        public Item removeContentItem(double valueStart, double valueEnd, int indexSelect)
        {
            List<Item> items = transcribed.results.items;
            Item itemSelect = items.Find(x => (x.start_time == valueStart) && (x.end_time == valueEnd));
            if ((indexSelect > 0) && (indexSelect < itemSelect.alternatives.Count))
            {
                itemSelect.alternatives.RemoveAt(indexSelect);
                return itemSelect;
            }
            else
            {
                return null;
            }
            
        }
    }
}
