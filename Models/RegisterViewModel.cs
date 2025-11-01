using System.ComponentModel.DataAnnotations;

namespace AuthServer.Models
{
    public class RegisterViewModel
    {
        [Required]
        [StringLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [StringLength(16, MinimumLength = 8)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{8,16}$",
            ErrorMessage = "Password must have 1 uppercase, 1 lowercase, 1 digit, 1 special symbol and be 8–16 characters long.")]
        public string Password { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required]
        [Phone]
        [RegularExpression(@"^\+380\d{9}$", ErrorMessage = "Phone must be in format +380XXXXXXXXX")]
        public string Phone { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string SelectedRole { get; set; }
        public string ReturnUrl { get; set; }
    }

}
