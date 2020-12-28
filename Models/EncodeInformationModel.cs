
using Stylet;

namespace FfmpegEnkoder.Models
{
    public class EncodeInformationModel : PropertyChangedBase
    {
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
                _finishPath = value;
            }
        }

        private int _encodeResolution = 720;

        public int EncodeResolution
        {
            get
            {
                return _encodeResolution;
            }
            set
            {
                SetAndNotify(ref _encodeResolution, value);
            }
        }

    }
}