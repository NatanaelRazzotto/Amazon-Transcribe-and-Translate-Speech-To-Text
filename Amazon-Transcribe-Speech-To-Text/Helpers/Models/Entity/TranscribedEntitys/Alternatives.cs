using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amazon_Transcribe_Speech_To_Text.Helpers.Models.Entity
{
    public class Alternatives
    {
        public float confidence { get; set; }
        public string content { get; set; }
        public bool changed { get; set; } = false;
    }
}
