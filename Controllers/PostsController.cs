using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SocialFeed.Api.Data;
using SocialFeed.Api.Dtos;
using SocialFeed.Api.Models;

namespace SocialFeed.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PostsController : ControllerBase
    {
        private readonly AppDbContext _context;
        public PostsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Route("get_all")]
        public async Task<ActionResult<IEnumerable<PostResponseDto>>> GetAll(int userId)
        {
            var posts = await _context.Posts
                .Include(p => p.User)
                .Include(p => p.Comments)
                .Include(p => p.Likes)
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new PostResponseDto
                {
                    Id = p.Id,
                    Content = p.Content,
                    CreatedAt = p.CreatedAt,
                    Username = p.User!.Username,
                    IsLiked = p.Likes!.Any(l => l.UserId == userId),
                    LikeCount = p.Likes!.Count,
                    CommentCount = p.Comments!.Count

                })
                .ToListAsync();

            return Ok(posts);
        }

        [HttpPost]
        [Route("Create")]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] CreatePostDto dto)
        {
            var user = await _context.Users.FindAsync(dto.UserId);
            if (user == null) return BadRequest("Invalid user");

            var post = new Post { Content = dto.Content, UserId = dto.UserId };
            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Post created", post.Id });
        }

        [HttpDelete("{id}")]
        [Route("Delete")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post == null) return NotFound();

            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();

            return Ok("Post deleted");
        }
    }
}
