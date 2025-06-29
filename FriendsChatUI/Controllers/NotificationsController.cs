using FriendsChatUI.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http.Headers;

public class NotificationController : Controller
{
    private readonly IHttpClientFactory _http;
    private readonly IConfiguration _config;

    public NotificationController(IHttpClientFactory http, IConfiguration config)
    {
        _http = http;
        _config = config;
    }

    public async Task<IActionResult> Index()
    {
        var token = HttpContext.Session.GetString("JwtToken");
        var userId = HttpContext.Session.GetString("UserId");
        var client = _http.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var res = await client.GetAsync($"{_config["ApiCoreBaseUrl"]}/api/notifications/all");
        var data = JsonConvert.DeserializeObject<List<Notification>>(await res.Content.ReadAsStringAsync());
        return View(data);
    }

}
