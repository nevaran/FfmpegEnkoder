
namespace FfmpegEnkoder.Models
{
    class EncodeInformationModel
    {
        private string encodePath = "";

        /// <summary>
        /// The location (folder or perhaps single file) of the video(s) that will be encoded
        /// </summary>
        public string EncodePath
        {
            get
            {
                return encodePath;
            }
            set
            {
                encodePath = value;
            }
        }

        private string finishPath = "";

        /// <summary>
        /// The location (normally folder directory) where the encoded videos will be
        /// </summary>
        public string FinishPath
        {
            get
            {
                return finishPath;
            }
            set
            {
                finishPath = value;
            }
        }

    }
}
