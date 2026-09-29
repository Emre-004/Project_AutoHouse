using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Project_AutoHouse.Data;
using Project_AutoHouse.Models;
using System.IO;
using System.Linq;

namespace Project_AutoHouse.Controllers
{
    public class HomeController : Controller
    {
        // Пазим сесията (кой е влязъл)
        public static bool IsLoggedIn = false;
        public static string CurrentUserName = "";
        public static string CurrentUserEmail = "";
        public static string AdminEmail = "admin@autohouse.com";

        // ТОВА Е ВРЪЗКАТА НИ С БАЗАТА ДАННИ!
        private readonly AppDbContext _db;

        public HomeController(AppDbContext db)
        {
            _db = db;
        }

        // 1. Показва началната страница с всички коли от SQL базата
        // Добавяме параметри, които ще идват от търсачката
        // 1. Показва началната страница с всички коли от SQL базата
        public IActionResult Index(string searchMake, string searchModel, int? minYear, decimal? maxPrice, string searchCity, string sortOrder)
        {
            List<int> favIds = new List<int>();
            if (IsLoggedIn)
            {
                favIds = _db.Favorites.Where(f => f.UserEmail == CurrentUserEmail).Select(f => f.CarAdId).ToList();
            }
            ViewBag.Favorites = favIds;

            // ТУК ПРОМЕНЛИВАТА СЕ КАЗВА carsQuery
            var carsQuery = _db.Cars.AsQueryable();

            // --- 1. ПРИЛАГАМЕ ВСИЧКИ ФИЛТРИ ПЪРВО ---

            if (!string.IsNullOrEmpty(searchMake))
            {
                carsQuery = carsQuery.Where(c => c.Make == searchMake);
            }

            if (!string.IsNullOrEmpty(searchCity))
            {
                carsQuery = carsQuery.Where(c => c.City == searchCity);
            }

            if (!string.IsNullOrEmpty(searchModel))
            {
                carsQuery = carsQuery.Where(c => c.Model.Contains(searchModel));
            }

            if (minYear.HasValue)
            {
                carsQuery = carsQuery.Where(c => c.Year >= minYear.Value);
            }

            if (maxPrice.HasValue)
            {
                carsQuery = carsQuery.Where(c => c.Price <= maxPrice.Value);
            }


            // --- 2. СЛЕД ТОВА СОРТИРАМЕ РЕЗУЛТАТА (Използваме carsQuery) ---

            ViewBag.CurrentSort = sortOrder;
            switch (sortOrder)
            {
                case "price_desc":
                    carsQuery = carsQuery.OrderByDescending(c => c.Price);
                    break;
                case "price_asc":
                    carsQuery = carsQuery.OrderBy(c => c.Price);
                    break;
                case "year_desc":
                    carsQuery = carsQuery.OrderByDescending(c => c.Year);
                    break;
                case "year_asc":
                    carsQuery = carsQuery.OrderBy(c => c.Year);
                    break;
                default: // "newest" или ако нищо не е избрано
                    // Сортираме по Id намаляващо (най-голямото Id е последно добавената кола)
                    carsQuery = carsQuery.OrderByDescending(c => c.Id);
                    break;
            }

            // --- 3. НАКРАЯ ВРЪЩАМЕ ИЗГЛЕДА ---
            return View(carsQuery.ToList());
        }

        // 2. Страницата с детайлите
        public IActionResult Details(int id)
        {
            var car = _db.Cars.FirstOrDefault(c => c.Id == id);
            if (car == null)
            {
                return NotFound();
            }

            // УВЕЛИЧАВАМЕ БРОЯЧА С 1
            car.Views += 1;
            _db.SaveChanges(); // ЗАПАЗВАМЕ В БАЗАТА

            return View(car);
        }
        // 3. Отваря формата за Добавяне
        [HttpGet]
        public IActionResult Create()
        {
            if (!IsLoggedIn) return RedirectToAction("Login");
            return View();
        }

        // 4. Записва новата кола със снимката в SQL базата
        [HttpPost]
        public IActionResult Create(CarAd newCar, List<IFormFile> CarImages, List<string> SelectedExtras)
        {
            newCar.OwnerEmail = CurrentUserEmail;
            List<string> savedImageUrls = new List<string>();
            if (CarImages != null && CarImages.Count > 0)
            {
                int imageCount = 0;
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                // Въртим цикъл през всички качени файлове
                foreach (var file in CarImages)
                {
                    if (imageCount >= 10) break; // Спираме, ако станат повече от 10!

                    if (file.Length > 0)
                    {
                        string uniqueFileName = System.Guid.NewGuid().ToString() + "_" + file.FileName;
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            file.CopyTo(fileStream);
                        }

                        savedImageUrls.Add("/images/" + uniqueFileName);
                        imageCount++;
                    }
                }

                // ЗАЛЕПВАМЕ ги в един текст, разделени с права черта "|"
                newCar.ImageUrl = string.Join("|", savedImageUrls);
            }
            else
            {
                newCar.ImageUrl = "/images/no-image.jpg";
            }
            if (SelectedExtras != null && SelectedExtras.Count > 0)
            {
                newCar.Extras = string.Join(", ", SelectedExtras);
            }
            else
            {
                newCar.Extras = "Няма посочени екстри";
            }
            _db.Cars.Add(newCar);
            _db.SaveChanges(); // ЗАПИСВАМЕ В БАЗАТА!

            return RedirectToAction("Index");
        }

        // 5. Триене от базата
        [HttpPost]
        public IActionResult Delete(int id)
        {
            var car = _db.Cars.FirstOrDefault(c => c.Id == id);
            if (car != null)
            {
                if (car.OwnerEmail == CurrentUserEmail || CurrentUserEmail == AdminEmail)
                {
                    _db.Cars.Remove(car);
                    _db.SaveChanges(); // ЗАПИСВАМЕ ИЗТРИВАНЕТО!
                }
            }
            return RedirectToAction("Index");
        }

        // --- ЛОГИН И РЕГИСТРАЦИЯ ---

        [HttpGet]
        public IActionResult Login() => View();

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        public IActionResult RegisterPost(string FullName, string Email, string Password)
        {
            // Създаваме нов потребител и го мятаме в базата
            var newUser = new User { FullName = FullName, Email = Email, Password = Password };
            _db.Users.Add(newUser);
            _db.SaveChanges();

            IsLoggedIn = true;
            CurrentUserName = FullName;
            CurrentUserEmail = Email;
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult LoginPost(string Email, string Password)
        {
            // Проверка за Админ
            if (Email == AdminEmail && Password == "admin123")
            {
                IsLoggedIn = true;
                CurrentUserName = "ШЕФА (Админ)";
                CurrentUserEmail = AdminEmail;
                return RedirectToAction("Index");
            }

            // Търсим потребителя в истинската база данни
            var user = _db.Users.FirstOrDefault(u => u.Email == Email && u.Password == Password);

            if (user != null)
            {
                IsLoggedIn = true;
                CurrentUserName = user.FullName;
                CurrentUserEmail = user.Email;
                return RedirectToAction("Index");
            }

            ViewBag.ErrorMessage = "Грешен имейл или парола!";
            return View("Login");
        }

        public IActionResult Logout()
        {
            IsLoggedIn = false;
            CurrentUserName = "";
            CurrentUserEmail = "";
            return RedirectToAction("Index");
        }

        public IActionResult MyAds()
        {
            // 1. Проверяваме дали потребителят е влязъл
            if (!IsLoggedIn)
            {
                return RedirectToAction("Login");
            }

            // 2. Взимаме само колите, които принадлежат на този мейл
            var myCars = _db.Cars.Where(c => c.OwnerEmail == CurrentUserEmail).ToList();

            return View(myCars);
        }

        // 1. Метод за добавяне в любими
        public IActionResult AddToFavorites(int id)
        {
            if (!IsLoggedIn) return RedirectToAction("Login");

            var existingFav = _db.Favorites.FirstOrDefault(f => f.UserEmail == CurrentUserEmail && f.CarAdId == id);

            if (existingFav != null)
            {
                // Ако вече е в любими -> ТРИЕМ ГО (Сърцето става празно)
                _db.Favorites.Remove(existingFav);
            }
            else
            {
                // Ако го няма -> ДОБАВЯМЕ ГО (Сърцето става плътно червено)
                var fav = new Favorite { UserEmail = CurrentUserEmail, CarAdId = id };
                _db.Favorites.Add(fav);
            }

            _db.SaveChanges(); // ЗАПАМЕТЯВАМЕ!

            // Връщаме потребителя на страницата, от която е кликнал
            return Redirect(Request.Headers["Referer"].ToString());
        }

        // 2. Метод за показване на Паркинга
        public IActionResult Parking()
        {
            if (!IsLoggedIn) return RedirectToAction("Login");

            var myFavorites = _db.Favorites
                .Where(f => f.UserEmail == CurrentUserEmail)
                .Select(f => f.CarAd)
                .ToList();

            return View(myFavorites);
        }
        // МЕТОД ЗА СРАВНЯВАНЕ НА АВТОМОБИЛИ
        public IActionResult Compare(List<int> selectedIds)
        {
            if (selectedIds == null || selectedIds.Count < 2)
            {
                ViewBag.Message = "Моля, изберете поне два автомобила за сравнение!";
                return RedirectToAction("Index");
            }

            // Взимаме колите от базата, чиито ID-та са в списъка
            var carsToCompare = _db.Cars.Where(c => selectedIds.Contains(c.Id)).ToList();

            return View(carsToCompare);
        }
    }
}