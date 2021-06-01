using Amazon_Transcribe_Speech_To_Text.Helpers.Interface;
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
        private Mp3FileReader mp3Reader;
        private WaveOutEvent outputDevice;
        private AudioFileReader audioFile;
        private IController controller;
        private bool executeMedia = false;
        private string fileNameMedia = @"D:\UserFiles\Musicas\Audios\rosalina_batista_entrevista.mp3";

        public PlayerMedia(IController controller) {
            this.controller = controller;
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

        public async Task<bool> newFileAudio(string fileName)
        {
            if (outputDevice == null)
            {
                outputDevice = new WaveOutEvent();
                outputDevice.PlaybackStopped += OnPlaybackStopped;
            }
            if (mp3Reader == null)
            {
                string path = searchFile(fileName);
                if (!string.IsNullOrEmpty(path))
                {
                    mp3Reader = new Mp3FileReader(path);
                    outputDevice.Init(mp3Reader);
                    return true;
                }
                else
                {
                    MessageBox.Show("Não foi encontrado o arquivo");
                    return false;
                }
            }
            return false;

        }
        private string searchFile(string fileName) {
            string pathAudios = @"..\..\..\Audios";
            if (Directory.Exists(pathAudios))
            {
                Console.WriteLine("a");
               // string[] arquivos = Directory.GetFiles(pathAudios);
                string[] arquivos = Directory.GetFiles(pathAudios, "*mp3", SearchOption.AllDirectories);
                for (int i = 0; i < arquivos.Length; i++)
                {
                    string nameFileList = Path.GetFileName(arquivos[i]);
                    if (fileName == nameFileList)
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
