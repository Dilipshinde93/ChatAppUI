using FriendsChatUI.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http.Headers;

namespace FriendsChatUI.Controllers
{
    public class FriendsController : Controller
    {
        private readonly IHttpClientFactory _http;
        private readonly IConfiguration _config;

        public FriendsController(IHttpClientFactory http, IConfiguration config)
        {
            _http = http;
            _config = config;
        }

        public async Task<IActionResult> LoadFriendsWithSuggestionsPartial()
        {
            var token = HttpContext.Session.GetString("JwtToken");
            var userId = HttpContext.Session.GetString("UserId");

            var client = _http.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // 1. Get accepted friends
            var friendsResponse = await client.GetAsync($"{_config["ApiCoreBaseUrl"]}/api/friends/list?UserId={userId}");
            var friendsJson = await friendsResponse.Content.ReadAsStringAsync();
            var friends = JsonConvert.DeserializeObject<List<UserDto>>(friendsJson) ?? new();

            // 2. Get friend suggestions
            var suggestionsResponse = await client.GetAsync($"{_config["ApiCoreBaseUrl"]}/api/friends/suggestions?UserId={userId}");
            var suggestionsJson = await suggestionsResponse.Content.ReadAsStringAsync();
            var suggestions = JsonConvert.DeserializeObject<List<UserDto>>(suggestionsJson) ?? new();

            // 3. Get pending requests (to show badges or actions if needed)
            var pendingResponse = await client.GetAsync($"{_config["ApiCoreBaseUrl"]}/api/friends/pending?UserId={userId}");
            var pendingJson = await pendingResponse.Content.ReadAsStringAsync();
            var pendingRequests = JsonConvert.DeserializeObject<List<FriendRequestDto>>(pendingJson) ?? new();
            ViewBag.PendingRequests = pendingRequests;

            var vm = new FriendsAndSuggestionsViewModel
            {
                Friends = friends,
                Suggestions = suggestions
            };

            return PartialView("_FriendsAndSuggestions", vm);
        }

        [HttpPost]
        public async Task<IActionResult> SendRequest(Guid toUserId)
        {
            var token = HttpContext.Session.GetString("JwtToken");
            var userId = HttpContext.Session.GetString("UserId");
            var client = _http.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var formData = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("toUserId", toUserId.ToString()),
                new KeyValuePair<string, string>("fromUserId", userId.ToString())
            });

            var response = await client.PostAsync($"{_config["ApiCoreBaseUrl"]}/api/friends/request", formData);

            TempData[response.IsSuccessStatusCode ? "Success" : "Error"] =
                response.IsSuccessStatusCode ? "Friend request sent!" : "Failed to send friend request.";

            return RedirectToAction("LoadFriendsWithSuggestionsPartial", "Friends");
        }

        //public async Task<IActionResult> Requests()
        //{
        //    var token = HttpContext.Session.GetString("JwtToken");
        //    var client = _http.CreateClient();
        //    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        //    var response = await client.GetAsync($"{_config["ApiCoreBaseUrl"]}/api/friends/requests");
        //    var data = JsonConvert.DeserializeObject<List<FriendRequest>>(await response.Content.ReadAsStringAsync());

        //    return View(data);
        //}

        [HttpPost]
        public async Task<IActionResult> AcceptRequest(Guid requestId)
        {
            var token = HttpContext.Session.GetString("JwtToken");
            var client = _http.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            await client.PostAsync($"{_config["ApiCoreBaseUrl"]}/api/friends/accept?requestId={requestId}", null);
            return RedirectToAction("Index", "Dashboard");
        }

        [HttpPost]
        public async Task<IActionResult> RejectRequest(Guid requestId)
        {
            var token = HttpContext.Session.GetString("JwtToken");
            var client = _http.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            await client.PostAsync($"{_config["ApiCoreBaseUrl"]}/api/friends/reject?requestId={requestId}", null);
            return Ok();
        }
    }
}
