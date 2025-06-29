namespace FriendsChatUI.Models
{
    public class ChatMessageDto
    {
        public Guid messageId { get; set; }
        public Guid fromUser { get; set; }
        public Guid toUser { get; set; }
        public string message { get; set; }
        public DateTime timestamp { get; set; }
        public string status { get; set; }
        public string? mediaUrl { get; set; }
        public string? mediaType { get; set; }
    }

}
