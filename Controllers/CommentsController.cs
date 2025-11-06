using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SocialFeed.Api.Data;
using SocialFeed.Api.Dtos;
using SocialFeed.Api.Models;
using System.ComponentModel.Design;

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
        [Route("add")]
        public async Task<IActionResult> Add([FromBody] CreateCommentDto dto)
        {
            var comment = new Comment
            {
                Content = dto.Content,
                UserId = dto.UserId,
                PostId = dto.PostId
            };
            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();
            return Ok(comment);
        }

        [HttpGet]
        [Route("getcomments")]
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
                    c.User!.Username
                })
                .ToListAsync();

            return Ok(comments);
        }

        [HttpPost]
        [Route("delete")]
        public async Task<IActionResult> Delete(int commentId)
        {
            var comment = await _context.Comments.Where(c => c.Id == commentId).FirstOrDefaultAsync();
            if (comment != null)
            {
                _context.Comments.Remove(comment);
                await _context.SaveChangesAsync();
                return Ok("Da xoa");
            }

            return NotFound("Ko tim thay comment trong Db.");
        }

        [HttpPut]
        [Route("edit")]
        public async Task<IActionResult> Edit (int commentId, string newComment)
        {
            var comment = await _context.Comments.Where(c => c.Id == commentId).FirstOrDefaultAsync();
            if (comment != null)
            {
                comment.Content = newComment;
                _context.Comments.Update(comment);
                await _context.SaveChangesAsync();
                return Ok("Da cap nhat");
            }

            return NotFound("Ko tim thay comment trong Db.");
        }

    }
}
