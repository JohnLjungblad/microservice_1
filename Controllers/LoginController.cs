using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

[ApiController]
[Route("/api")]
public class AuthController : ControllerBase
{
    private readonly MessageService messageService;
    private readonly SignInManager<IdentityUser> signInManager;
    private readonly UserManager<IdentityUser> userManager;

    public AuthController(SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager, MessageService messageService)
    {
        this.signInManager = signInManager;
        this.userManager = userManager;
        this.messageService = messageService;
    }
    [HttpPost]
    [Route("/login")]
    public async Task<IActionResult> Login(LoginDto loginDto)
    {
        //Find the user by username
        var user = await userManager.FindByNameAsync(loginDto.Username);
        if (user == null)
        {
            return BadRequest("Username or password is incorrect.");
        }

        //Validate password and sign in the user
        var result = await signInManager.PasswordSignInAsync(user.UserName, loginDto.Password, isPersistent: false, lockoutOnFailure: false);

        if (result.Succeeded)
        {
            var token = GenerateJwtToken(user.UserName);
            messageService.ExportToken(token);
            messageService.SendLoggingActions("User: " + user.UserName + " logged in");
            return Ok("User signed in " + new { token });
        }
        else if (result.IsLockedOut)
        {
            return BadRequest("User is locked out.");
        }
        else
        {
            return BadRequest("Username or password is incorrect.");
        }
    }

    private string GenerateJwtToken(string username)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes("0123456789012345678901234567890123456789123");

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, username)
            }),
            Expires = DateTime.UtcNow.AddHours(1), //1 hour expiration date
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}

public class LoginDto
{
    public required string Username { get; set; }
    public required string Password { get; set; }
}
