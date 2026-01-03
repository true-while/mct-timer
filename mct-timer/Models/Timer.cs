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

        [DefaultValue(false)]
        public bool IsBing;

    }
}
