using FfmpegEnkoder.Models;
using FfmpegEnkoder.Services;
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
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Shell;

namespace FfmpegEnkoder.ViewModels
{
    public class ShellViewModel : Screen
    {
        private readonly List<Process> _processes = new();

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

        private double _totalProgress = 0;

        public double TotalProgress
        {
            get
            {
                return _totalProgress;
            }
            set
            {
                SetAndNotify(ref _totalProgress, value);
            }
        }

        private TaskbarItemProgressState _progressState = TaskbarItemProgressState.None;

        public TaskbarItemProgressState ProgressState
        {
            get
            {
                return _progressState;
            }
            set
            {
                SetAndNotify(ref _progressState, value);
            }
        }

        string[] filePaths;

        public EncodeInformationModel EncodeInfo { get; set; } = new EncodeInformationModel();
        public EncodeParametersModel EncodeParams { get; set; } = new EncodeParametersModel();

        TimeSpan gifDuration = TimeSpan.Zero;

        protected override void OnInitialActivate()
        {
            base.OnInitialActivate();

            EncodeInfo.FfmpegPath = $"{AppDomain.CurrentDomain.BaseDirectory}\\bin\\ffmpeg.exe";

            EncodeInfo.PropertyChanged += EncodeInfo_PropertyChanged;

            NotifyByEmail.WriteEmailDefaultInfo();
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
            OpenFileDialog ofd = new()
            {
                Filter = "All files (*.*)|*.*",
                Multiselect = true
            };

            if (ofd.ShowDialog() == true)
            {
                OpenFiles(ofd.FileNames);
            }
            else
            {
                //EncodeInfo.IsNotEncoding = true;
                return;
            }
        }

        public void OpenFiles(string[] newFiles)
        {
            filePaths = newFiles;
            EncodePathSet(Path.GetDirectoryName(newFiles[0]));

            MediaInfoWrapper mediaInfo = new(filePaths[0]);

            //get video duration in seconds (with floating points)
            VideoDurationSeconds = (float)mediaInfo.Duration / 1000;//mediaInfo.Duration = ms

            EncodeParams.TrimEndSeconds = VideoDurationSeconds;
            EncodeParams.TrimStartSeconds = 0;

            EncodeInfo.EncodingStatus = "Files Opened:\n";
            for (int i = 0; i < filePaths.Length; i++)
            {
                EncodeInfo.EncodingStatus += $"[{i + 1}/{filePaths.Length}] {filePaths[i]}\n";
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

        public void ExecuteEncoder(object stateInfo)
        {
            DateTime startTime = DateTime.Now;

            EncodeInfo.EncodingStatus = $"Started encoding - {startTime}\n";
            ProgressState = TaskbarItemProgressState.Normal;

            for (int i = 0; i < filePaths.Length; i++)
            {
                EncodeInfo.debugLinesBuilder.Clear();
                EncodeInfo.EncodingDebug = string.Empty;

                string fullFile = filePaths[i];
                string encodeFile = Path.Combine(EncodeInfo.FinishPath, Path.ChangeExtension(Path.GetFileName(filePaths[i]), $".{EncodeParams.Format[EncodeParams.FormatIndex]}"));

                if (!File.Exists(fullFile))
                {
                    EncodeInfo.EncodingStatus += $"[{i + 1}/{filePaths.Length}] Could not find file: {filePaths[i]}\n";
                }
                else
                {
                    EncodeInfo.EncodingStatus += $"[{i + 1}/{filePaths.Length}] Encoding {fullFile}\n";

                    if (!Directory.Exists(EncodeInfo.FinishPath))
                    {
                        Directory.CreateDirectory(EncodeInfo.FinishPath);
                    }

                    MediaInfoWrapper mediaInfo = new(fullFile);

                    //var ratio = (Single)mediaInfo.Width / mediaInfo.Height;
                    //var height = Math.Min(mediaInfo.Height, EncodeInfo.EncodeResolution);
                    //var height = mediaInfo.Height;
                    //var width = mediaInfo.Width;
                    //var width = Math.Ceiling(height * ratio);
                    //if (width % 2 == 1)
                    //width++;

                    //var scale = $"{width}x{height}";
                    double frames = 0;
                    if (Path.GetExtension(fullFile).ToLower() != ".gif" && Path.GetExtension(encodeFile).ToLower() != ".gif")
                        frames = Math.Ceiling(mediaInfo.BestVideoStream.Duration.TotalSeconds * mediaInfo.Framerate);

                    //var audioTrack = FindBestAudioTrack(mediaInfo.AudioStreams.ToArray(), false);
                    //var audioMap = $"-map -0:a -map 0:a:{audioTrack}";

                    string notWebmArg = "";
                    if (Path.GetExtension(encodeFile).ToLower() != ".webm")
                    {
                        notWebmArg = $"libx{EncodeParams.Encoder[EncodeParams.EncoderIndex]}";
                    }
                    else
                    {
                        notWebmArg = "libvpx-vp9";//use webm encoder if its set as webm
                    }

                    string forTrimStart = "";
                    if (EncodeParams.TrimStartSeconds > 0)
                    {
                        forTrimStart = $" -ss {EncodeParams.TrimStartSeconds}";
                    }

                    string forTrimEnd = "";
                    if (EncodeParams.TrimEndSeconds != VideoDurationSeconds)
                    {
                        forTrimEnd = $" -to {EncodeParams.TrimEndSeconds}";
                    }

                    string noAudio = "";
                    if (EncodeParams.NoAudio)
                    {
                        noAudio = " -an";
                    }

                    var startInfo = new ProcessStartInfo(EncodeInfo.FfmpegPath)
                    {
                        //$"-i \"{EncodeInfo.EncodePath}\" -hide_banner -y -threads {0} -map 0 {audioMap} {subtitleMap} -c:s copy -c:a aac -b:a {128}k {videoFilter} -c:v libx265 -preset fast -crf {18} -pix_fmt yuv420p -frames:{mediaInfo.BestVideoStream.StreamNumber} {frames} \"{outputFile.FullName}\"";
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardError = true
                    };
                    //-preset ultrafast, superfast, faster, fast, medium, slow, slower, veryslow, placebo - faster = more size, faster encoding
                    //-crf 18 - 0 = identical to input (takes a long time); higher number = lower quality

                    if (Path.GetExtension(encodeFile).ToLower() == ".apng" || Path.GetExtension(encodeFile).ToLower() == ".png")//any -> apng
                    {
                        startInfo.Arguments =
                        $"-i \"{fullFile}\" -hide_banner -plays 0 -y -f apng \"{encodeFile}\"";
                    }
                    else if (Path.GetExtension(fullFile).ToLower() == ".gif" && Path.GetExtension(encodeFile).ToLower() != ".gif")//gif -> video
                    {
                        startInfo.Arguments =
                        $"-i \"{fullFile}\" -hide_banner -y -threads {EncodeParams.UsedThreads} -c:v {notWebmArg} -preset {EncodeParams.EncodePreset[EncodeParams.EncodePresetIndex]} -crf {EncodeParams.CrfQuality} -pix_fmt yuv420p \"{encodeFile}\"";
                    }
                    else
                    {
                        if (Path.GetExtension(encodeFile).ToLower() == ".gif")//any -> gif
                        {
                            startInfo.Arguments =
                            $"-i \"{fullFile}\" -hide_banner -y -threads {EncodeParams.UsedThreads} -vf \"scale = {mediaInfo.Width}:-1:flags = lanczos,split[s0][s1];[s0]palettegen[p];[s1][p]paletteuse\" -loop 0 \"{encodeFile}\"";
                        }
                        else//video -> video
                        {
                            startInfo.Arguments =
                            $"-i \"{fullFile}\"{forTrimStart}{forTrimEnd} -hide_banner -y -threads {EncodeParams.UsedThreads} -c:v {notWebmArg} -preset {EncodeParams.EncodePreset[EncodeParams.EncodePresetIndex]} -crf {EncodeParams.CrfQuality} -pix_fmt yuv420p{noAudio} \"{encodeFile}\"";
                        }
                    }

                    EncodeInfo.EncodeArguments = startInfo.Arguments;

                    var lastPercentage = 0d;

                    void ProgressReport(object sender, DataReceivedEventArgs e)
                    {
                        if (e?.Data == null)
                            return;

                        //for debug logging
                        EncodeInfo.debugLinesBuilder.Add(e.Data);
                        if (EncodeInfo.debugLinesBuilder.Count > 20 * (i + 1))
                            EncodeInfo.debugLinesBuilder.RemoveAt(EncodeInfo.debugLinesBuilder.Count-1);
                        EncodeInfo.EncodingDebug = string.Join("\n", EncodeInfo.debugLinesBuilder);

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

                                _ = double.TryParse(chunk[..^1], out speed);

                                break;
                            }
                        }

                        if (string.IsNullOrWhiteSpace(time))
                            return;

                        double encodedTime = TimeSpan.Parse(time[5..]).TotalSeconds;
                        double totalTime = 1;
                        if (mediaInfo.BestVideoStream is not null)
                            totalTime = mediaInfo.BestVideoStream.Duration.TotalSeconds;
                        else
                            totalTime = gifDuration.TotalSeconds;

                        var percentage = encodedTime / totalTime;
                        if (percentage > lastPercentage)
                        {
                            EncodeInfo.EncodeSpeedString = speed;

                            EncodeInfo.ProgressPercentage = percentage;
                            lastPercentage = percentage;

                            TotalProgress = (i / (double)filePaths.Length) + (percentage / filePaths.Length);
                            //Debug.WriteLine(TotalProgress.ToString());
                        }
                    }

                    var process = Process.Start(startInfo);
                    _processes.Add(process);

                    if (Path.GetExtension(fullFile).ToLower() == ".gif")
                    {
                        gifDuration = GetGifDuration(Image.FromFile(fullFile));
                    }

                    process.ErrorDataReceived += ProgressReport;
                    process.BeginErrorReadLine();

                    process.WaitForExit();
                    process.ErrorDataReceived -= ProgressReport;

                    _processes.Remove(process);

                    EncodeInfo.ProgressPercentage = 1;//to show 100% in the view
                }
            }

            EncodeInfo.IsNotEncoding = true;//all queued encoding is done and we can open the ability for new encoding

            var endTime = DateTime.Now;
            EncodeInfo.EncodingStatus += $"\nEncoding Done! - {endTime}";

            TotalProgress = 100;
            ProgressState = TaskbarItemProgressState.None;

            if ((endTime - startTime).Minutes > 20)//send email only if it took longer than set amount of minutes
                NotifyByEmail.SendNotify();
        }

        public static TimeSpan GetGifDuration(Image image, int fps = 60)
        {
            var minimumFrameDelay = (1000.0 / fps);
            if (!image.RawFormat.Equals(ImageFormat.Gif)) return TimeSpan.Zero;
            if (!ImageAnimator.CanAnimate(image)) return TimeSpan.Zero;

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

        public void OnOpenEncoderPath()
        {
            if (Directory.Exists(EncodeInfo.EncodePath))
            {
                Process.Start("explorer.exe", string.Format("/open,\"{0}\"", Path.GetFullPath(EncodeInfo.EncodePath)));
            }
        }

        public void OnOpenFinishedPath()
        {
            if (Directory.Exists(EncodeInfo.FinishPath))
            {
                Process.Start("explorer.exe", string.Format("/open,\"{0}\"", Path.GetFullPath(EncodeInfo.FinishPath)));
                //Process.Start("explorer.exe", string.Format("/select,\"{0}\"", Path.GetFullPath(EncodeInfo.FinishPath)));
            }
        }

        public void OnMedia_Drop(object sender, DragEventArgs e)
        {
            if (EncodeInfo.IsNotEncoding == false) return;//encoding in progress, dont do anything

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                // Note that you can have more than one file.
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                // Assuming you have one file that you care about, pass it off; if there are no valid files it will still open the folder but have an empty list
                if (files.Length > 0)
                    OpenFiles(files);
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

        /*
        public void OnApplicationExit()
        {
            Application.Current.Shutdown();
        }
        */

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
