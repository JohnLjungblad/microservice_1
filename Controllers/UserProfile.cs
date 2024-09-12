using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("/api")]
public class UserProfile : ControllerBase
{
    private readonly AppDbContext database;

    public UserProfile(AppDbContext database)
    {
        this.database = database;
    }

    [HttpGet]
    [Route("/getuserinfo")]
    public ActionResult<List<UserInfoDto>> SendUserProfileInformation(string searchPhrase)
    {
        var users = database.Users
            .Where(u => u.UserName.Contains(searchPhrase))
            .Select(u => new UserInfoDto{
                Username = u.UserName,
                Email = u.Email
            })
            .ToList();

            if(users != null)
            {
                return Ok(users);   
            }

            return BadRequest("No users found.");
    }
}

public class UserInfoDto
{
    public required string Username { get; set; }
    public required string Email { get; set; }
}