using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using System.Collections;

namespace mct_timer.Models
{
    public class User
    {

        public enum Languages
        {
            English =1
        }


        [Display(Name = "Name")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Your name is required")]
        public string Name { get; set; }

        [Key]
        [Display(Name = "Email")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Your email is required")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string Email { get; set; }

        [Display(Name = "Password")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Your password is required")]
        [StringLength(255, ErrorMessage = "Must be between 8 and 255 characters", MinimumLength = 8)]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DefaultValue(null)]
        public string DefTZ { get; set; }

        [DefaultValue(true)]
        public bool Ampm { get; set; }

        [DefaultValue(Languages.English)]
        public Languages Language { get; set; }

        public List<Background> Backgrounds { get; set; }

        public User() {
            Backgrounds = new List<Background>();
        }

        public void LoadDefaultBG()
        {
            var bg1 = new Background()
            {
                id = "L0",
                Author = "system",
                Url = "lab0.jpg",  //short url just name of the file
                Location = "",
                LocationLink = "",
                BgType = PresetType.Lab,
                Locked = true,
                Visible = true,
                
            };

            var bg2 = new Background()
            {
                id = "W0",
                Author = "system",
                Url = "wait0.jpg",  //short url just name of the file
                Location = "",
                LocationLink = "",
                BgType = PresetType.Wait,
                Locked = true,
                Visible = true,
            };
            var bg3 = new Background()
            {
                id = "W0",
                Author = "system",
                Url = "lunch0.jpg",  //short url just name of the file
                Location = "",
                LocationLink = "",
                BgType = PresetType.Lunch,
                Locked = true,
                Visible = true,

            };
            var bg4 = new Background()
            {
                id = "C0",
                Author = "system",
                Url = "coffee0.jpg",  //short url just name of the file
                Location = "",
                LocationLink = "",
                BgType = PresetType.Coffee,
                Locked = true,
                Visible = true,
            };
            var bg5 = new Background()
            {
                id = "C1",
                Author = "system",
                Url = "coffee1.jpg",  //short url just name of the file
                Location = "",
                LocationLink = "",
                BgType = PresetType.Coffee,
                Locked = true,
                Visible = true,
            };
            var bg6 = new Background()
            {
                id = "C2",
                Author = "system",
                Url = "coffee2.jpg",  //short url just name of the file
                Location = "",
                LocationLink = "",
                BgType = PresetType.Coffee,
                Locked = true,
                Visible = true,
            };
            var bg7 = new Background()
            {
                id = "C3",
                Author = "system",
                Url = "coffee3.jpg",  //short url just name of the file
                Location = "",
                LocationLink = "",
                BgType = PresetType.Coffee,
                Locked = true,
                Visible = true,
            };
            var bg8 = new Background()
            {
                id = "C4",
                Author = "system",
                Url = "coffee4.jpg",  //short url just name of the file
                Location = "",
                LocationLink = "",
                BgType = PresetType.Coffee,
                Locked = true,
                Visible = true,
            };

            this.Backgrounds.AddRange(new []{ bg1,bg2,bg3,bg4,bg5,bg6,bg7,bg8});
        }


        public Dictionary<PresetType, int> GetQuote()
        {
            var dic = new Dictionary<PresetType, int>();

            foreach (PresetType tp in Enum.GetValues(typeof(PresetType))) {
                dic[tp] = Backgrounds.Count(x => x.BgType == tp && x.Locked != true);
            }
            
            return dic;
        }
    }

    public class Background
    {
        [Key]
        public string id { get; set; }     
        public string Author { get; set; }
        public string Url { get; set; }
        public string Info { get; set; }
        public string Location { get; set; }
        public string LocationLink { get; set; }
        public PresetType BgType  {get;set;}
        [DefaultValue(false)]
        public bool Locked { get; set; }
        [DefaultValue(true)]
        public bool Visible { get; set; }
    }

        public class Login
    {
        [Display(Name = "Email")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Your email is required")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string email { get; set; }

        [Display(Name = "Password")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Your password is required")]
        public string password { get; set; }
    }
}
