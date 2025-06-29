using FriendsChatUI.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http.Headers;

public class VideoController : Controller
{
    private readonly IHttpClientFactory _http;

    public VideoController(IHttpClientFactory http)
    {
        _http = http;
    }

    public async Task<IActionResult> Index()
    {
        var token = HttpContext.Session.GetString("JwtToken");
        var client = _http.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var res = await client.GetAsync("https://localhost:5002/api/videos");
        var data = JsonConvert.DeserializeObject<List<Video>>(await res.Content.ReadAsStringAsync());

        return View(data);
    }

    [HttpPost]
    public async Task<IActionResult> Upload(string title, IFormFile file)
    {
        var token = HttpContext.Session.GetString("JwtToken");
        var client = _http.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var form = new MultipartFormDataContent();
        form.Add(new StringContent(title), "title");
        if (file != null)
            form.Add(new StreamContent(file.OpenReadStream()), "file", file.FileName);

        await client.PostAsync("https://localhost:5002/api/videos/upload", form);
        return RedirectToAction("Index");
    }
}
