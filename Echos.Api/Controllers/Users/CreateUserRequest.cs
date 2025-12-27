namespace Echos.Api.Controllers.Users
{
    public record CreateUserRequest
    (
        string UserName,
        string Name,
        string Email,
        string Password
    );
}
