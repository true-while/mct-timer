namespace mct_timer.Models
{

    public enum PresetType
    {
        Lab = 0,
        Coffee = 1,
        Lunch = 2,
        Wait = 3,
    }

    public class PresetItem
    {
        public PresetItem(int ln, PresetType type)
        {
            Length = ln;
            Type = type;    
        }
        public int Length { get; set; } 
        public PresetType Type { get; set; }
    }

    public class PresetGroup
    {
        public List<PresetItem> Items {  get; set; }
    }
    
    public class Personalization
    {
        public string  TimeZone { get; set; }
        public List<PresetGroup> Groups { get; set; }
        public string Language { get; set; }
        public bool Ampm { get; set; }
        public string CDNUrl { get; set; }
    }
}
