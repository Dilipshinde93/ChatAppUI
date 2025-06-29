using FriendsChatUI.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using static System.Net.WebRequestMethods;
using System.Net.Http.Headers;
using System.Text.Json;

namespace FriendsChatUI.Controllers
{
    public class DashboardController : Controller
    {
        private readonly IConfiguration _config;
        private readonly IHttpClientFactory _client;
        public DashboardController(IConfiguration config, IHttpClientFactory client)
        {
            _config = config;
            _client = client;
        }
        public async Task<IActionResult> Index()
        {
            var token = HttpContext.Session.GetString("JwtToken");
            var userId = HttpContext.Session.GetString("UserId");

            var client = _client.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // 1. Get pending friend requests
            var pendingRequests = new List<FriendRequestDto>();
            var pendingResponse = await client.GetAsync($"{_config["ApiCoreBaseUrl"]}/api/friends/pending?UserId={userId}");

            if (pendingResponse.IsSuccessStatusCode)
            {
                var json = await pendingResponse.Content.ReadAsStringAsync();
                pendingRequests = System.Text.Json.JsonSerializer.Deserialize<List<FriendRequestDto>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                })!;
            }

            ViewBag.PendingRequestCount = pendingRequests.Count;

            // 2. Get feed posts for default view rendering
            var postList = new List<PostDto>();
            var feedResponse = await client.GetAsync($"{_config["ApiCoreBaseUrl"]}/api/Posts/GetPosts?UserId={userId}");

            if (feedResponse.IsSuccessStatusCode)
            {
                var json = await feedResponse.Content.ReadAsStringAsync();
                postList = System.Text.Json.JsonSerializer.Deserialize<List<PostDto>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                })!;
            }

            return View(postList); // 👈 Send feed as model
        }

    }
}
