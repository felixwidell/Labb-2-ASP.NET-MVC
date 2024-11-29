using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RestaurangWebAPI.Models;

namespace RestaurangWebAPI.Controllers
{
    public class TableController : Controller
    {
        Uri baseAddress = new Uri("https://localhost:7005/api");
        private readonly HttpClient _httpClient;

        public TableController()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = baseAddress;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (HttpContext.Session.GetString("IsAdmin") != "true")
            {
                return RedirectToAction("Index", "Home");
            }
            List<TableViewModel> tableList = new List<TableViewModel>();
            HttpResponseMessage response = await _httpClient.GetAsync(_httpClient.BaseAddress + "/Table/tables/GetAllTables");

            if (response.IsSuccessStatusCode)
            {
                string data = await response.Content.ReadAsStringAsync();
                tableList = JsonConvert.DeserializeObject<List<TableViewModel>>(data);
                return View(tableList);
            }
            else
            {
                // Hantera fel
                ModelState.AddModelError(string.Empty, "An error occurred while fetching tables.");
                return View(new List<TableViewModel>());
            }
        }

        public IActionResult Create()
        {
            if (HttpContext.Session.GetString("IsAdmin") != "true")
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Create(TableViewModel model)
        {
            Console.WriteLine(model.Seats);

            if (ModelState.IsValid)
            {

                var response = await _httpClient.PostAsJsonAsync(_httpClient.BaseAddress + "/Table/AddTable", model);

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"API Error: {response.StatusCode}");
                }

                if (response.IsSuccessStatusCode)
                {
                    TempData["Message"] = "Table added successfully!";
                    return RedirectToAction("Index");
                }
            }

            TempData["Error"] = "Failed to add table.";
            return View(model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            if (HttpContext.Session.GetString("IsAdmin") != "true")
            {
                return RedirectToAction("Index", "Home");
            }
            var response = await _httpClient.GetAsync(_httpClient.BaseAddress + $"/Table/{id}");
            if (response.IsSuccessStatusCode)
            {
                var tableItem = await response.Content.ReadFromJsonAsync<TableViewModel>();
                return View(tableItem);
            }
            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> Edit(TableViewModel model)
        {
            Console.WriteLine(model.Seats);

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var response = await _httpClient.PatchAsJsonAsync(_httpClient.BaseAddress + "/Table/UpdateTable", model);
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
            if (HttpContext.Session.GetString("IsAdmin") != "true")
            {
                return RedirectToAction("Index", "Home");
            }
            var response = await _httpClient.GetAsync(_httpClient.BaseAddress + $"/Table/{id}");

            if (!response.IsSuccessStatusCode)
            {
                return NotFound();
            }
            var table = await response.Content.ReadFromJsonAsync<TableViewModel>();

            return View(table);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var response = await _httpClient.DeleteAsync(_httpClient.BaseAddress + $"/Table/{id}");
            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Index");
            }

            return View();
        }
    }
}
