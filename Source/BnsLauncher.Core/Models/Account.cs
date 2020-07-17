namespace BnsLauncher.Core.Models
{
    public class Account : PropertyChangedBase
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Pin { get; set; }
    }
}