using HelixPortal.Application.DTOs.Auth;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace HelixPortal.Web.Controllers;

public class AccountController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        IHttpClientFactory httpClientFactory,
        ILogger<AccountController> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginRequestDto model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var client = _httpClientFactory.CreateClient("ApiClient");
        var json = JsonSerializer.Serialize(model);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await client.PostAsync("/api/auth/login", content);

        if (response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            var authResponse = JsonSerializer.Deserialize<AuthResponseDto>(
                responseContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (authResponse != null)
            {
                // Store token in session
                HttpContext.Session.SetString("AuthToken", authResponse.Token);
                HttpContext.Session.SetString("UserEmail", authResponse.Email);
                HttpContext.Session.SetString("UserRole", authResponse.Role);
                HttpContext.Session.SetString("DisplayName", authResponse.DisplayName);
                if (authResponse.ClientOrganisationId.HasValue)
                {
                    HttpContext.Session.SetString("ClientOrganisationId", authResponse.ClientOrganisationId.Value.ToString());
                }

                return RedirectToAction("Index", "Home");
            }
        }

        ModelState.AddModelError("", "Invalid email or password");
        return View(model);
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Register(RegisterRequestDto model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var client = _httpClientFactory.CreateClient("ApiClient");
        var json = JsonSerializer.Serialize(model);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // For now, registration might require auth - you may need to handle this differently
        var response = await client.PostAsync("/api/auth/register", content);

        if (response.IsSuccessStatusCode)
        {
            TempData["SuccessMessage"] = "Registration successful. Please log in.";
            return RedirectToAction("Login");
        }

        var errorContent = await response.Content.ReadAsStringAsync();
        ModelState.AddModelError("", "Registration failed. Please try again.");
        return View(model);
    }

    [HttpPost]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Login");
    }
}

