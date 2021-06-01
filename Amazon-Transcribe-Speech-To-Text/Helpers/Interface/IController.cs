using Amazon.TranscribeService.Model;
using Amazon_Transcribe_Speech_To_Text.Helpers.Models.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amazon_Transcribe_Speech_To_Text.Helpers.Interface
{
    public interface IController
    {
        public void ViewStatusofTranscriptJob(TranscriptionJob transcriptionJob, int incrementProgrees);
        public void displayParametersInitials(TimeSpan totalTime, List<Models.Entity.Transcript> contentText);
        public Task displayParametersCurrents(TimeSpan currentAudio, Item item);
    }
}
