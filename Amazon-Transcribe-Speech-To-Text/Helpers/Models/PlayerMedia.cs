using Amazon_Transcribe_Speech_To_Text.Helpers.Interface;
using Amazon_Transcribe_Speech_To_Text.Helpers.Models.AWServices;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Amazon_Transcribe_Speech_To_Text.Helpers.Models
{
    public class PlayerMedia
    {
        private AWSS3Service awsS3Sevices;
        private Mp3FileReader mp3Reader;
        private WaveOutEvent outputDevice;
        private AudioFileReader audioFile;
        private IController controller;
        private bool executeMedia = false;
        private string fileNameMedia = @"D:\UserFiles\Musicas\Audios\rosalina_batista_entrevista.mp3";

        public PlayerMedia(IController controller) {
            this.controller = controller;
            this.awsS3Sevices = new AWSS3Service();
        }

        public bool checkExecute() {
            return executeMedia;
        }

        public void clickPaused() {
            outputDevice.Pause();
            executeMedia = checkPlayPause();
        }
        public void clickPlay() {
            outputDevice.Play();
            executeMedia = checkPlayPause();
        }
        public bool checkPlayPause()
        { 
            return (executeMedia == false) ? true : false;
        }
        public async Task newFileAudio(string fileName)
        {
            if (outputDevice == null)
            {
                outputDevice = new WaveOutEvent();
                outputDevice.PlaybackStopped += OnPlaybackStopped;
            }
            if (mp3Reader == null)
            {
                // searchFile();
                // string dire = Directory.GetCurrentDirectory();
                mp3Reader = new Mp3FileReader((string.IsNullOrEmpty(fileName) ? fileNameMedia : fileName));
                outputDevice.Init(mp3Reader);

            }

        }

        public async Task<bool> newFileAudio(AWSUtil file)
        {
            if (outputDevice == null)
            {
                outputDevice = new WaveOutEvent();
                outputDevice.PlaybackStopped += OnPlaybackStopped;
            }
            if (mp3Reader == null)
            {
                string path = searchFile(file.FileNameActual, @"..\..\..\Audios");
                if (!string.IsNullOrEmpty(path))
                {
                    mp3Reader = new Mp3FileReader(path);
                    outputDevice.Init(mp3Reader);
                    return true;
                }
                else
                {
                    DialogResult dialogResult = MessageBox.Show("Download", "Arquivo não encontrado! Deseja Baixa-lo?",
                                                                MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (dialogResult == DialogResult.Yes)
                    {
                        string downloadPath = await awsS3Sevices.DownloadFileS3(file);
                        if (!string.IsNullOrEmpty(downloadPath)) {
                           /* string pathNewFile = searchFile(file);
                            if (!string.IsNullOrEmpty(path))
                            {*/
                                mp3Reader = new Mp3FileReader(downloadPath);
                                outputDevice.Init(mp3Reader);
                                return true;
                           // }
                        }
                        else
                        {
                            MessageBox.Show("O arquivo não foi baixado");
                            return false;
                        }
                    }
                    else
                    {
                        MessageBox.Show("O arquivo não foi baixado");
                        return false;
                    }


                }
            }
            return false;

        }
        public async Task<bool> newFileAudioPolly(string filePath)
        {
            if (outputDevice == null)
            {
                outputDevice = new WaveOutEvent();
                outputDevice.PlaybackStopped += OnPlaybackStopped;
            }
            if (mp3Reader == null)
            {
                string path = searchFile(filePath, @"..\..\..\Audios\Traduzidos");
                if (!string.IsNullOrEmpty(path))
                {
                    mp3Reader = new Mp3FileReader(path);
                    outputDevice.Init(mp3Reader);
                    return true;
                }
                else
                {
                    throw new ApplicationException("Um erro ao encontrar o arquivo");
                }
            }
            return false;

        }
        private string searchFile(string file, string pathAudios) {

            if (Directory.Exists(pathAudios))
            {
                Console.WriteLine("a");
                file = Path.GetFileName(file);
                // string[] arquivos = Directory.GetFiles(pathAudios);
                string[] arquivos = Directory.GetFiles(pathAudios, "*mp3", SearchOption.AllDirectories);
                for (int i = 0; i < arquivos.Length; i++)
                {
                    string nameFileList = Path.GetFileName(arquivos[i]);
                    if (file == nameFileList)
                    {
                        return arquivos[i];
                    }
                }
            }
            // var CurrentDirectory = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

            return null;

        }

        private void OnPlaybackStopped(object sender, StoppedEventArgs args)
        {
            outputDevice.Dispose();
            outputDevice = null;
            audioFile.Dispose();
            audioFile = null;
        }
        public async Task trackAudioA() {
            while (outputDevice.PlaybackState == PlaybackState.Playing)
            {
                await Task.Delay(3000);
                long currentPosition = outputDevice.GetPosition();
                decimal position = ((decimal)currentPosition / (decimal)audioFile.Length)*100;
            }          
        
        }
        public void trackAudioPlay(TimeSpan time) {
            Console.WriteLine(time);
            mp3Reader.CurrentTime = time;
        }

        public PlaybackState getPlaybackState()
        {
            return outputDevice.PlaybackState;
        }

        public TimeSpan getCurrentTime()
        {
           return mp3Reader.CurrentTime;
        }
        public TimeSpan getTotalTimeAudio() {
            return mp3Reader.TotalTime;
        }
    }
}
