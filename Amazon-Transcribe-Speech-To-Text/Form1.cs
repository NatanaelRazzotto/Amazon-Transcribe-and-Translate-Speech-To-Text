using Amazon.Polly.Model;
using Amazon.TranscribeService;
using Amazon.TranscribeService.Model;
using Amazon.Translate.Model;
using Amazon_Transcribe_Speech_To_Text.Helpers.Interface;
using Amazon_Transcribe_Speech_To_Text.Helpers.Models;
using Amazon_Transcribe_Speech_To_Text.Helpers.Models.Entity;
using Amazon_Transcribe_Speech_To_Text.Helpers.Models.Entity.TranscribedEntitys;
using Amazon_Transcribe_Speech_To_Text.Helpers.Models.Entity.TranscribedEntitys.segments;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Amazon_Transcribe_Speech_To_Text
{
    public partial class Form1 : Form, IViewTranscribe
    {
        Controller controller;
        public Form1()
        {
            InitializeComponent();
            controller = new Controller(this);
            List<string> buckets = controller.getLoadS3ListBuckets();
            if (buckets != null)
            {
                foreach (string buckte in buckets)
                {
                    cbBucketsInputS3.Items.Add(buckte);
                    cbBucketsOutputS3.Items.Add(buckte);
                }
            }
        }
        private void btnPlay_Click(object sender, EventArgs e)
        {
            controller.setPlayMedia();
        }

        private void btnSearchFiles_Click(object sender, EventArgs e)
        {
            ofdSearchFiles.InitialDirectory = "c:\\";
            ofdSearchFiles.Filter = "Media File (*.mp3, *.MP3) | *.mp3; *.MP3";
            ofdSearchFiles.Title = "Selecione uma Imagem a Ser Analizada";
            if (ofdSearchFiles.ShowDialog() == DialogResult.OK)
            {
                controller.setFileFromBucket(ofdSearchFiles.FileName);
            }
        }



        private void btnSelectBucktes_Click(object sender, EventArgs e)
        {
            string bucketInput = cbBucketsInputS3.SelectedItem.ToString();
            string bucketOutput = cbBucketsOutputS3.SelectedItem.ToString();
            if (!String.IsNullOrEmpty(bucketInput))
            {
                if (!controller.setFromNameBuckets(bucketInput, bucketOutput))
                {
                    MessageBox.Show("Não foi possivel Buscar os Arquivos constidos no Bucket");
                }
                else
                {
                    controller.setFromListJobs();
                    tabControlBody.Enabled = true;
                }
                
            }
            else
            {
                MessageBox.Show("Você deve informar os Buckets");
            }
           
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(tbFileAudio.Text))
            {
                controller.setFileFromAnalize(Path.GetFileName(tbFileAudio.Text));
            }
        }

        private void cbFilesBucket_SelectedValueChanged(object sender, EventArgs e)
        {
            if ((cbFilesBucket.SelectedItem != null))//&& (cbFilesBucket.ValueMember != "")
            {
                controller.setFileFromAnalize(cbFilesBucket.SelectedItem.ToString());
            }
        }

        private void btnAnalizer_Click(object sender, EventArgs e)
        {
            pgbAnalizer.Minimum = 0;
            pgbAnalizer.Maximum = 100;
            controller.executeTranscribeToS3();
        }

        private void btnLoadLastTranscription_Click(object sender, EventArgs e)
        {
            controller.TranscribeObject();
            tabControlBody.SelectedIndex = 1;
            panel2.Enabled = true;
        }
        private void btnLoadTranscription_Click(object sender, EventArgs e)
        {
            controller.TranscribeObject();//TranscribeObjectSelected();
            tabControlBody.SelectedIndex = 1;
            panel2.Enabled = true;
        }

        private void cbJobTranscribe_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectTranscribe = cbJobTranscribe.SelectedItem.ToString();
            if (!string.IsNullOrEmpty(selectTranscribe))
            {
                controller.getTranscriptInformation(selectTranscribe);
            }

        }

        private void trackBarStateAudio_ValueChanged(object sender, EventArgs e)
        {
            double timeSelect = trackBarStateAudio.Value;
        }

        private void btnSubstituir_Click(object sender, EventArgs e)
        {
            double valueStart = Convert.ToDouble(lblStart.Text);
            double valueEnd = Convert.ToDouble(lblEnd.Text);
            int indexSelect = cbAlternative.SelectedIndex;
            controller.setRemoveContentSelect(valueStart, valueEnd, indexSelect);

        }
        private void cbAlternative_SelectedIndexChanged(object sender, EventArgs e)
        {
            double valueStart = Convert.ToDouble(lblStart.Text);
            double valueEnd = Convert.ToDouble(lblEnd.Text);
            int indexSelect = cbAlternative.SelectedIndex;
            controller.setRemoveContentSelect(valueStart, valueEnd, indexSelect);
        }

        private void btnAddContent_Click(object sender, EventArgs e)
        {
            double valueStart = Convert.ToDouble(lblStart.Text);
            double valueEnd = Convert.ToDouble(lblEnd.Text);
            string content = txtContent.Text;
            if (!String.IsNullOrEmpty(content))
            {
                controller.setModifyContent(valueStart, valueEnd, content);
            }
        }

        private void btnReGerar_Click(object sender, EventArgs e)
        {
            controller.genarateNewContent();
        }

        private void trackBarStateAudio_Scroll(object sender, EventArgs e)
        {
            double timeSelect = trackBarStateAudio.Value;
            controller.definedPositionAudioMilisseconds(timeSelect);
        }
        #region Methods
        // Metodos responsaveis pelo populamento dos dados

        public bool updateComboNameAudios(string nameBucket,List<string> nameAudios)
        {
            if (!String.IsNullOrEmpty(nameBucket)) 
            {
                lblNameBucketInput.Text = nameBucket;
                if (nameAudios != null)
                {
                    foreach (string item in nameAudios)
                    {
                        cbFilesBucket.Items.Add(item);
                    }
                    return true;
                }
                else
                {
                    return true;
                }
            }
            return false;
        }
        public bool updateComboNameJobs(List<TranscriptionJobSummary> jobsSummary)
        {
            cbJobTranscribe.Items.Clear();
            if (jobsSummary.Count != 0)
            {
                foreach (TranscriptionJobSummary job in jobsSummary)
                {
                    cbJobTranscribe.Items.Add(job.TranscriptionJobName);
                }
                return true;
            }
            else
            {
                return false;
            }
        }
        public bool updateComboNameTranscribes(string nameBucket, List<string> nameTranscribe)
        {
            if (!String.IsNullOrEmpty(nameBucket))
            {
                lblNameBucketOutput.Text = nameBucket;
                if (nameTranscribe != null)
                {
                    foreach (string item in nameTranscribe)
                    {
                        cbJobTranscribe.Items.Add(item);
                    }
                    return true;
                }
                else
                {
                    return true;
                }
            }
            return false;
        }
        public async Task displayStatusProgressFile(bool state, string fileName = null)
        {
            if (state)
            {
                pgbUploadFlie.Style = ProgressBarStyle.Marquee;
                pgbUploadFlie.MarqueeAnimationSpeed = 10;
                pgbUploadFlie.Visible = true;
            }
            else
            {
                pgbUploadFlie.Style = ProgressBarStyle.Continuous;
                pgbUploadFlie.MarqueeAnimationSpeed = 0;
                tbFileAudio.Text = fileName;
            }
        }
        public void releaseTranscript(string file)
        {
            lblFileSelecionado.Text = file;
            btnAnalizer.Enabled = true;
        }
        #endregion



        public void setJobProperties(TranscriptionJob transcriptionJob, int incrementProgrees)
        {
            pgbAnalizer.Value = incrementProgrees;
            lblNameJob.Text = transcriptionJob.TranscriptionJobName;
            lblStatusJob.Text = transcriptionJob.TranscriptionJobStatus;
            MediaFormat formatMidia = transcriptionJob.MediaFormat;
            LanguageCode language = transcriptionJob.LanguageCode;
            if (formatMidia == MediaFormat.Mp3)
            {
                lblFormat.Text = "Mp3";
            }
            if (language == LanguageCode.PtBR)
            {
                lblLanguage.Text = "PtBr";
            }
            lblHertz.Text = transcriptionJob.MediaSampleRateHertz.ToString();
            
        }

        public void setJobPropertiesTranslate(TextTranslationJobProperties transcriptionJob, int incrementProgrees)
        {
            pgbAnalizerTranslate.Value = incrementProgrees;
            lblJobNameTranslate.Text = transcriptionJob.JobName;
            lblJobStatusTranslate.Text = transcriptionJob.JobStatus;
            lblJobIdioma.Text = transcriptionJob.TargetLanguageCodes.ElementAt(0);
        }

        public void displayStatusCurrentProgress(TimeSpan TotalTime, TimeSpan currentAudio)
        {
            if (lblTempoTotal.Text == "00:00:00")
            {
                lblTempoTotal.Text = $"{TotalTime.Hours}:{TotalTime.Minutes}:{TotalTime.Seconds}";
                int currentMilliseconds = (int)TotalTime.TotalMilliseconds;
                trackBarStateAudio.Maximum = (int)currentMilliseconds;
                trackBarStateAudio.Minimum = 0;
            }
            if (currentAudio.TotalMilliseconds != 0)
            {
                tbAccountant.Text = $"{currentAudio.Hours}:{currentAudio.Minutes}:{currentAudio.Seconds}";
                int currentMilliseconds = (int)currentAudio.TotalMilliseconds;
                int value = currentMilliseconds;
                if (value < trackBarStateAudio.Maximum)
                {
                    trackBarStateAudio.Value = value;
                }
                else
                {
                    trackBarStateAudio.Value = trackBarStateAudio.Maximum;
                }
            }
        }

        public void displayTotalTime(TimeSpan totalTime)
        {
            if (lblTempoTotal.Text == "00:00:00")
            {
                lblTempoTotal.Text = $"{totalTime.Hours}:{totalTime.Minutes}:{totalTime.Seconds}";
                int currentMilliseconds = (int)totalTime.TotalMilliseconds;
                trackBarStateAudio.Maximum = (int)currentMilliseconds;
                trackBarStateAudio.Minimum = 0;
            }
        }
        public void bindMenuTranslate(LanguageCode languageCode, List<string> languageCodes)
        {
            lblIdiomaIdentificado.Text = languageCode;
            languageCodes.ForEach(language => cbxIdiomas.Items.Add(language.ToString()));
        }
        public void bindTextContent(List<Helpers.Models.Entity.Transcript> contentText)
        {
            richTextBox1.Text = contentText.ElementAt(0).transcript;
            rcbTraduzido.Clear();
            tabControlBody.SelectedIndex = 1;

        }
        /*public void bindTextTranslator(string translatedText)
        {
            rcbTraduzido.Clear();
            rcbTraduzido.AppendText(translatedText);
        }*/

        public void bindVoicesPolly(List<Voice> voices) {          
            foreach (Voice item in voices)
            {
                cbxPolly.Items.Add(item.Id);
            }
        }

        private void btnPolly_Click(object sender, EventArgs e)
        {
            // controller.setPlayMedia();
            controller.setPlayMediaPolly();
        }

        public async void displayTrancribe(Item item, Segment segment = null)
        {
            if (item != null)
            {
                lblStart.Text = item.start_time.ToString();
                lblEnd.Text = item.end_time.ToString();

                if (segment != null)
                {
                    if (segment.alternatives.Count != 0)
                    {
                        cbAlternative.Items.Clear();
                        foreach (AlternativeSegment alternative in segment.alternatives)
                        {
                            cbAlternative.Items.Add($"{alternative.transcript}"); // - Confidence: {alternative.confidence.ToString("F")}%");
                        }
                        cbAlternative.SelectedIndex = 0;
                    }
                }


                // txtContent.Text = item.alternatives.ElementAt(0).content;
                if (item.alternatives.Count > 0)
                {
                    lblContentActual.Text = item.alternatives.ElementAt(0).content;
                }
              
                /* if (item.alternatives.Count != 0)
                 {
                     cbAlternative.Items.Clear();
                     foreach (Alternatives alternative in item.alternatives)
                     {
                         cbAlternative.Items.Add($"{alternative.content} - Confidence: {alternative.confidence.ToString("F")}%");
                     }
                     cbAlternative.SelectedIndex = 0;
                     txtContent.Text = item.alternatives.ElementAt(0).content;
                 }*/
                lblConfidence.Text = $"{item.averageConfidence}%";
                lblType.Text = item.type;
            }
        }

        public async void displayStatusCurrentProgress(TimeSpan currentAudio)
        {         
            if (currentAudio.TotalMilliseconds != 0)
            {
                tbAccountant.Text = $"{currentAudio.Hours}:{currentAudio.Minutes}:{currentAudio.Seconds}";
                int currentMilliseconds = (int)currentAudio.TotalMilliseconds;
                int value = currentMilliseconds;
                if (value < trackBarStateAudio.Maximum)
                {
                    trackBarStateAudio.Value = value;
                }
                else
                {
                    trackBarStateAudio.Value = trackBarStateAudio.Maximum;
                }
            }
        }

        public void displayDetailsTrancribe(string alternative)
        {
            rtbDetalhes.AppendText(alternative);
        }

        private void cbFilesBucket_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void btnTraduzir_Click(object sender, EventArgs e)
        {
            pgbAnalizerTranslate.Minimum = 0;
            pgbAnalizerTranslate.Maximum = 100;
            int selectIdiomaTranscribe = cbxIdiomas.SelectedIndex;
            if (selectIdiomaTranscribe >= 0) {
                controller.TranslateFromIdioma(selectIdiomaTranscribe);
            }
        }

        public void bindTextTranslator(string translatedText)
        {
            rcbTraduzido.Clear();
            rcbTraduzido.AppendText(translatedText);
        }

        private void cbxPolly_SelectedIndexChanged(object sender, EventArgs e)
        {
            controller.setFromVoicesAsync(cbxPolly.Text);
        }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void tabControlBody_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void btnTrechoTrasncription_Click(object sender, EventArgs e)
        {
            controller.trackAudio();
        }

    }
}
