using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SmartAIChatbot.Api.Models;
using SmartAIChatbot.Api.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SmartAIChatbot.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _config;
    private readonly UserService _userService;

    public AuthController(IConfiguration config, UserService userService)
    {
        _config = config;
        _userService = userService;
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] AuthRequest request)
    {
        var (isValid, role) = _userService.ValidateUser(request.Email, request.Password);
        if (!isValid)
            return Unauthorized("Invalid credentials");

        var token = GenerateJwtToken(request.Email, role);

        return Ok(new AuthResponse
        {
            Token = token,
            Role = role
        });
    }

    private string GenerateJwtToken(string email, string role)
    {
        var jwtKey = _config["Jwt:Key"];
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Role, role)
        };

        var token = new JwtSecurityToken(
            issuer: null,
            audience: null,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(2),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
