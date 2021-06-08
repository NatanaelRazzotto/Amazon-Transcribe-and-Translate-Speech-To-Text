using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amazon_Transcribe_Speech_To_Text.Helpers.Models.Entity
{
    class Transcribed
    {
        public string jobName { get; set; }
        public string accountId { get; set; }
        public ResultsTranscribed results { get; set; }
        public string status { get; set; }
    }
}
