namespace ApiClinica.Models;

public class Usuario
{
    public int id { get; set; }
    public string User { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}