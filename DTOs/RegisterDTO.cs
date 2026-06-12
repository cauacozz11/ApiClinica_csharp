namespace ApiClinica.DTOs;

public class RegisterDTO
{
    public string User { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}