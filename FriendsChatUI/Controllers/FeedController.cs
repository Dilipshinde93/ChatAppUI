using FriendsChatUI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

namespace FriendsChatUI.Controllers
{
    public class FeedController : Controller
    {
        private readonly IHttpClientFactory _httpFactory;
        private readonly IConfiguration _config;

        public FeedController(IHttpClientFactory httpFactory, IConfiguration config)
        {
            _httpFactory = httpFactory;
            _config = config;
        }

        public async Task<IActionResult> LoadFeedPartial()
        {
            var token = HttpContext.Session.GetString("JwtToken");
            var client = _httpFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await client.GetAsync($"{_config["ApiCoreBaseUrl"]}/api/Posts/GetPosts");
            var content = await response.Content.ReadAsStringAsync();

            var posts = JsonConvert.DeserializeObject<List<Post>>(content);
            return PartialView("_FeedPartial", posts);
        }

        [HttpPost]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> Create([FromForm] string content, [FromForm] IFormFile? image)
        {
            var token = HttpContext.Session.GetString("JwtToken");
            var userId = HttpContext.Session.GetString("UserId");
            var client = _httpFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            string? imageUrl = null;

            if (image != null && image.Length > 0)
            {
                var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                Directory.CreateDirectory(uploadsPath);

                var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
                var filePath = Path.Combine(uploadsPath, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(stream);
                }

                // URL path to be sent to API (make sure this matches static file serving)
                imageUrl = "/uploads/" + uniqueFileName;
            }

            // Send the content and image path to API
            var postPayload = new
            {
                UserId = userId,
                Content = content,
                ImageUrl = imageUrl
            };

            var jsonPayload = new StringContent(
                JsonConvert.SerializeObject(postPayload),
                Encoding.UTF8,
                "application/json"
            );

            var response = await client.PostAsync($"{_config["ApiCoreBaseUrl"]}/api/posts/create", jsonPayload);

            return RedirectToAction("Index", "Dashboard");
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Like(Guid postId)
        {
            var token = HttpContext.Session.GetString("JwtToken");
            var client = _httpFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            await client.PostAsync($"{_config["ApiCoreBaseUrl"]}/api/posts/{postId}/like", null);
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> Comment(Guid postId, [FromForm] string text)
        {
            var token = HttpContext.Session.GetString("JwtToken");
            var client = _httpFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var form = new MultipartFormDataContent();
            form.Add(new StringContent(text), "text");

            await client.PostAsync($"{_config["ApiCoreBaseUrl"]}/api/posts/{postId}/comments", form);
            return Ok();
        }
    }

}
