
using Stylet;

namespace FfmpegEnkoder.Models
{
    public class EncodeInformationModel : PropertyChangedBase
    {
        private string _ffmpegPath = "";

        public string FfmpegPath
        {
            get
            {
                return _ffmpegPath;
            }
            set
            {
                SetAndNotify(ref _ffmpegPath, value);
            }
        }

        private bool _isNotEncoding = true;

        /// <summary>
        /// If the encoder is running or not. Used to lock from another run to be initialized
        /// </summary>
        public bool IsNotEncoding
        {
            get
            {
                return _isNotEncoding;
            }
            set
            {
                SetAndNotify(ref _isNotEncoding, value);
            }
        }

        private string _encodingStatus = "";

        /// <summary>
        /// Logging total progress and events
        /// </summary>
        public string EncodingStatus
        {
            get
            {
                return _encodingStatus;
            }
            set
            {
                SetAndNotify(ref _encodingStatus, value);
            }
        }

        private string _encodingDebug = "";

        /// <summary>
        /// Debugging progress and events
        /// </summary>
        public string EncodingDebug
        {
            get
            {
                return _encodingDebug;
            }
            set
            {
                SetAndNotify(ref _encodingDebug, value);
            }
        }

        private double _progressPercentage = 0d;

        public double ProgressPercentage
        {
            get
            {
                return _progressPercentage;
            }
            set
            {
                SetAndNotify(ref _progressPercentage, value);
                this.NotifyOfPropertyChange(nameof(this.ProgressPercentageFormatted));
            }
        }

        public string ProgressPercentageFormatted
        {
            get
            {
                return $"Encoding: {(int)(ProgressPercentage * 100)}% - Speed: x{EncodeSpeedString}";
            }
        }

        private double _encodeSpeedString = 0d;

        /// <summary>
        /// How many times the video is encoded over real time
        /// </summary>
        public double EncodeSpeedString
        {
            get
            {
                return _encodeSpeedString;
            }
            set
            {
                SetAndNotify(ref _encodeSpeedString, value);
                this.NotifyOfPropertyChange(nameof(this.ProgressPercentageFormatted));
            }
        }

        private string _encodePath = "";

        /// <summary>
        /// The location (folder or perhaps single file) of the video(s) that will be encoded
        /// </summary>
        public string EncodePath
        {
            get
            {
                return _encodePath;
            }
            set
            {
                //_encodePath = value;
                SetAndNotify(ref _encodePath, value);
                //this.NotifyOfPropertyChange(nameof(this._encodePath));
            }
        }

        private string _finishPath = "";

        /// <summary>
        /// The location (normally folder directory) where the encoded videos will be
        /// </summary>
        public string FinishPath
        {
            get
            {
                return _finishPath;
            }
            set
            {
                SetAndNotify(ref _finishPath, value);
            }
        }

        private string _encodeArguments = "";

        public string EncodeArguments
        {
            get
            {
                return _encodeArguments;
            }
            set
            {
                SetAndNotify(ref _encodeArguments, value);
            }
        }

    }
}