using Microsoft.AspNetCore.Identity;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public string? Bio { get; set; } = string.Empty; 
    public string? MediaUrl { get; set; }
    public string? SocialLinks { get; set; }
}
