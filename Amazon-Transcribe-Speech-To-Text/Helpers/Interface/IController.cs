using Amazon.Polly.Model;
using Amazon.TranscribeService.Model;
using Amazon.Translate.Model;
using Amazon_Transcribe_Speech_To_Text.Helpers.Models.Entity;
using Amazon_Transcribe_Speech_To_Text.Helpers.Models.Entity.TranscribedEntitys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amazon_Transcribe_Speech_To_Text.Helpers.Interface
{
    public interface IController
    {
        void ViewStatusofTranscriptJob(TranscriptionJob transcriptionJob, int incrementProgrees);
        void ViewStatusofTranslateJob(TextTranslationJobProperties transcriptionJob, int incrementProgrees);
        void displayParametersInitials(TimeSpan totalTime, List<Models.Entity.Transcript> contentText, TranscriptionJob transcriptionJob);
        void displayParametersCurrents(TimeSpan currentAudio, Item item , Segment segment);
        void setViewDetailsContentSelect( Segment segment, int index);//Segment segment
        //void setViewDetailsContentSelect(double valueStart, double valueEnd, int indexSelect);//Segment segment
        void setTranscribedEditTranslator(TranslateTextResponse translatedText);
        void setFromListVoices(List<Voice> voices);
    }
}
