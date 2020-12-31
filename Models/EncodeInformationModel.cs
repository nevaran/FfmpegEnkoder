
using Stylet;

namespace FfmpegEnkoder.Models
{
    public class EncodeInformationModel : PropertyChangedBase
    {
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

        private double _progressPercentage = 0;

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
                return $"Encoding: {(int)(ProgressPercentage * 100)}% - {ProgressTimeString }";
            }
        }

        private string _progressTimeString = "";

        /// <summary>
        /// How many seconds of the video have been encoded
        /// </summary>
        public string ProgressTimeString
        {
            get
            {
                return _progressTimeString;
            }
            set
            {
                SetAndNotify(ref _progressTimeString, value);
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
    }
}