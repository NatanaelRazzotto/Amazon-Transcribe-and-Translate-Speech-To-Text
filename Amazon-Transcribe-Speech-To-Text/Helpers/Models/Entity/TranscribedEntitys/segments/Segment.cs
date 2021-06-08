using Amazon_Transcribe_Speech_To_Text.Helpers.Models.Entity.TranscribedEntitys.segments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amazon_Transcribe_Speech_To_Text.Helpers.Models.Entity.TranscribedEntitys
{
    public class Segment
    {
        public double start_time { get; set; }
        public double end_time { get; set; }
        public List<AlternativeSegment> alternatives { get; set; }
    }
}
