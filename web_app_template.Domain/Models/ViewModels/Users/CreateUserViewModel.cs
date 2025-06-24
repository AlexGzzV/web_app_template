using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace web_app_template.Domain.Models.ViewModels.Users
{
    public class CreateUserViewModel
    {
        [Required(ErrorMessage = "EmailRequired")]
        [EmailAddress(ErrorMessage = "InvalidEmailFormat")]
        public string Email { get; set; }

        [Required(ErrorMessage = "PasswordRequired")]
        public string Password { get; set; }

        [Required(ErrorMessage = "ConfirmPasswordRequired")]
        [Compare("Password", ErrorMessage = "PasswordsDoNotMatch")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "FirstNameRequired")]
        [StringLength(50, ErrorMessage = "FirstNameMaxLength")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "LastNameRequired")]
        [StringLength(50, ErrorMessage = "LastNameMaxLength")]
        public string LastName { get; set; }

        [StringLength(50, ErrorMessage = "MotherLastNameMaxLength")]
        public string MotherLastName { get; set; }

        public IFormFile ProfilePicture { get; set; }

        public int UserType { get; set; }
    }
}
