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
        public string ffmpegPath = "";

        public EncodeInformationModel EncodeInfo { get; set; } = new EncodeInformationModel();

        protected override void OnInitialActivate()
        {
            base.OnInitialActivate();

            EncodeInfo.PropertyChanged += EncodeInfo_PropertyChanged;
        }

        private async void EncodeInfo_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(EncodeInfo.EncodePath):
                    await ExecuteEncoder();
                    break;
            }
        }

        private async Task ExecuteEncoder()
        {
            var mediaInfo = new MediaInfoWrapper(EncodeInfo.EncodePath);

            var ratio = (Single)mediaInfo.Width / mediaInfo.Height;
            var height = Math.Min(mediaInfo.Height, EncodeInfo.EncodeResolution);
            var width = Math.Ceiling(height * ratio);
            if (width == 1279)
                width++;

            var scale = $"{width}x{height}";
            var frames = Math.Ceiling(mediaInfo.BestVideoStream.Duration.TotalSeconds * mediaInfo.Framerate);

            var outputFile = new FileInfo(Path.Combine(EncodeInfo.FinishPath, Path.ChangeExtension(EncodeInfo.EncodePath, ".mkv")));

            var videoFilter = $"-vf scale={scale}";
            if (mediaInfo.Height == EncodeInfo.EncodeResolution)
                videoFilter = String.Empty;

            var audioTrack = FindBestAudioTrack(mediaInfo.AudioStreams.ToArray());
            var audioMap = $"-map -0:a -map 0:a:{audioTrack}";
                
            var startInfo = new ProcessStartInfo(ffmpegPath);
            startInfo.Arguments =
                $"-i \"{EncodeInfo.EncodePath}\" -hide_banner -y -threads {1} -map 0 {audioMap} {subtitleMap} -c:s copy -c:a aac -b:a {this._options.Bitrate}k {videoFilter} -c:v libx265 -preset fast -crf {this._options.CRF} -pix_fmt yuv420p -frames:{mediaInfo.BestVideoStream.StreamNumber} {frames} {outputFile.FullName.Quote()}";
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true;
            startInfo.RedirectStandardError = true;

            this._logger.Debug("Starting ffmpeg with the following arguments: {arguments}",
                startInfo.Arguments);

            private List<Process> _processes;

            var process = Process.Start(startInfo);
            this._processes.Add(process);
        }

        public int FindBestAudioTrack(AudioStream[] audioStreams, bool japanesePrority = true)
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
    }
}
