using Amazon_Transcribe_Speech_To_Text.Helpers.Models.Entity.TranscribedEntitys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amazon_Transcribe_Speech_To_Text.Helpers.Models.Entity
{
    public class ResultsTranscribed
    {
        public List<Transcript> transcripts { get; set; }
        public List<Item> items { get; set; }
        public List<Segment> Segments { get; set; }
    }
}
