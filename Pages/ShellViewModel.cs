using FfmpegEnkoder.Models;
using MediaInfo;
using MediaInfo.Model;
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
                EncodeInfo.FinishPath = $@"{EncodeInfo.EncodePath}\EnkoderOutput";
            }
        }

        private void ExecuteEncoder()
        {
            var mediaInfo = new MediaInfoWrapper(EncodeInfo.EncodePath);

            var ratio = (Single)mediaInfo.Width / mediaInfo.Height;
            var height = Math.Min(mediaInfo.Height, EncodeInfo.EncodeResolution);
            var width = Math.Ceiling(height * ratio);
            if (width == 1279)
                width++;

            var scale = $"{width}x{height}";
            var frames = Math.Ceiling(mediaInfo.BestVideoStream.Duration.TotalSeconds * mediaInfo.Framerate);

            var outputFile = new FileInfo(Path.ChangeExtension(EncodeInfo.EncodePath, ".mkv"));

            var videoFilter = $"-vf scale={scale}";
            if (mediaInfo.Height == EncodeInfo.EncodeResolution)
                videoFilter = string.Empty;

            //var audioTrack = FindBestAudioTrack(mediaInfo.AudioStreams.ToArray(), false);
            //var audioMap = $"-map -0:a -map 0:a:{audioTrack}";

            var startInfo = new ProcessStartInfo(ffmpegPath)
            {
                Arguments =
                //-preset ultrafast, superfast, faster, fast, medium, slow, slower, veryslow, placebo - faster = more size, faster encoding
                $"-i \"{EncodeInfo.EncodePath}\" -hide_banner -y -threads 0 -map 0 -c:s copy -c:a aac -b:a {128}k {videoFilter} -c:v libx265 -preset ultrafast -crf 18 -pix_fmt yuv420p -frames:{mediaInfo.BestVideoStream.StreamNumber} {frames} \"{outputFile.FullName}\"",
                //$"-i \"{EncodeInfo.EncodePath}\" -hide_banner -y -threads {0} -map 0 {audioMap} {subtitleMap} -c:s copy -c:a aac -b:a {128}k {videoFilter} -c:v libx265 -preset fast -crf {18} -pix_fmt yuv420p -frames:{mediaInfo.BestVideoStream.StreamNumber} {frames} \"{outputFile.FullName}\"";
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardError = true
            };

            var process = Process.Start(startInfo);
            _processes.Add(process);

            //_processes.Remove(process);
        }

        public static int FindBestAudioTrack(AudioStream[] audioStreams, bool japanesePrority = true)
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
        }

        public void OnEncodePathSet(string newPath)
        {
            EncodeInfo.EncodePath = newPath;
        }

        public void OnFinishPathSet(string newPath)
        {
            EncodeInfo.FinishPath = newPath;
        }

        protected override void OnClose()
        {
            base.OnClose();

            foreach (var process in this._processes.Where(p => !p.HasExited))
            {
                process.Kill();
            }
        }
    }
}
