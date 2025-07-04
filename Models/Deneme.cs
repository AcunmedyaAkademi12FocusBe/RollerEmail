using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Roller.Models;

public class Deneme
{
    [Required, MaxLength(50)]
    public string Name { get; set; }
    [Required, EmailAddress, MaxLength(100)]
    public string Email { get; set; }
    [Required, JsonPropertyName("g-recaptcha-response")]
    public string? RecaptchaResponse { get; set; }
}

