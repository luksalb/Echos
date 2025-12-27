namespace Echos.Api.Controllers.Users
{
    public record LoginRequest
    (
        string Login,
        string Password
    );
}
