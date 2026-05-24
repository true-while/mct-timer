using System.ComponentModel;

namespace mct_timer.Models
{
    public class Timer
    {
        public int Length;
        public string Timezone;
        public bool Ampm;
        public PresetType BreakType;
        public string BGUrl = string.Empty;
        public string SpotifyPlaylistEmbedUrl = string.Empty;
        public string SpotifyPlaylistValidationMessage = string.Empty;
        public string SessionTitle = string.Empty;
        public string CustomerName = string.Empty;
        public string RegionLocation = string.Empty;
        public string ClassHours = string.Empty;
        public string InstructorName = string.Empty;
        public string InstructorLinkedInUrl = string.Empty;
        public string InstructorQrCodeUrl = string.Empty;
        public string InstructorProfileValidationMessage = string.Empty;
        public string ShowcaseMediaUrl = string.Empty;
        public string ShowcaseMediaCaption = string.Empty;
        public string ShowcaseMediaValidationMessage = string.Empty;
        public ShowcaseMediaKind ShowcaseMediaKind = ShowcaseMediaKind.None;
        public string AiBackgroundValidationMessage = string.Empty;
        public bool UseBingDailyBackground;

        [DefaultValue(false)]
        public bool IsBing;

    }
}
