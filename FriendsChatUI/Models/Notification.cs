﻿namespace FriendsChatUI.Models
{
    public enum NotificationType
    {
        FriendRequest,
        Message,
        FriendSuggestion
    }

    public class Notification
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string FromUserId { get; set; }
        public string Message { get; set; }
        public NotificationType Type { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
