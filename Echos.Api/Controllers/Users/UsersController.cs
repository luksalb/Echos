using Microsoft.AspNetCore.Mvc;
using Echos.Api.Infra.Data;
using Echos.Api.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Echos.Api.Controllers.Users;

[ApiController]
[Route("users")]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _db;

    public UsersController(AppDbContext db)
    {
        _db = db;
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateUserRequest request)
    {
        var userName = request.UserName.Trim().ToLowerInvariant();
        var email = request.Email.Trim().ToLowerInvariant();

        var exists = await _db.Users.AnyAsync(U => !U.IsDeleted && (U.UserName == userName || U.Email == email));

        if (exists)
        {
            return Conflict(new
            {
                message = "Username or Email already in use."
            });
        }

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        var user = new User(
            userName,
            request.Name.Trim(),
            email,
            passwordHash
        );

        

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(Create), new { id = user.Id }, new
        {
            user.Id,
            user.UserName,
            user.Name,
            user.Email
        });
    }
}
