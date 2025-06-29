using System;

namespace FriendsChatUI.Models
{
    public class ChatMessage
    {
        public Guid Id { get; set; }
        public Guid SenderId { get; set; } = Guid.Empty;
        public Guid ReceiverId { get; set; } = Guid.Empty;
        public string Message { get; set; } = string.Empty;
        public string? MediaUrl { get; set; }
        public string? MediaType { get; set; }
        public Guid fromUser { get; set; }
        public DateTime Timestamp { get; set; }

        public MessageStatus Status { get; set; } = MessageStatus.Sent;
    }

    public enum MessageStatus
    {
        Sent = 0,
        Delivered = 1,
        Read = 2
    }
}
