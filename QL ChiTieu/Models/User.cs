using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel;
namespace QL_ChiTieu.Models
{
    public class User
    {
        [Key]
        public int idUser { get; set; }

        [Column(TypeName = "nvarchar(50)")]
        [Required(ErrorMessage = "First Name is required.")]
        public string FirstName { get; set; }

        [Column(TypeName = "nvarchar(50)")]
        [Required(ErrorMessage = "Last Name is required.")]
        public string LastName { get; set; }

        [Column(TypeName = "varchar(50)")]
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string Email { get; set; }

        [Column(TypeName = "varchar(50)")]
        [Required(ErrorMessage = "Password is required.")]
        public string Password { get; set; }

        [Column(TypeName = "int")]
        [DefaultValue(0)]
        public int OTPMail { get; set; }

        [NotMapped]
        [Required(ErrorMessage = "Confirm Password is required.")]
        [Compare("Password", ErrorMessage = "Confirm Password and Password do not match.")]
        public string ConfirmPassword { get; set; }

        [NotMapped]
        [Required(ErrorMessage = "Old Password is required.")]
        public string OldPassword { get; set; }

        [NotMapped]
        [Required(ErrorMessage = "New Password is required.")]
        public string NewPassword { get; set; }

        [NotMapped]
        [Compare("NewPassword")]
        public string ConfirmNewPassword { get; set; }

        [Column(TypeName = "nvarchar(255)")]
        public string ProfilePhotoPath { get; set; }

        public string FullName()
        {
            return this.FirstName + " " + this.LastName;
        }
        [NotMapped]
        public int OTP { get; set; }
    }
}