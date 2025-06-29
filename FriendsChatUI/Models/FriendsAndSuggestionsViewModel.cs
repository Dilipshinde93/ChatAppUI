using FriendsChatUI.Models;

public class FriendsAndSuggestionsViewModel
{
    public List<UserDto> Friends { get; set; } = new();
    public List<UserDto> Suggestions { get; set; } = new();
}
