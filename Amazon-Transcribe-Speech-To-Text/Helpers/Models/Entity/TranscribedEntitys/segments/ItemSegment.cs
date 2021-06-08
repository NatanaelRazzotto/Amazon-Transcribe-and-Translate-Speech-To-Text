using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amazon_Transcribe_Speech_To_Text.Helpers.Models.Entity.TranscribedEntitys.segments
{
    public class ItemSegment
    {
        public double start_time { get; set; }
        public double end_time { get; set; }
        public float confidence { get; set; }
        public string content { get; set; }
        public string type { get; set; }
    }
}
