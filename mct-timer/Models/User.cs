﻿using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using System.Collections;
using System.Text.Json.Serialization;
using System.Diagnostics;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;

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
        [Required(AllowEmptyStrings = false, ErrorMessage = "Your Email is required")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string Email { get; set; }

        [Display(Name = "Password")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Your Password is required")]
        [StringLength(255, ErrorMessage = "Must be 6 or more characters", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DefaultValue(null)]
        public string DefTZ { get; set; }

        [JsonIgnore]
        public string Altcha { get; set; }

        [DefaultValue(true)]
        public bool Ampm { get; set; }

        [DefaultValue(Languages.English)]
        public Languages Language { get; set; }

        public Dictionary<string,bool> DefBGHidden { get; set; }
        public List<DateTime> AIActivity { get; set; }
        public List<Background> Backgrounds { get; set; }
 
        public List<String> PwdResets { get; set; }

        public User() {
            Backgrounds = new List<Background>();
            DefBGHidden = new Dictionary<string,bool>();
            AIActivity = new List<DateTime>();
            PwdResets = new List<String>();
        }


        public int HowManyActivityAllowed(int maxAI)
        {
            if (AIActivity != null)
            {
                AIActivity = AIActivity.OrderByDescending(x => x).ToList();
                var range = AIActivity.TakeWhile(x => DateTime.Compare(DateTime.Now.AddHours(-24), x) < 0).ToList();
                return maxAI - range.Count();
            }

            return maxAI; //no records
        }

        public string WhenAIAvaiable(int maxAI)
        {
            if (AIActivity != null)
            {
                AIActivity = AIActivity.OrderByDescending(x => x).ToList();
                var range = AIActivity.TakeWhile(x => DateTime.Compare(DateTime.Now.AddHours(-24), x) < 0).ToList();
                if (range.Count() >= maxAI )
                {
                    var timerange = new TimeSpan(range.Last().Ticks - DateTime.Now.AddHours(-24).Ticks);
                    return $"{timerange.Hours} hours and {timerange.Minutes} minutes";
                }

            }

            return null; //no records
        }
        public bool IsAIActivityAllowed(int maxAI)
        {
            if (AIActivity != null)
            {
                AIActivity = AIActivity.OrderByDescending(x=>x).ToList();
                var range = AIActivity.TakeWhile(x => DateTime.Compare(DateTime.Now.AddHours(-24), x) < 0).ToList();
                if (range.Count() >= maxAI ) { return false; }
            }

            return true; //no records
        }

        public void LoadDefaultBG()
        {
            var bgl0 = new Background()
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
            var bgl1 = new Background()
            {
                id = "L1",
                Author = "system",
                Url = "lab1.jpg",  //short url just name of the file
                Location = "",
                LocationLink = "",
                BgType = PresetType.Lab,
                Locked = true,
                Visible = true,

            };
            var bgl2 = new Background()
            {
                id = "L2",
                Author = "system",
                Url = "lab2.jpg",  //short url just name of the file
                Location = "",
                LocationLink = "",
                BgType = PresetType.Lab,
                Locked = true,
                Visible = true,

            };
            var bgl3 = new Background()
            {
                id = "L3",
                Author = "system",
                Url = "lab3.jpg",  //short url just name of the file
                Location = "",
                LocationLink = "",
                BgType = PresetType.Lab,
                Locked = true,
                Visible = true,

            };

            var bgw0 = new Background()
            {
                id = "W0",
                Author = "system",
                Url = "wait0.jpg",  //short url just name of the file
                Location = "",
                LocationLink = "",
                BgType = PresetType.Wait,
                Locked = true,
                Visible = true,
                Info = "Default background"
            };
            var bgw1 = new Background()
            {
                id = "W1",
                Author = "system",
                Url = "wait1.jpg",  //short url just name of the file
                Location = "",
                LocationLink = "",
                BgType = PresetType.Wait,
                Locked = true,
                Visible = true,
                Info = "Default background"
            };
            var bgw2 = new Background()
            {
                id = "W2",
                Author = "system",
                Url = "wait2.jpg",  //short url just name of the file
                Location = "",
                LocationLink = "",
                BgType = PresetType.Wait,
                Locked = true,
                Visible = true,
                Info = "Default background"
            };
            var bgw3 = new Background()
            {
                id = "W3",
                Author = "system",
                Url = "wait3.jpg",  //short url just name of the file
                Location = "",
                LocationLink = "",
                BgType = PresetType.Wait,
                Locked = true,
                Visible = true,
                Info = "Default background"
            };

            var bgln0 = new Background()
            {
                id = "LN0",
                Author = "system",
                Url = "lunch0.jpg",  //short url just name of the file
                Location = "",
                LocationLink = "",
                BgType = PresetType.Lunch,
                Locked = true,
                Visible = true,
                Info = "Default background"
            };
            var bgln1 = new Background()
            {
                id = "LN1",
                Author = "system",
                Url = "lunch1.jpg",  //short url just name of the file
                Location = "",
                LocationLink = "",
                BgType = PresetType.Lunch,
                Locked = true,
                Visible = true,
                Info = "Default background"
            };            
            var bgln2 = new Background()
            {
                id = "LN2",
                Author = "system",
                Url = "lunch2.jpg",  //short url just name of the file
                Location = "",
                LocationLink = "",
                BgType = PresetType.Lunch,
                Locked = true,
                Visible = true,
                Info = "Default background"
            };             
            var bgln3 = new Background()
            {
                id = "LN3",
                Author = "system",
                Url = "lunch3.jpg",  //short url just name of the file
                Location = "",
                LocationLink = "",
                BgType = PresetType.Lunch,
                Locked = true,
                Visible = true,
                Info = "Default background"
            };

            var bgc0 = new Background()
            {
                id = "C0",
                Author = "system",
                Url = "coffee0.jpg",  //short url just name of the file
                Location = "",
                LocationLink = "",
                BgType = PresetType.Coffee,
                Locked = true,
                Visible = true,
                Info = "Default background"
            };
            var bgc1 = new Background()
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
            var bgc2 = new Background()
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
            var bgc3 = new Background()
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
            var bgc4 = new Background()
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
            var bgc5 = new Background()
            {
                id = "C5",
                Author = "system",
                Url = "coffee5.jpg",  //short url just name of the file
                Location = "",
                LocationLink = "",
                BgType = PresetType.Coffee,
                Locked = true,
                Visible = true,
            };

            this.Backgrounds.AddRange(new []{ 
                bgc0,bgc1,bgc2,bgc3,bgc4,bgc5,
                bgl0,bgl1,bgl2,bgl3,
                bgln0,bgln1,bgln2,bgln3,
                bgw0,bgw1,bgw2,bgw3

            });


            if (this.DefBGHidden == null) this.DefBGHidden = new Dictionary<string, bool>();
            this.Backgrounds.ForEach(bg =>
            {
                bg.Visible = this.DefBGHidden.ContainsKey(bg.id) ? this.DefBGHidden[bg.id] : true;
            });
        }

        public Dictionary<PresetType, int> GetQuote()
        {
            var dic = new Dictionary<PresetType, int>();

            foreach (PresetType tp in Enum.GetValues(typeof(PresetType))) {
                dic[tp] = Backgrounds.Count(x => x.BgType == tp && x.Locked != true);
            }
            
            return dic;
        }

        internal void CleanAllBG()
        {
            this.Backgrounds = new List<Background>();
        }
    }

    public class Background
    {
        [Key]
        public string id { get; set; }     
        public string Author { get; set; }
        public string Url { get; set; }
        [DefaultValue("")]
        [Required]
        public string Info { get; set; }
        [DefaultValue("")]
        public string Location { get; set; }
        public string LocationLink { get; set; }

        [DefaultValue(PresetType.Coffee)]
        [Required]
        public PresetType BgType  {get;set;}

        [DefaultValue(false)]
        public bool Locked { get; set; }

        [DefaultValue(true)]
        public bool Visible { get; set; }
    }

    public class Login
    {
        [HiddenInput()]
        [JsonIgnore]
        public string Altcha { get; set; }

        [Display(Name = "Email")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Your Email is required")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string Email { get; set; }

        [Display(Name = "Password")]
        [PasswordPropertyText]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Your Password is required")]
        public string Password { get; set; }

        [Display(Name = "Repeat Password")]
        [NotMapped]
        [PasswordPropertyText]
        [System.ComponentModel.DataAnnotations.Compare("Password")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Please repeat your Password")]
        public string Password_Conformation { get; set; }

        [HiddenInput()]
        [JsonIgnore]
        public string Tkn { get; set; }

        public static bool IfPasswordStrong(string pswd)
        {
           return
                Regex.IsMatch(pswd, ".*[A-Z]") &&
                Regex.IsMatch(pswd, ".*[0-9]") &&
                Regex.IsMatch(pswd, ".{6}");

        }

        public static bool IsEmail(string email)
        {
            return  Regex.IsMatch(email, @"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$");
        
        }
    }
}
