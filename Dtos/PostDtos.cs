namespace SocialFeed.Api.Dtos
{
    public class CreatePostDto
    {
        public string Content { get; set; } = string.Empty;
        public int UserId { get; set; }
    }

    public class PostResponseDto
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string Username { get; set; } = string.Empty;
        public int LikeCount { get; set; }
        public int CommentCount { get; set; }
    }
}
