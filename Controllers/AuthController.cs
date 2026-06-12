using ApiClinica.Data;
using ApiClinica.DTOs;
using ApiClinica.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ApiClinica.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthController(AppDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDTO dto)
    {
        if (_context.Usuarios.Any(u => u.User == dto.User))
            return BadRequest(new { mensagem = "Username já existente!" });

        var usuario = new Usuario
        {
            User = dto.User,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role = dto.Role
        };

        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();

        return Ok(new { mensagem = "Usuário criado com sucesso." });
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginDTO dto)
    {
        var usuario = _context.Usuarios.FirstOrDefault(u => u.User == dto.User);

        if (usuario == null || !BCrypt.Net.BCrypt.Verify(dto.Password, usuario.PasswordHash))
            return Unauthorized(new { mensagem = "Usuário ou senha inválidos." });

        var token = GerarToken(usuario);
        return Ok(new { token });
    }

    private string GerarToken(Usuario usuario)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, usuario.User),
            new Claim(ClaimTypes.Role, usuario.Role)
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddHours(8),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
