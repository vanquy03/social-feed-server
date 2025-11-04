using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SocialFeed.Api.Data;
using SocialFeed.Api.Dtos;
using SocialFeed.Api.Models;

namespace SocialFeed.Api.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UserController : Controller
    {
        private readonly AppDbContext _dbContext;

        public UserController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }   

        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> RegisterAsync(RegisterUserDto dto)
        {
            if (await _dbContext.Users.AnyAsync(u => u.Username == dto.Username))
            {
                return BadRequest("UserName already exits.");
            }

            var user = new User
            {
                Username = dto.Username,
                PasswordHash = dto.Password
            };

            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            return Ok(user);
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> LoginAsync(LoginUserDto userDto)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Username == userDto.Username && u.PasswordHash == userDto.Password);

            if (user == null)
                return Unauthorized("Invalid credentials");

            return Ok(new { message = "Login successful", user.Id });
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserResponseDto>> GetUser(int id)
        {
            var user = await _dbContext.Users.FindAsync(id);
            if (user == null) return NotFound();

            return new UserResponseDto
            {
                Id = user.Id,
                Username = user.Username,
                CreatedAt = user.CreatedAt
            };
        }
    }
}
