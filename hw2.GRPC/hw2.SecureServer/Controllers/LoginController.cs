using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using hw2.SecureServer.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace hw2.SecureServer.Controllers;

[ApiController]
[Route("/")]
public class LoginController(IConfiguration config) : ControllerBase
{
    [HttpGet]
    public IActionResult Get() 
        => Ok("This site is operated by Genetic Lifeform and Disk Operating System. For the Science!");
    
    [HttpPost]
    public IActionResult Login([FromBody] LoginRequest data)
    {
        if (data is not {Name: "GLaDOS", Password: "Science!"})
            return Unauthorized("Neurotoxin will be released in 3 minutes... Just kidding)");
                
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["JWT:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new Claim[] {new(ClaimTypes.NameIdentifier, data.Name)};
        var token = new JwtSecurityToken(
            config["JWT:Issuer"],
            config["JWT:Audience"],
            claims,
            expires: DateTime.Now.AddMinutes(15),
            signingCredentials: creds);
        return Ok(new JwtSecurityTokenHandler().WriteToken(token));

    }
}