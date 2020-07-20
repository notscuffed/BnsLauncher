using BnsLauncher.Core.Helpers;
using Newtonsoft.Json;

namespace BnsLauncher.Core.Models
{
    public class Account : PropertyChangedBase
    {
        public string Username { get; set; }
        [JsonIgnore] public string Password { get; set; }
        [JsonIgnore] public string Pin { get; set; }
        public string ProfilePatterns { get; set; }

        [JsonProperty("Password")]
        public string EncryptedPassword
        {
            get => Encryption.Encrypt(Password);
            set => Password = Encryption.Decrypt(value);
        }
        
        [JsonProperty("Pin")]
        public string EncryptedPin
        {
            get => Encryption.Encrypt(Pin);
            set => Pin = Encryption.Decrypt(value);
        }
    }
}