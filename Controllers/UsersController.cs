using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SocialFeed.Api.Data;
using SocialFeed.Api.Dtos;
using SocialFeed.Api.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SocialFeed.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : Controller
    {
        private readonly AppDbContext _dbContext;
        private readonly IConfiguration _config;


        public UsersController(AppDbContext dbContext, IConfiguration config)
        {
            _dbContext = dbContext;
            _config = config;
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
                return Unauthorized(new { message = "Invalid username or password" });

            // sinh token
            var token = GenerateJwtToken(user);

            return Ok(new {
                message = "Login successful",
                user,
                token,
            });
        }

        [HttpGet("{id}")]
        [Authorize]
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
        private string GenerateJwtToken(User user)
        {
            var jwtSettings = _config.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Username),
                new Claim("userId", user.Id.ToString())
            };

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(double.Parse(jwtSettings["ExpireMinutes"])),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
