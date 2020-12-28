using FfmpegEnkoder.Models;
using Stylet;

namespace FfmpegEnkoder.Pages
{
    public class ShellViewModel : Screen
    {
        EncodeInformationModel EncodeInfo { get; set; } = new EncodeInformationModel();

        protected override void OnInitialActivate()
        {
            base.OnInitialActivate();


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
