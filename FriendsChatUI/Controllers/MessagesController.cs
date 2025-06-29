using FriendsChatUI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;

public class MessagesController : Controller
{
    private readonly IHttpClientFactory _httpFactory;
    private readonly IConfiguration _config;

    public MessagesController(IHttpClientFactory httpFactory, IConfiguration config)
    {
        _httpFactory = httpFactory;
        _config = config;
    }

    // Step 1: Show list of friends with Message button
    public async Task<IActionResult> Index()
    {
        var token = HttpContext.Session.GetString("JwtToken");
        var userId = HttpContext.Session.GetString("UserId");
        var client = _httpFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var res = await client.GetAsync($"{_config["ApiCoreBaseUrl"]}/api/friends/list?UserId={userId}");
        if (!res.IsSuccessStatusCode)
        {
            TempData["Error"] = "Failed to load friends.";
            return View(new List<UserDto>());
        }

        var json = await res.Content.ReadAsStringAsync();
        var friends = JsonConvert.DeserializeObject<List<UserDto>>(json);
        return View(friends);
    }

    // Step 2: Chat with a selected friend
    [HttpGet]
    public async Task<IActionResult> Chat(Guid friendId)
    {
        ViewBag.FriendId = friendId;

        var token = HttpContext.Session.GetString("JwtToken");
        var client = _httpFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // 🔹 Step 1: Get friend's name (or full user details)
        var friendResponse = await client.GetAsync($"{_config["ApiCoreBaseUrl"]}/api/user/Get?id={friendId}");
        if (friendResponse.IsSuccessStatusCode)
        {
            var friendJson = await friendResponse.Content.ReadAsStringAsync();
            var friend = JsonConvert.DeserializeObject<UserDto>(friendJson);
            ViewBag.FriendName = friend!.FullName;  // ✅ Important
            ViewBag.FriendImage = friend.ProfileImageUrl;  // ✅ Important
            HttpContext.Session.SetString("Name", friend!.FullName);
        }
        else
        {
            ViewBag.FriendName = "Friend";
        }

        // 🔹 Step 2: Load chat messages
        var res = await client.GetAsync($"{_config["ApiCoreBaseUrl"]}/api/messages/GetMessages?otherUserId={friendId}");
        if (!res.IsSuccessStatusCode)
        {
            TempData["Error"] = "Failed to load messages.";
            return View("Chat", new List<ChatMessage>());
        }

        var json = await res.Content.ReadAsStringAsync();
        var messages = JsonConvert.DeserializeObject<List<ChatMessage>>(json);

        return View("Chat", messages);
    }


    [HttpPost]
    public async Task<IActionResult> UploadMedia(IFormFile file, Guid friendId)
    {
        var token = HttpContext.Session.GetString("JwtToken");
        var client = _httpFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var content = new MultipartFormDataContent();
        content.Add(new StreamContent(file.OpenReadStream()), "file", file.FileName);
        content.Add(new StringContent(friendId.ToString()), "receiverId");

        var res = await client.PostAsync($"{_config["ApiCoreBaseUrl"]}/api/messages/send-media", content);

        return RedirectToAction("Chat", new { friendId });
    }

    [HttpGet]
    public async Task<IActionResult> GetChatHistory(Guid friendId)
    {
        string FriendId = friendId.ToString();
        var token = HttpContext.Session.GetString("JwtToken");
        var userId = HttpContext.Session.GetString("UserId");
        var client = _httpFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await client.GetAsync($"{_config["ApiCoreBaseUrl"]}/api/chat/messages?user1={userId}&user2={friendId}");
        if (!response.IsSuccessStatusCode) return BadRequest("Failed to load");

        var data = await response.Content.ReadFromJsonAsync<List<ChatMessageDto>>();

        var messages = data.Select(d => new ChatMessage
        {
            Id = d.messageId,
            SenderId = d.fromUser, // <-- This is critical
            fromUser = d.fromUser,
            ReceiverId = d.toUser,
            Message = d.message,
            Timestamp = d.timestamp,
            MediaUrl = d.mediaUrl,
            MediaType = d.mediaType,
            Status = Enum.TryParse<MessageStatus>(d.status, out var s) ? s : MessageStatus.Sent
        }).ToList();

        return Json(messages);

    }

}
