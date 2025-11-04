using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SocialFeed.Api.Data;
using SocialFeed.Api.Dtos;
using SocialFeed.Api.Models;

namespace SocialFeed.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LikesController : ControllerBase
    {
        private readonly AppDbContext _context;
        public LikesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> LikeOrUnlike(LikeDto dto)
        {
            var existing = await _context.Likes
                .FirstOrDefaultAsync(l => l.UserId == dto.UserId && l.PostId == dto.PostId);

            if (existing != null)
            {
                _context.Likes.Remove(existing);
                await _context.SaveChangesAsync();
                return Ok("Unliked");
            }

            _context.Likes.Add(new Like { UserId = dto.UserId, PostId = dto.PostId });
            await _context.SaveChangesAsync();
            return Ok("Liked");
        }

        [HttpGet("{postId}")]
        public async Task<IActionResult> GetLikes(int postId)
        {
            var likes = await _context.Likes
                .Where(l => l.PostId == postId)
                .Include(l => l.User)
                .Select(l => l.User!.Username)
                .ToListAsync();

            return Ok(likes);
        }
    }
}
