using Stylet;
using System;

namespace FfmpegEnkoder.Models
{
    public class EncodeParametersModel : PropertyChangedBase
    {
        public string[] EncodePreset { get; } = new string[] { "ultrafast", "superfast", "faster", "fast", "medium", "slow", "slower", "veryslow", "placebo" };

        private int _encodePresetIndex = 4;

        public int EncodePresetIndex
        {
            get
            {
                return _encodePresetIndex;
            }
            set
            {
                SetAndNotify(ref _encodePresetIndex, value);
            }
        }

        private int _crfQuality = 18;

        public int CrfQuality
        {
            get
            {
                return _crfQuality;
            }
            set
            {
                SetAndNotify(ref _crfQuality, value);
            }
        }

        private int _usedThreads = 0;

        public int UsedThreads
        {
            get
            {
                return _usedThreads;
            }
            set
            {
                SetAndNotify(ref _usedThreads, value);
            }
        }

        public static int MaxUsableThreads
        {
            get
            {
                return Environment.ProcessorCount;
            }
        }
    }
}