using Microsoft.AspNetCore.Identity;

namespace web_app_template.Domain.Models.Entities
{
    //write custom properties for IdentityUser here
    public class CustomIdentityUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MotherLastName { get; set; }
        public string ProfilePicture { get; set; }
        public string RefreshToken { get; set; }
        public DateTime RefreshTokenExpiryTime { get; set; }
    }
}
