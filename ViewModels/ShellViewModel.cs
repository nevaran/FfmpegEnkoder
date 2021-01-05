using FfmpegEnkoder.Models;
using MediaInfo;
using Microsoft.Win32;
using Stylet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;

namespace FfmpegEnkoder.ViewModels
{
    public class ShellViewModel : Screen
    {
        private readonly List<Process> _processes = new List<Process>();

        private float _videoDurationSeconds = 0;

        public float VideoDurationSeconds
        {
            get
            {
                return _videoDurationSeconds;
            }
            set
            {
                SetAndNotify(ref _videoDurationSeconds, value);
            }
        }

        string[] filePaths;

        public EncodeInformationModel EncodeInfo { get; set; } = new EncodeInformationModel();
        public EncodeParametersModel EncodeParams { get; set; } = new EncodeParametersModel();

        protected override void OnInitialActivate()
        {
            base.OnInitialActivate();

            EncodeInfo.FfmpegPath = $"{AppDomain.CurrentDomain.BaseDirectory}\\bin\\ffmpeg.exe";

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
            FinishPathSet($"{EncodeInfo.EncodePath}\\EnkoderOutput");
        }

        public void OnOpenFiles()
        {
            OpenFileDialog ofd = new OpenFileDialog
            {
                Filter = "All files (*.*)|*.*",
                Multiselect = true
            };

            if (ofd.ShowDialog() == true)
            {
                filePaths = ofd.FileNames;
                EncodePathSet(Path.GetDirectoryName(ofd.FileNames[0]));

                var mediaInfo = new MediaInfoWrapper(filePaths[0]);

                //get video duration in seconds (with floating points)
                VideoDurationSeconds = (float)mediaInfo.Duration / 1000;//mediaInfo.Duration 1s == 1000ms

                EncodeInfo.EncodingStatus = "Files Opened:\n";
                for (int i = 0; i < filePaths.Length; i++)
                {
                    EncodeInfo.EncodingStatus += $"[{i + 1}/{filePaths.Length}] {filePaths[i]}\n";
                }
            }
            else
            {
                //EncodeInfo.IsNotEncoding = true;
                return;
            }
        }

        public void OnExecuteEncoder()
        {
            if (filePaths == null || filePaths.Length < 1)
            {
                EncodeInfo.EncodingStatus = "No files selected!";
                return;
            }

            EncodeInfo.IsNotEncoding = false;
            ThreadPool.QueueUserWorkItem(ExecuteEncoder);
        }

        public void ExecuteEncoder(Object stateInfo)
        {
            //EncodeInfo.EncodingStatus = string.Empty;
            EncodeInfo.EncodingStatus = $"preset:{EncodeParams.EncodePreset[EncodeParams.EncodePresetIndex]}; CRF:{EncodeParams.CrfQuality}; Threads:{EncodeParams.UsedThreads}\n\n";

            for (int i = 0; i < filePaths.Length; i++)
            {
                var fullFile = filePaths[i];
                var encodeFile = Path.Combine(EncodeInfo.FinishPath, Path.ChangeExtension(Path.GetFileName(filePaths[i]), $".{EncodeParams.Format[EncodeParams.FormatIndex]}"));

                if (!File.Exists(fullFile))
                {
                    EncodeInfo.EncodingStatus += $"[{i + 1}/{filePaths.Length}] Could not find file: {filePaths[i]}\n";
                    return;
                }

                EncodeInfo.EncodingStatus += $"[{i + 1}/{filePaths.Length}] Encoding {fullFile}\n";

                if (!Directory.Exists(EncodeInfo.FinishPath))
                {
                    Directory.CreateDirectory(EncodeInfo.FinishPath);
                }

                var mediaInfo = new MediaInfoWrapper(fullFile);

                //var ratio = (Single)mediaInfo.Width / mediaInfo.Height;
                //var height = Math.Min(mediaInfo.Height, EncodeInfo.EncodeResolution);
                //var height = mediaInfo.Height;
                //var width = mediaInfo.Width;
                //var width = Math.Ceiling(height * ratio);
                //if (width % 2 == 1)
                    //width++;

                //var scale = $"{width}x{height}";
                //var frames = Math.Ceiling(mediaInfo.BestVideoStream.Duration.TotalSeconds * mediaInfo.Framerate);

                //var audioTrack = FindBestAudioTrack(mediaInfo.AudioStreams.ToArray(), false);
                //var audioMap = $"-map -0:a -map 0:a:{audioTrack}";

                var startInfo = new ProcessStartInfo(EncodeInfo.FfmpegPath)
                {
                    Arguments =
                    //-preset ultrafast, superfast, faster, fast, medium, slow, slower, veryslow, placebo - faster = more size, faster encoding
                    //-crf 18 - 0 = identical to input (takes a long time); higher number = lower quality
                    $"-i \"{fullFile}\" -ss {EncodeParams.TrimStartSeconds} -hide_banner -y -threads {EncodeParams.UsedThreads} -map 0 -c:s copy -c:a aac -b:a {128}k -c:v libx{EncodeParams.Encoder[EncodeParams.EncoderIndex]} -preset {EncodeParams.EncodePreset[EncodeParams.EncodePresetIndex]} -crf {EncodeParams.CrfQuality} -pix_fmt yuv420p \"{encodeFile}\"",
                    //$"-i \"{EncodeInfo.EncodePath}\" -hide_banner -y -threads {0} -map 0 {audioMap} {subtitleMap} -c:s copy -c:a aac -b:a {128}k {videoFilter} -c:v libx265 -preset fast -crf {18} -pix_fmt yuv420p -frames:{mediaInfo.BestVideoStream.StreamNumber} {frames} \"{outputFile.FullName}\"";
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardError = true
                };

                var lastPercentage = 0d;

                void ProgressReport(Object sender, DataReceivedEventArgs e)
                {
                    if (e?.Data == null)
                        return;

                    var chunks = e.Data.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    var time = chunks.FirstOrDefault(c => c.StartsWith("time="));

                    var speed = 0d;

                    for (var i = 0; i < chunks.Length; i++)
                    {
                        var chunk = chunks[i];
                        if (chunk.StartsWith("speed="))
                        {
                            if (chunk.Length == 6)
                                chunk = chunks[i + 1];
                            else chunk = chunk[6..];

                            speed = Double.Parse(chunk[..^1]);

                            break;
                        }
                    }

                    if (String.IsNullOrWhiteSpace(time))
                        return;

                    double encodedTime = TimeSpan.Parse(time[5..]).TotalSeconds;
                    double totalTime = mediaInfo.BestVideoStream.Duration.TotalSeconds;

                    var percentage = encodedTime / totalTime;
                    if (percentage > lastPercentage)
                    {
                        EncodeInfo.EncodeSpeedString = speed;

                        EncodeInfo.ProgressPercentage = percentage;
                        lastPercentage = percentage;
                    }
                }

                var process = Process.Start(startInfo);
                _processes.Add(process);

                if (mediaInfo.BestVideoStream is not null)
                {
                    process.ErrorDataReceived += ProgressReport;
                    process.BeginErrorReadLine();
                }
                
                process.WaitForExit();
                if (mediaInfo.BestVideoStream is not null)
                    process.ErrorDataReceived -= ProgressReport;

                _processes.Remove(process);

                EncodeInfo.ProgressPercentage = 1;//to show 100% in the view
            }

            EncodeInfo.IsNotEncoding = true;//all queued encoding is done and we can open the ability for new encoding

            EncodeInfo.EncodingStatus += $"\nEncoding Done!";
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

        public void OnEncoderPath()
        {
            if (Directory.Exists(EncodeInfo.EncodePath))
            {
                Process.Start("explorer.exe", string.Format("/open,\"{0}\"", Path.GetFullPath(EncodeInfo.EncodePath)));
            }
        }

        public void OnFinishedPath()
        {
            if (Directory.Exists(EncodeInfo.FinishPath))
            {
                Process.Start("explorer.exe", string.Format("/open,\"{0}\"", Path.GetFullPath(EncodeInfo.FinishPath)));
                //Process.Start("explorer.exe", string.Format("/select,\"{0}\"", Path.GetFullPath(EncodeInfo.FinishPath)));
            }
        }

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
            /*MessageBoxResult result = MessageBox.Show("There is an encoding still running! Quit anyway?", "Encoding In Progress", MessageBoxButton.YesNo);
            switch (result)
            {
                case MessageBoxResult.Yes:
                    break;
                case MessageBoxResult.No:
                    return;
            }*/

            foreach (var process in _processes.Where(p => !p.HasExited))
            {
                process.Kill();
            }

            base.OnClose();
        }
    }

    public static class GifExtension
    {
        public static TimeSpan? GetGifDuration(this Image image, int fps = 60)
        {
            var minimumFrameDelay = (1000.0 / fps);
            if (!image.RawFormat.Equals(ImageFormat.Gif)) return null;
            if (!ImageAnimator.CanAnimate(image)) return null;

            var frameDimension = new FrameDimension(image.FrameDimensionsList[0]);

            var frameCount = image.GetFrameCount(frameDimension);
            var totalDuration = 0;

            for (var f = 0; f < frameCount; f++)
            {
                var delayPropertyBytes = image.GetPropertyItem(20736).Value;
                var frameDelay = BitConverter.ToInt32(delayPropertyBytes, f * 4) * 10;
                // Minimum delay is 16 ms. It's 1/60 sec i.e. 60 fps
                totalDuration += (frameDelay < minimumFrameDelay ? (int)minimumFrameDelay : frameDelay);
            }

            return TimeSpan.FromMilliseconds(totalDuration);
        }
    }
}
