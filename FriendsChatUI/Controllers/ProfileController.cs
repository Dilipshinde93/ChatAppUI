using FriendsChatUI.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http.Headers;

public class ProfileController : Controller
{
    private readonly IHttpClientFactory _http;
    private readonly IConfiguration _config;

    public ProfileController(IHttpClientFactory http, IConfiguration config)
    {
        _http = http;
        _config = config;
    }

    public async Task<IActionResult> Index()
    {
        var token = HttpContext.Session.GetString("JwtToken");
        var userId = new Guid(HttpContext.Session.GetString("UserId"));
        var client = _http.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var res = await client.GetAsync($"{_config["ApiCoreBaseUrl"]}/api/profile/UserProfile?UserId={userId}");
        var data = JsonConvert.DeserializeObject<UserProfile>(await res.Content.ReadAsStringAsync());

        return View(data);
    }
}
