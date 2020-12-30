using FfmpegEnkoder.Models;
using MediaInfo;
using MediaInfo.Model;
using Microsoft.Win32;
using Stylet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FfmpegEnkoder.Pages
{
    public class ShellViewModel : Screen
    {
        private List<Process> _processes = new List<Process>();

        public string ffmpegPath;

        public EncodeInformationModel EncodeInfo { get; set; } = new EncodeInformationModel();

        protected override void OnInitialActivate()
        {
            base.OnInitialActivate();

            ffmpegPath = $@"{AppDomain.CurrentDomain.BaseDirectory}\bin";

            EncodeInfo.PropertyChanged += EncodeInfo_PropertyChanged;
        }

        private void EncodeInfo_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(EncodeInfo.EncodePath):
                    SetupEncoder();
                    break;
            }
        }

        private void SetupEncoder()
        {
            if (EncodeInfo.FinishPath == "" || EncodeInfo.FinishPath == EncodeInfo.EncodePath)
            {
                EncodeInfo.FinishPath = $@"{Path.GetDirectoryName(EncodeInfo.EncodePath)}\EnkoderOutput";
            }
        }

        public void ExecuteEncoder()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            //ofd.InitialDirectory = "c:\\";
            ofd.Filter = "All files (*.*)|*.*";
            ofd.FilterIndex = 2;
            ofd.RestoreDirectory = true;
            ofd.Multiselect = true;

            string[] filePaths;

            if (ofd.ShowDialog() == true)
            {
                filePaths = ofd.FileNames;
            }
            else return;

            for (int i = 0; i < filePaths.Length; i++)
            {
                var fullFile = Path.Combine(EncodeInfo.EncodePath, filePaths[i]);
                var encodeFile = Path.Combine(EncodeInfo.FinishPath, Path.ChangeExtension(filePaths[i], ".mkv"));

                if (!Directory.Exists(EncodeInfo.FinishPath))
                {
                    Directory.CreateDirectory(EncodeInfo.FinishPath);
                }

                var mediaInfo = new MediaInfoWrapper(fullFile);

                var ratio = (Single)mediaInfo.Width / mediaInfo.Height;
                var height = Math.Min(mediaInfo.Height, EncodeInfo.EncodeResolution);
                var width = Math.Ceiling(height * ratio);
                if (width % 2 == 1)
                    width++;

                var scale = $"{width}x{height}";
                var frames = Math.Ceiling(mediaInfo.BestVideoStream.Duration.TotalSeconds * mediaInfo.Framerate);

                var videoFilter = $"-vf scale={scale}";
                if (mediaInfo.Height == EncodeInfo.EncodeResolution)
                    videoFilter = string.Empty;

                //var audioTrack = FindBestAudioTrack(mediaInfo.AudioStreams.ToArray(), false);
                //var audioMap = $"-map -0:a -map 0:a:{audioTrack}";

                var startInfo = new ProcessStartInfo(ffmpegPath)
                {
                    Arguments =
                    //-preset ultrafast, superfast, faster, fast, medium, slow, slower, veryslow, placebo - faster = more size, faster encoding
                    $"-i \"{fullFile}\" -hide_banner -y -threads 0 -map 0 -c:s copy -c:a aac -b:a {128}k {videoFilter} -c:v libx265 -preset ultrafast -crf 18 -pix_fmt yuv420p -frames:{mediaInfo.BestVideoStream.StreamNumber} {frames} \"{encodeFile}\"",
                    //$"-i \"{EncodeInfo.EncodePath}\" -hide_banner -y -threads {0} -map 0 {audioMap} {subtitleMap} -c:s copy -c:a aac -b:a {128}k {videoFilter} -c:v libx265 -preset fast -crf {18} -pix_fmt yuv420p -frames:{mediaInfo.BestVideoStream.StreamNumber} {frames} \"{outputFile.FullName}\"";
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardError = true
                };

                var process = Process.Start(startInfo);//TODO; put in different thread outside the UI
                _processes.Add(process);

                process.WaitForExit();

                _processes.Remove(process);
            }
        }

        /*public static int FindBestAudioTrack(AudioStream[] audioStreams, bool japanesePrority = true)
        {
            if (audioStreams.Length == 1)
                return 0;

            if (japanesePrority)
            {
                for (var i = 0; i < audioStreams.Length; i++)
                {
                    if (audioStreams[i].Language == "Japanese")
                        return i;
                }
            }

            // return the default audio.
            return 0;
        }*/

        public void EncodePathSet(string newPath)
        {
            EncodeInfo.EncodePath = newPath;
        }

        public void FinishPathSet(string newPath)
        {
            EncodeInfo.FinishPath = newPath;
        }

        protected override void OnClose()
        {
            base.OnClose();

            foreach (var process in _processes.Where(p => !p.HasExited))
            {
                process.Kill();
            }
        }
    }
}
