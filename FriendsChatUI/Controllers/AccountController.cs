using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using FriendsChatClientApp.Models;
using System.Net.Http.Headers;

namespace FriendsChatClientApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _config;

        public AccountController(IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            _httpClientFactory = httpClientFactory;
            _config = config;
        }

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Build wwwroot/uploads path using current directory
            var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            if (!Directory.Exists(uploadsDir))
                Directory.CreateDirectory(uploadsDir);

            string? imagePath = null;

            // Save profile image locally
            if (model.ProfileImage != null)
            {
                var fileName = $"{Guid.NewGuid()}_{model.ProfileImage.FileName}";
                var filePath = Path.Combine(uploadsDir, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.ProfileImage.CopyToAsync(stream);
                }

                imagePath = $"/uploads/{fileName}"; // Relative path to be stored
            }

            // Prepare API call
            var client = _httpClientFactory.CreateClient();
            var form = new MultipartFormDataContent
            {
                { new StringContent(model.Email), "Email" },
                { new StringContent(model.Password), "Password" }
            };

            if (!string.IsNullOrEmpty(model.FullName))
                form.Add(new StringContent(model.FullName), "FullName");

            if (!string.IsNullOrEmpty(imagePath))
                form.Add(new StringContent(imagePath), "ProfileImagePath");

            var response = await client.PostAsync(_config["ApiCoreBaseUrl"] + "/api/auth/register", form);

            if (response.IsSuccessStatusCode)
                return RedirectToAction("Login");

            ModelState.AddModelError("", "Registration failed");
            return View(model);
        }




        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var client = _httpClientFactory.CreateClient();
            var json = JsonSerializer.Serialize(model);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync(_config["MainBaseUrl"] + "/api/auth/login", content);

            if (response.IsSuccessStatusCode)
            {
                var tokenJson = await response.Content.ReadAsStringAsync();
                var tokenObj = JsonSerializer.Deserialize<TokenResponse>(tokenJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                HttpContext.Session.SetString("JwtToken", tokenObj.Token);
                HttpContext.Session.SetString("username", tokenObj.User.FullName);
                HttpContext.Session.SetString("UserId", tokenObj.User.Id.ToString());
                HttpContext.Session.SetString("ProfileImageUrl",  tokenObj.User.ProfileImageUrl);
                return RedirectToAction("Index", "Dashboard");
            }

            ModelState.AddModelError("", "Invalid UserName or Password");
            //return RedirectToAction("Index", "Dashboard");
            return View();
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }

    public class TokenResponse
    {
        public string Token { get; set; } = string.Empty;
        public TokenUser User { get; set; } = new();
    }

    public class TokenUser
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? ProfileImageUrl { get; set; }
    }
}