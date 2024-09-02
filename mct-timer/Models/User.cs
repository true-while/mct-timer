using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace mct_timer.Models
{
    public class User
    {

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
