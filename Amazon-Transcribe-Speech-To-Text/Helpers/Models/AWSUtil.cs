using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amazon_Transcribe_Speech_To_Text.Helpers.Models
{
    public class AWSUtil
    {
        private string bucketNameInput;
        private string bucketNameOutput;
        private string fileNameActual;
        private string newFileName;
        private string folderActual;
        private string jobName = "Transcribe-MediaFile-20212801092827.json";
        private string idiomaEntrada;
        private string idiomaSaida;

        private List<string> ListExistingImagesBucket;

        public List<string> ExistingImagesBucket
        {
            get => ListExistingImagesBucket;
            set => ListExistingImagesBucket = value;
        }
        public string NewFileName
        {
            get => newFileName;
            set => newFileName = value;
        }
        public string FileNameActual
        {
            get => fileNameActual;
            set => fileNameActual = value;
        }
        public string FolderActual
        {
            get => folderActual;
            set => folderActual = value;
        }

        public string IdiomaEntrada
        {
            get => idiomaEntrada;
            set => idiomaEntrada = value;
        }

        public string IdiomaSaida
        {
            get => idiomaSaida;
            set => idiomaSaida = value;
        }

        public string JobName
        {
            get => jobName;
            set => jobName = value;
        }

        public string BucketNameInput
        {
            get => bucketNameInput;
        }
        public string BucketNameOutput
        {
            get => bucketNameOutput;
        }
        public void setBucktes(string bucketInput, string bucketOutput)
        {
            this.bucketNameInput = bucketInput;
            this.bucketNameOutput = bucketOutput;
        }
    }
}
