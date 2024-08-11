using System.ComponentModel.DataAnnotations;

namespace mtt_timer.Models
{
    public class WebSettings
    {

        [Key]
        public Guid ID { get; set; }
        public string Name { get; set; }
        public string User { get; set; }
        public string Prompt { get; set; }
        public Uri? Path { get; set; }
        public string? Description { get; set; }


        public string GetFileName()
        {
            return string.Format("{0}-{1}.{2}", this.User, this.ID.ToString(), "jpg"); ;

        }
    }
}
