namespace FriendsChatUI.Models
{
    public class PostDto
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public string ImageUrl { get; set; }
        public string AuthorName { get; set; }
        public Guid AuthorId { get; set; }
        public DateTime Timestamp { get; set; }
        public List<LikeDto> Likes { get; set; }
        public List<CommentDto> Comments { get; set; }
    }

    public class LikeDto
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; }
    }

    public class CommentDto
    {
        public string Text { get; set; }
        public DateTime Timestamp { get; set; }
        public string UserName { get; set; }
    }
}
