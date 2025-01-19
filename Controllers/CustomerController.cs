using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using RestaurangWebAPI.Models;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Serialization;

namespace RestaurangWebAPI.Controllers
{
    public class CustomerController : Controller
    {
        Uri baseAddress = new Uri("https://localhost:7005/api");
        private readonly HttpClient _httpClient;

        public CustomerController()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = baseAddress;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (HttpContext.Session.GetString("IsAdmin") != "true")
            {
                return RedirectToAction("Create", "Booking");
            }

            List<CustomerViewModel> customerList = new List<CustomerViewModel>();
            HttpResponseMessage response = await _httpClient.GetAsync(_httpClient.BaseAddress + "/Customer/GetAllCustomers");

            if(response.IsSuccessStatusCode)
            {
                string data = await response.Content.ReadAsStringAsync();
                customerList = JsonConvert.DeserializeObject<List<CustomerViewModel>>(data);
                return View(customerList);
            }
            else
            {
                // Hantera fel
                ModelState.AddModelError(string.Empty, "An error occurred while fetching customers.");
                return View(new List<CustomerViewModel>());
            }
        }


        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CustomerViewModelNoID model)
        {
            Console.WriteLine($"CustomerName: {model.CustomerName}, Phone: {model.Phone}");
            Console.WriteLine(ModelState.IsValid);

            if (ModelState.IsValid)
            {

                var response = await _httpClient.PostAsJsonAsync(_httpClient.BaseAddress + "/Customer/AddCustomer", model);


                if (response.IsSuccessStatusCode)
                {
                    TempData["Message"] = "Customer added successfully!";
                    return RedirectToAction("Index");
                }
            }

            TempData["Error"] = "Failed to add customer.";
            return View(model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            if (HttpContext.Session.GetString("IsAdmin") != "true")
            {
                return RedirectToAction("Index", "Home");
            }
            var response = await _httpClient.GetAsync(_httpClient.BaseAddress + $"/Customer/{id}");
            if (response.IsSuccessStatusCode)
            {
                var customerItem = await response.Content.ReadFromJsonAsync<CustomerViewModel>();
                return View(customerItem);
            }
            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> Edit(CustomerViewModel model)
        {
            Console.WriteLine(model.CustomerName);

            var token = Request.Cookies["jwtToken"];
            Console.WriteLine(token);
            if (token == null)
            {
                return RedirectToPage("/Login");
            }

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var response = await _httpClient.PatchAsJsonAsync(_httpClient.BaseAddress + "/Customer/UpdateCustomer", model);
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
            var response = await _httpClient.GetAsync(_httpClient.BaseAddress + $"/Customer/{id}");

            if (!response.IsSuccessStatusCode)
            {
                return NotFound();
            }
            var customer = await response.Content.ReadFromJsonAsync<CustomerViewModel>();

            return View(customer);
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

            var response = await _httpClient.DeleteAsync(_httpClient.BaseAddress + $"/Customer/{id}");
            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Index");
            }

            return View();
        }


    }
}
