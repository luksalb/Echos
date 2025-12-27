using Echos.Api.Infra.Data;
using Echos.Api.Domain.Echos;
using Echos.Api.Controllers.Echos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Echos.Api.Controllers.Echos
{
    [ApiController]
    [Route("api/[controller]")]
    public class EchosController : ControllerBase
    {
        private readonly AppDbContext _db;

        public EchosController(AppDbContext db)
        {
            _db = db;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateEchoRequest request)
        {
            // Pega o ID do usuário logado do token JWT
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized(new { message = "Invalid token" });
            }

            var userId = long.Parse(userIdClaim!);
            
            // Valida
            CreateEchoValidator.Validate(request.Content);

            // Cria o echo
            var echo = new Echo(userId, request.Content);

            _db.Echos.Add(echo);
            await _db.SaveChangesAsync();

            return Ok(new
            {
                id = echo.Id,
                content = echo.Content,
                createdAt = echo.CreationTime,
                userId = echo.UserId
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(long id)
        {
            var echo = await _db.Echos
                .Include(e => e.User)
                .Where(e => !e.IsDeleted)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (echo == null)
            {
                return NotFound();
            }

            return Ok(new
            {
                id = echo.Id,
                content = echo.Content,
                createdAt = echo.CreationTime,
                user = new
                {
                    id = echo.User.Id,
                    userName = echo.User.UserName,
                    name = echo.User.Name,
                }
            });
        }
    }
}
