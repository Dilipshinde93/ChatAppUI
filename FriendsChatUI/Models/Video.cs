namespace FriendsChatUI.Models
{
    public class Video
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string VideoUrl { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; }
    }
}
