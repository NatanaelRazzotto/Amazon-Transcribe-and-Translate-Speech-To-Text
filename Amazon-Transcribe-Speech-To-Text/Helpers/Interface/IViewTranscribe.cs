using Amazon.Polly.Model;
using Amazon.TranscribeService;
using Amazon.TranscribeService.Model;
using Amazon.Translate.Model;
using Amazon_Transcribe_Speech_To_Text.Helpers.Models.Entity;
using Amazon_Transcribe_Speech_To_Text.Helpers.Models.Entity.TranscribedEntitys;
using Amazon_Transcribe_Speech_To_Text.Helpers.Models.Entity.TranscribedEntitys.segments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amazon_Transcribe_Speech_To_Text.Helpers.Interface
{
    public interface IViewTranscribe
    {
       public bool updateComboNameAudios(string nameBucket, List<string> nameAudios);
       public bool updateComboNameTranscribes(string nameBucket, List<string> nameTranscribe);
       public Task displayStatusProgressFile(bool state, string fileName = null);
       public void releaseTranscript(string file);
       public void setJobProperties(TranscriptionJob transcriptionJob, int incrementProgrees);
        public void displayTotalTime(TimeSpan totalTime);
        public void bindTextContent(List<Models.Entity.Transcript> contentText);
        public void displayTrancribe(Item item, Segment segment = null);
        public void displayDetailsTrancribe(string alternative);
        public void displayStatusCurrentProgress(TimeSpan currentAudio);
        public bool updateComboNameJobs(List<TranscriptionJobSummary> jobsSummary);
        void bindMenuTranslate(LanguageCode languageCode, List<string> languageCodes);
        void bindTextTranslator(string translatedText);
        void setJobPropertiesTranslate(TextTranslationJobProperties transcriptionJob, int incrementProgrees);
        void bindVoicesPolly(List<Voice>voices );
    }
}
