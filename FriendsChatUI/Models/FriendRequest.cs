namespace FriendsChatUI.Models
{
    public class FriendRequest
    {
        public Guid Id { get; set; }
        public Guid FromUserId { get; set; }
        public Guid ToUserId { get; set; }
        public string Status { get; set; } = "Pending"; // Pending, Accepted, Rejected
        public DateTime RequestedAt { get; set; }
    }
}
