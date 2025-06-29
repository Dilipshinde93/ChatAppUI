using System.ComponentModel.DataAnnotations;

public class RegisterViewModel
{
    [Required]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;

    public string? FullName { get; set; }

    public IFormFile? ProfileImage { get; set; }
}
