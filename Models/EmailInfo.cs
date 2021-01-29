
namespace FfmpegEnkoder.Models
{
    public sealed class EmailInfo
    {
        private string _email = "example@gmail.com";

        public string Email
        {
            get
            {
                return _email;
            }
            set
            {
                _email = value;
            }
        }

        private string _password = "yourpassword";

        public string Password
        {
            get
            {
                return _password;
            }
            set
            {
                _password = value;
            }
        }

        private string _emailClient = "other@gmail.com";

        public string EmailClient
        {
            get
            {
                return _emailClient;
            }
            set
            {
                _emailClient = value;
            }
        }
    }
}
