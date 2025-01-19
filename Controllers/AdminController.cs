using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NuGet.Common;
using RestaurangWebAPI.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Security.Claims;

namespace RestaurangWebAPI.Controllers
{
    public class AdminController : Controller
    {

        Uri baseAddress = new Uri("https://localhost:7005/api");
        private readonly HttpClient _httpClient;

        public AdminController()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = baseAddress;
        }

                public IActionResult Login()
                {
                    return View();
                }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(AdminLogin model)
        {
            if (ModelState.IsValid)
            {

                var response = await _httpClient.PostAsJsonAsync(_httpClient.BaseAddress + "/Admin/Login", model);

                if (response.IsSuccessStatusCode)
                {
                    var jwtToken = await response.Content.ReadAsStringAsync();
                    var token = JsonConvert.DeserializeObject<TokenResponse>(jwtToken);
                    Console.WriteLine(token.Token);

                    var handler = new JwtSecurityTokenHandler();
                    var JwtToken = handler.ReadJwtToken(token.Token);

                    var claims = JwtToken.Claims.ToList();

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal, new AuthenticationProperties
                    {
                        IsPersistent = true,
                        ExpiresUtc = JwtToken.ValidTo
                    });

                    HttpContext.Session.SetString("IsAdmin", "true");


                    HttpContext.Response.Cookies.Append("jwtToken", token.Token, new CookieOptions
                    {
                        HttpOnly = true, // Skyddar mot XSS-attacker
                        Secure = true,   // Kräver HTTPS
                        SameSite = SameSiteMode.Strict,
                        Expires = JwtToken.ValidTo
                    });

                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    Console.WriteLine(response);
                    ViewBag.Error = "Invalid username or password.";
                }
                
            }

            return View(model);
        }


        public IActionResult Dashboard()
        {
            if (HttpContext.Session.GetString("IsAdmin") != "true")
            {
                return RedirectToAction("Login");
            }

            return View();
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme); // "Cookies" är autentiseringsschemat om du använder cookies

            // Rensa JWT-token-cookien explicit
            HttpContext.Response.Cookies.Delete("jwtToken");

            HttpContext.Session.Remove("IsAdmin");
            return RedirectToAction("Index", "Home");
        }

    }
}
