using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SocialFeed.Api.Data;
using SocialFeed.Api.Dtos;
using SocialFeed.Api.Models;

namespace SocialFeed.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CommentsController : ControllerBase
    {
        private readonly AppDbContext _context;
        public CommentsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> Add(CreateCommentDto dto)
        {
            var comment = new Comment
            {
                Content = dto.Content,
                UserId = dto.UserId,
                PostId = dto.PostId
            };
            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();
            return Ok("Comment added");
        }

        [HttpGet("{postId}")]
        public async Task<IActionResult> GetByPost(int postId)
        {
            var comments = await _context.Comments
                .Where(c => c.PostId == postId)
                .Include(c => c.User)
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => new
                {
                    c.Id,
                    c.Content,
                    c.CreatedAt,
                    Username = c.User!.Username
                })
                .ToListAsync();

            return Ok(comments);
        }
    }
}
