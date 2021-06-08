using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amazon_Transcribe_Speech_To_Text.Helpers.Models.Entity.TranscribedEntitys.segments
{
    public class AlternativeSegment
    {
        public string transcript { get; set; }
        public List<ItemSegment> items { get; set; }
    }
}
