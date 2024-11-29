using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using RestaurangWebAPI.Models;

namespace RestaurangWebAPI.Controllers
{
    public class BookingController : Controller
    {
        Uri baseAddress = new Uri("https://localhost:7005/api");
        private readonly HttpClient _httpClient;

        public BookingController()
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
            List<BookingViewModel> bookingList = new List<BookingViewModel>();
            HttpResponseMessage response = await _httpClient.GetAsync(_httpClient.BaseAddress + "/Booking/GetAllBookings");

            if (response.IsSuccessStatusCode)
            {
                string data = await response.Content.ReadAsStringAsync();
                bookingList = JsonConvert.DeserializeObject<List<BookingViewModel>>(data);

                return View(bookingList);
            }
            else
            {
                // Hantera fel
                ModelState.AddModelError(string.Empty, "An error occurred while fetching bookings.");
                return View(new List<BookingViewModel>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
 
            var response = await _httpClient.GetAsync(_httpClient.BaseAddress + "/Table/tables/GetAllTables");
            if (response.IsSuccessStatusCode)
            {
                var availableTables = await response.Content.ReadFromJsonAsync<List<TableViewModel>>();

                ViewBag.AvailableTables = availableTables.Select(t => new SelectListItem
                {
                    Value = t.Id.ToString(),
                    Text = $"Table {t.Id} - Seats: {t.Seats}"
                }).ToList();
            }
            else
            {
                ViewBag.AvailableTables = new List<SelectListItem>();
            }

            return View(new BookingViewModelCustomerName());
        }

        [HttpPost]
        public async Task<IActionResult> Create(BookingViewModelCustomerName model)
        {
            Console.WriteLine($"CustomerName: {model.BookingDate}");

            var tableResponse = await _httpClient.GetAsync(_httpClient.BaseAddress + $"/Table/{model.TableId}");

            if (!tableResponse.IsSuccessStatusCode)
            {
                return NotFound();
            }
            var table = await tableResponse.Content.ReadFromJsonAsync<TableViewModel>();

            List<CustomerViewModel> customerList = new List<CustomerViewModel>();
            var customerResponseList = await _httpClient.GetAsync(_httpClient.BaseAddress + $"/Customer/GetAllCustomers");

            if(!customerResponseList.IsSuccessStatusCode)
            {
                return NotFound();
            }
            string data = await customerResponseList.Content.ReadAsStringAsync();
            customerList = JsonConvert.DeserializeObject<List<CustomerViewModel>>(data);
            var FoundCustomer = customerList.Where(x => x.CustomerName == model.CustomerName && x.Phone == model.PhoneNumber).FirstOrDefault();

            if(FoundCustomer == null)
            {
                return RedirectToAction("Create", "Customer");
            }

            var Booking = new BookingViewModel()
            {
                BookingDate = model.BookingDate,
                Customers = FoundCustomer,
                Tables = table,
                PeopleAmount = model.PeopleAmount,
            };


            if (ModelState.IsValid)
            {

                var response = await _httpClient.PostAsJsonAsync(_httpClient.BaseAddress + "/Booking/AddBooking", Booking);



                if (response.IsSuccessStatusCode && HttpContext.Session.GetString("IsAdmin") == "true")
                {
                    TempData["Message"] = "Customer added successfully!";
                    return RedirectToAction("Index");
                }
                else
                {
                    return RedirectToAction("BookingConfirmed");
                }
            }

            TempData["Error"] = "Failed to add customer.";
            return View(model);
        }

        public async Task<IActionResult> BookingConfirmed()
        {
            return View();
        }

        public async Task<IActionResult> Edit(int id)
        {
            if (HttpContext.Session.GetString("IsAdmin") != "true")
            {
                return RedirectToAction("Index", "Home");
            }
            var response = await _httpClient.GetAsync(_httpClient.BaseAddress + $"/Booking/{id}");
            if (response.IsSuccessStatusCode)
            {
                var bookingItem = await response.Content.ReadFromJsonAsync<BookingViewModel>();

                BookingViewModelNonForeignKeys BookingDto = new BookingViewModelNonForeignKeys();

                BookingDto.Customer = bookingItem.Customers.Id;
                BookingDto.TableId = bookingItem.Tables.Id;   
                BookingDto.BookingDate = bookingItem.BookingDate;
                BookingDto.PeopleAmount = bookingItem.PeopleAmount;

                return View(BookingDto);
            }
            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> Edit(BookingViewModelNonForeignKeys model)
        {
            Console.WriteLine(model.BookingDate);
            Console.WriteLine(model.PeopleAmount);
            Console.WriteLine(model.Customer);
            Console.WriteLine(model.TableId);

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var response = await _httpClient.PatchAsJsonAsync(_httpClient.BaseAddress + "/Booking/UpdateBooking", model);
            Console.WriteLine(response);
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
            var response = await _httpClient.GetAsync(_httpClient.BaseAddress + $"/Booking/{id}");

            if (!response.IsSuccessStatusCode)
            {
                return NotFound();
            }
            var booking = await response.Content.ReadFromJsonAsync<BookingViewModel>();

            return View(booking);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var response = await _httpClient.DeleteAsync(_httpClient.BaseAddress + $"/Booking/{id}");
            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Index");
            }

            return View();
        }
    }
}
