using Stylet;

namespace FfmpegEnkoder.Models
{
    public class EncodeParametersModel : PropertyChangedBase
    {
        public string[] Format { get; } = new string[] { "webm", "mkv", "mp4", "gif", "png" };

        private int _formatIndex = 0;

        public int FormatIndex
        {
            get
            {
                return _formatIndex;
            }
            set
            {
                //if (value == 0)//set the recommended webm quality
                    //CrfQuality = 30;

                SetAndNotify(ref _formatIndex, value);
            }
        }

        public string[] Encoder { get; } = new string[] { "265", "264" };

        private int _encodeIndex = 0;

        public int EncoderIndex
        {
            get
            {
                return _encodeIndex;
            }
            set
            {
                SetAndNotify(ref _encodeIndex, value);
            }
        }

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

        private float _trimStartSeconds = 0;

        public float TrimStartSeconds
        {
            get
            {
                if (_trimStartSeconds > _trimEndSeconds) _trimStartSeconds = _trimEndSeconds;//clamp value

                return _trimStartSeconds;
                //return MathF.Round(_trimStartSeconds, 1);
            }
            set
            {
                SetAndNotify(ref _trimStartSeconds, value);
            }
        }

        private float _trimEndSeconds = 0;

        public float TrimEndSeconds
        {
            get
            {
                if (_trimEndSeconds < _trimStartSeconds) _trimEndSeconds = _trimStartSeconds;//clamp value

                return _trimEndSeconds;
            }
            set
            {
                SetAndNotify(ref _trimEndSeconds, value);
            }
        }

        private bool _noAudio = false;

        public bool NoAudio
        {
            get
            {
                return _noAudio;
            }
            set
            {
                SetAndNotify(ref _noAudio, value);
            }
        }
    }
}