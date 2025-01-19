using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RestaurangWebAPI.Models;
using System.Net.Http.Headers;

namespace RestaurangWebAPI.Controllers
{
    public class MenuController : Controller
    {
        Uri baseAddress = new Uri("https://localhost:7005/api");
        private readonly HttpClient _httpClient;

        public MenuController()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = baseAddress;
        }

        [HttpGet]
        public async  Task<IActionResult> Index()
        {
            var token = Request.Cookies["jwtToken"];
            Console.WriteLine(token);

            List<MenuViewModel> menuList = new List<MenuViewModel>();
            HttpResponseMessage response = await _httpClient.GetAsync(_httpClient.BaseAddress + "/Menu/GetMenu");

            if (response.IsSuccessStatusCode)
            {
                string data = await response.Content.ReadAsStringAsync();
                menuList = JsonConvert.DeserializeObject<List<MenuViewModel>>(data);
                return View(menuList);
            }
            else
            {
                // Hantera fel
                ModelState.AddModelError(string.Empty, "An error occurred while fetching menu.");
                return View(new List<MenuViewModel>());
            }
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(MenuViewModel model)
        {
            Console.WriteLine(model.FoodName + model.Price + model.IsAvaiable);

            var token = Request.Cookies["jwtToken"];
            Console.WriteLine(token);
            if (token == null)
            {
                return RedirectToPage("/Login");
            }

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            if (ModelState.IsValid)
            {

                var response = await _httpClient.PostAsJsonAsync(_httpClient.BaseAddress + "/Menu/AddMenu", model);

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"API Error: {response.StatusCode}");
                }

                if (response.IsSuccessStatusCode)
                {
                    TempData["Message"] = "Menu added successfully!";
                    return RedirectToAction("Index");
                }
            }

            TempData["Error"] = "Failed to add customer.";
            return View(model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var response = await _httpClient.GetAsync(_httpClient.BaseAddress + $"/Menu/{id}");
            if (response.IsSuccessStatusCode)
            {
                var menuItem = await response.Content.ReadFromJsonAsync<MenuViewModel>();
                return View(menuItem);
            }
            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> Edit(MenuViewModel model)
        {
            Console.WriteLine(model.FoodName);

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var token = Request.Cookies["jwtToken"];
            Console.WriteLine(token);
            if (token == null)
            {
                return RedirectToPage("/Login");
            }

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.PatchAsJsonAsync(_httpClient.BaseAddress + "/Menu/UpdateMenu", model);
            if (response.IsSuccessStatusCode)
            {
                TempData["Message"] = "Menu item updated successfully!";
                return RedirectToAction("Index");
            }

            TempData["Error"] = "Failed to update menu item.";
            return View(model);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var response = await _httpClient.GetAsync(_httpClient.BaseAddress + $"/Menu/{id}");

            if (!response.IsSuccessStatusCode)
            {
                return NotFound();
            }
            var menu = await response.Content.ReadFromJsonAsync<MenuViewModel>();

            return View(menu);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var token = Request.Cookies["jwtToken"];
            Console.WriteLine(token);
            if (token == null)
            {
                return RedirectToPage("/Login");
            }

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.DeleteAsync(_httpClient.BaseAddress + $"/Menu/{id}");
            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Index");
            }

            return View();
        }
    }
}
