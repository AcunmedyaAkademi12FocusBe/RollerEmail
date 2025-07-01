namespace Roller.Models.Emails;

public class WelcomeEmail
{
    public string Name { get; set; }
    public List<string> Features { get; set; } = [];
    public string Message { get; set; }
}