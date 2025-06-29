namespace FriendsChatUI.Models
{
    public class FriendRequestDto
    {
        public Guid Id { get; set; }
        public Guid FromUserId { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? ProfileImageUrl { get; set; }
        public bool IsFriend { get; set; }
        public bool RequestSent { get; set; }
    }
}
