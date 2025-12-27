using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authorization;
using Echos.Api.Infra.Data;
using Echos.Api.Domain.Users;
using Echos.Api.Application.Users;





namespace Echos.Api.Controllers.Users;

[ApiController]
[Route("users")]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _configuration;
    private readonly LoginAttemptTracker _attemptTracker;

    public UsersController(AppDbContext db, IConfiguration configuration, LoginAttemptTracker attemptTracker)
    {
        _db = db;
        _configuration = configuration;
        _attemptTracker = attemptTracker;
    }

    [AllowAnonymous]
    [HttpPost]
    public async Task<IActionResult> Create(CreateUserRequest request, [FromServices] UserService service)
    {
        try
        {
            var user = await service.ExecuteAsync(
                request.UserName,
                request.Name,
                request.Email,
                request.Password
            );

            return CreatedAtAction(nameof(Create), new
            {
                user.Id,
                user.UserName,
                user.Name,
                user.Email
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var login = request.Login.Trim().ToLowerInvariant();

        if (_attemptTracker.IsLockedOut(login))
        {
            var (current, max, lockoutEnd) = _attemptTracker.GetAttemptInfo(login);
            var remainingTime = (lockoutEnd.Value - DateTime.UtcNow).TotalMinutes;

            return StatusCode(429, new
            {
                message = $"Too many failed login attempts. Try again in {Math.Ceiling(remainingTime)} minutes."
            });
        }

        var user = await _db.Users.FirstOrDefaultAsync(U => 
            !U.IsDeleted && 
            (U.UserName == login || U.Email == login)
        );
            

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            _attemptTracker.RecordFailedAttempt(login);
            var (current, max, _) = _attemptTracker.GetAttemptInfo(login);

            return Unauthorized(new
            {
                message = $"Invalid login or password. Attempt {current}/{max}."
            });
        }

        _attemptTracker.ResetAttempts(login);

        var claims = new[]
{
        new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
        new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName),
        new Claim(JwtRegisteredClaimNames.Email, user.Email)
};

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!)
        );

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(2),
            signingCredentials: creds
        );

        var jwt = new JwtSecurityTokenHandler().WriteToken(token);

        return Ok(new
        {
            accessToken = jwt
        });
    }

    [Authorize]
    [HttpGet("me")]
    public IActionResult Me()
    {
        return Ok(new
        {
            UserId = User.FindFirstValue(JwtRegisteredClaimNames.Sub),
            UserName = User.FindFirstValue(JwtRegisteredClaimNames.UniqueName),
            Email = User.FindFirstValue(JwtRegisteredClaimNames.Email)
        });
    }

}
