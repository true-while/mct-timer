namespace mct_timer.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }    
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class Login
    {
        public string email { get; set; }
        public string password { get; set; }
    }
}
