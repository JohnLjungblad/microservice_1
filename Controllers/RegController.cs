using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("/api")]
public class RegController : ControllerBase
{
    private readonly AppDbContext database;
    private readonly UserManager<IdentityUser> userManager;

    private readonly MessageService messageService;

    public RegController(AppDbContext database, UserManager<IdentityUser> userManager, MessageService messageService)
    {
        this.database = database;
        this.userManager = userManager;
        this.messageService = messageService;
    }


    [HttpPost]
    [Route("/register")]
    public async Task<IActionResult> Register(RegisterDto registerDto)
    {
        //Check for user with same email
        var existingUser = await userManager.FindByEmailAsync(registerDto.Email);
        if (existingUser != null)
        {
            return BadRequest("Email is already in use.");
        }

        var user = new IdentityUser
        {
            UserName = registerDto.Username,
            Email = registerDto.Email
        };

        var userInformation = new SendUserDto
        {
            Id = user.Id,
            Username = registerDto.Username,
            Email = registerDto.Email,
            FullName = registerDto.FullName,
            Address = registerDto.Address
        };

        //Use userManager to register and hash user password
        var result = await userManager.CreateAsync(user, registerDto.Password);

        if (result.Succeeded)
        {
            messageService.NotifyUserCreation(userInformation);
            messageService.SendLoggingActions("User: " + user.UserName + " registered");
            return Ok("User registered!");
        }
        //If fail
        return BadRequest(result.Errors);
    }
}

public class RegisterDto
{
    public required string Username { get; set; }
    public required string Password { get; set; }
    public required string Email { get; set; }

    public required string FullName { get; set; }
    public required string Address { get; set; }
}
public class SendUserDto
{
    public required string Id { get; set; }
    public required string Username { get; set; }
    public required string Email { get; set; }
    public required string FullName { get; set; }
    public required string Address { get; set; }
}