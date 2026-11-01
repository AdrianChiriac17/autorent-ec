using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using AutoRentCraiova.Models;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System;
using Microsoft.AspNetCore.Authorization;

namespace AutoRentCraiova.Controllers
{
    [Authorize(Roles = "FleetManager,SystemAdministrator")]
    public class CarsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private const int PageSize = 6;

        public CarsController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index(string? searchString, int? categoryId, string? transmission, string? fuelType, CarStatus? status, string? sortOrder, bool clearSearch = false)
        {
            if (clearSearch)
            {
                HttpContext.Session.Remove("LastSearch");
                HttpContext.Session.Remove("LastCategory");
                HttpContext.Session.Remove("LastTransmission");
                HttpContext.Session.Remove("LastFuel");
                HttpContext.Session.Remove("LastStatus");
                HttpContext.Session.Remove("LastSort");
                searchString = null;
                categoryId = null;
                transmission = null;
                fuelType = null;
                status = null;
                sortOrder = null;
            }
            else if (Request.Query.Count > 0)
            {
                HttpContext.Session.SetString("LastSearch", searchString ?? "");
                HttpContext.Session.SetString("LastCategory", categoryId?.ToString() ?? "");
                HttpContext.Session.SetString("LastTransmission", transmission ?? "");
                HttpContext.Session.SetString("LastFuel", fuelType ?? "");
                HttpContext.Session.SetString("LastStatus", status?.ToString() ?? "");
                HttpContext.Session.SetString("LastSort", sortOrder ?? "");
            }
            else
            {
                searchString = HttpContext.Session.GetString("LastSearch");
                
                var catStr = HttpContext.Session.GetString("LastCategory");
                if (int.TryParse(catStr, out int cId)) categoryId = cId;
                
                transmission = HttpContext.Session.GetString("LastTransmission");
                if (string.IsNullOrEmpty(transmission)) transmission = null;

                fuelType = HttpContext.Session.GetString("LastFuel");
                if (string.IsNullOrEmpty(fuelType)) fuelType = null;

                var statusStr = HttpContext.Session.GetString("LastStatus");
                if (Enum.TryParse<CarStatus>(statusStr, out CarStatus parsedStatus)) status = parsedStatus;

                sortOrder = HttpContext.Session.GetString("LastSort");
                if (string.IsNullOrEmpty(sortOrder)) sortOrder = null;
            }

            ViewData["CurrentSearch"] = string.IsNullOrEmpty(searchString) ? null : searchString;
            ViewData["CurrentCategory"] = categoryId;
            ViewData["CurrentTransmission"] = string.IsNullOrEmpty(transmission) ? null : transmission;
            ViewData["CurrentFuel"] = string.IsNullOrEmpty(fuelType) ? null : fuelType;
            ViewData["CurrentStatus"] = status;
            ViewData["CurrentSort"] = string.IsNullOrEmpty(sortOrder) ? null : sortOrder;

            ViewBag.TotalCars = await _context.Cars.CountAsync();
            ViewBag.AvailableCars = await _context.Cars.CountAsync(c => c.Status == CarStatus.Disponibila);
            ViewBag.RentedCars = await _context.Cars.CountAsync(c => c.Status == CarStatus.Inchiriata);
            ViewBag.MaintenanceCars = await _context.Cars.CountAsync(c => c.Status == CarStatus.InMentenanta);

            var carsQuery = GetCarsQuery(searchString, categoryId, transmission, fuelType, status, sortOrder);
            var cars = await carsQuery.Take(PageSize).ToListAsync();

            ViewBag.Categories = new SelectList(await _context.Categories.ToListAsync(), "Id", "Name", categoryId);
            ViewBag.Transmissions = new SelectList(new[] { "Manuala", "Automata" }, transmission);
            ViewBag.FuelTypes = new SelectList(new[] { "Benzina", "Benzina + GPL", "Benzina + Hybrid", "Diesel", "Electric" }, fuelType);
            
            var statuses = Enum.GetValues(typeof(CarStatus)).Cast<CarStatus>().Select(s => new { Id = (int)s, Name = s.ToString() });
            ViewBag.Statuses = new SelectList(statuses, "Id", "Name", status);

            return View(cars);
        }

        [AllowAnonymous]
        public async Task<IActionResult> LoadMoreCars(int page, string? searchString, int? categoryId, string? transmission, string? fuelType, CarStatus? status, string? sortOrder)
        {
            var carsQuery = GetCarsQuery(searchString, categoryId, transmission, fuelType, status, sortOrder);
            var cars = await carsQuery.Skip(page * PageSize).Take(PageSize).ToListAsync();
            
            if (!cars.Any())
            {
                return Content("");
            }

            return PartialView("_CarsListPartial", cars);
        }

        private IQueryable<Car> GetCarsQuery(string? searchString, int? categoryId, string? transmission, string? fuelType, CarStatus? status, string? sortOrder)
        {
            var cars = _context.Cars.Include(c => c.Category).AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                var lowerSearchString = searchString.ToLower();
                cars = cars.Where(c => c.Brand.ToLower().Contains(lowerSearchString) || 
                                       c.Model.ToLower().Contains(lowerSearchString) ||
                                       c.VIN.ToLower().Contains(lowerSearchString) ||
                                       c.LicensePlate.ToLower().Contains(lowerSearchString));
            }

            if (categoryId.HasValue)
            {
                cars = cars.Where(c => c.CategoryId == categoryId.Value);
            }

            if (!string.IsNullOrEmpty(transmission))
            {
                cars = cars.Where(c => c.Transmission == transmission);
            }

            if (!string.IsNullOrEmpty(fuelType))
            {
                cars = cars.Where(c => c.FuelType == fuelType);
            }

            if (status.HasValue)
            {
                cars = cars.Where(c => c.Status == status.Value);
            }

            cars = cars.OrderBy(c => c.Status);

            switch (sortOrder)
            {
                case "price_asc":
                    cars = ((IOrderedQueryable<Car>)cars).ThenBy(c => c.PricePerDay);
                    break;
                case "price_desc":
                    cars = ((IOrderedQueryable<Car>)cars).ThenByDescending(c => c.PricePerDay);
                    break;
                default:
                    cars = ((IOrderedQueryable<Car>)cars).ThenByDescending(c => c.Id);
                    break;
            }
            return cars;
        }

        [HttpPost]
        public async Task<IActionResult> ChangeStatus(int id, CarStatus newStatus)
        {
            var car = await _context.Cars.FindAsync(id);
            if (car != null)
            {
                car.Status = newStatus;
                await _context.SaveChangesAsync();
                return Json(new { success = true, status = car.Status.ToString() });
            }
            return NotFound();
        }

        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var car = await _context.Cars
                .Include(c => c.Category)
                .Include(c => c.Images)
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (car == null) return NotFound();

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_CarDetailsPartial", car);
            }

            return View(car);
        }

        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Brand,Model,Year,Transmission,FuelType,PricePerDay,HasAC,Seats,Doors,AverageConsumption,CategoryId,ImageFiles,VIN,LicensePlate")] Car car)
        {
            if (ModelState.IsValid)
            {
                car.Status = CarStatus.Disponibila;
                
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "imagini");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                if (car.ImageFiles != null && car.ImageFiles.Count > 0)
                {
                    bool isFirst = true;
                    foreach (var file in car.ImageFiles)
                    {
                        if (file.Length > 0)
                        {
                            string uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
                            string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                            
                            using (var fileStream = new FileStream(filePath, FileMode.Create))
                            {
                                await file.CopyToAsync(fileStream);
                            }

                            if (isFirst)
                            {
                                car.ImageUrl = uniqueFileName;
                                isFirst = false;
                            }
                            else
                            {
                                car.Images.Add(new CarImage { ImageUrl = uniqueFileName });
                            }
                        }
                    }
                }
                
                if (string.IsNullOrEmpty(car.ImageUrl))
                {
                    car.ImageUrl = "default-car.png";
                }

                _context.Add(car);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", car.CategoryId);
            return View(car);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var car = await _context.Cars.Include(c => c.Images).FirstOrDefaultAsync(c => c.Id == id);
            if (car == null) return NotFound();
            
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", car.CategoryId);
            
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_CarEditPartial", car);
            }

            return View(car);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Brand,Model,Year,Transmission,FuelType,PricePerDay,HasAC,Seats,Doors,AverageConsumption,CategoryId,ImageFiles,ImageUrl,Status,VIN,LicensePlate")] Car car)
        {
            if (id != car.Id) return Json(new { success = false, message = "Car not found." });

            if (ModelState.IsValid)
            {
                try
                {
                    var carToUpdate = await _context.Cars.Include(c => c.Images).FirstOrDefaultAsync(c => c.Id == id);
                    if (carToUpdate == null) return Json(new { success = false, message = "Car not found." });

                    // Verificam daca pretul s-a schimbat pentru audit (REQ-40)
                    if (carToUpdate.PricePerDay != car.PricePerDay)
                    {
                        var priceLog = new PriceLog
                        {
                            CarId = carToUpdate.Id,
                            OldPrice = carToUpdate.PricePerDay,
                            NewPrice = car.PricePerDay,
                            ChangeDate = DateTime.Now
                        };
                        _context.PriceLogs.Add(priceLog);
                    }

                    carToUpdate.Brand = car.Brand;
                    carToUpdate.Model = car.Model;
                    carToUpdate.Year = car.Year;
                    carToUpdate.Transmission = car.Transmission;
                    carToUpdate.FuelType = car.FuelType;
                    carToUpdate.PricePerDay = car.PricePerDay;
                    carToUpdate.HasAC = car.HasAC;
                    carToUpdate.Seats = car.Seats;
                    carToUpdate.Doors = car.Doors;
                    carToUpdate.AverageConsumption = car.AverageConsumption;
                    carToUpdate.CategoryId = car.CategoryId;
                    carToUpdate.Status = car.Status;
                    carToUpdate.VIN = car.VIN;
                    carToUpdate.LicensePlate = car.LicensePlate;

                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "imagini");

                    if (car.ImageFiles != null && car.ImageFiles.Count > 0)
                    {
                        foreach (var file in car.ImageFiles)
                        {
                            if (file.Length > 0)
                            {
                                string uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
                                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                                
                                using (var fileStream = new FileStream(filePath, FileMode.Create))
                                {
                                    await file.CopyToAsync(fileStream);
                                }
                                
                                if (string.IsNullOrEmpty(carToUpdate.ImageUrl) || carToUpdate.ImageUrl == "default-car.png")
                                {
                                    carToUpdate.ImageUrl = uniqueFileName;
                                }
                                else
                                {
                                    carToUpdate.Images.Add(new CarImage { ImageUrl = uniqueFileName });
                                }
                            }
                        }
                    }

                    _context.Update(carToUpdate);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CarExists(car.Id)) return Json(new { success = false, message = "Car not found." });
                    else throw;
                }
                return Json(new { success = true });
            }
            
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            return Json(new { success = false, message = "Validation error", errors = errors });
        }
        
        [HttpPost]
        public async Task<IActionResult> DeleteImage(int imageId)
        {
            var image = await _context.CarImages.FindAsync(imageId);
            if (image == null) return Json(new { success = false, message = "Imaginea nu a fost gasita." });

            string filePath = Path.Combine(_webHostEnvironment.WebRootPath, "imagini", image.ImageUrl);
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }

            _context.CarImages.Remove(image);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var car = await _context.Cars
                .Include(c => c.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (car == null) return NotFound();

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_CarDeletePartial", car);
            }

            return View(car);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var car = await _context.Cars.Include(c => c.Images).FirstOrDefaultAsync(c => c.Id == id);
            if (car != null)
            {
                if (!string.IsNullOrEmpty(car.ImageUrl) && car.ImageUrl != "default-car.png")
                {
                    string filePath = Path.Combine(_webHostEnvironment.WebRootPath, "imagini", car.ImageUrl);
                    if (System.IO.File.Exists(filePath)) System.IO.File.Delete(filePath);
                }
                
                foreach (var img in car.Images)
                {
                    string filePath = Path.Combine(_webHostEnvironment.WebRootPath, "imagini", img.ImageUrl);
                    if (System.IO.File.Exists(filePath)) System.IO.File.Delete(filePath);
                }

                _context.Cars.Remove(car);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }

            return Json(new { success = false, message = "Car not found." });
        }

        public async Task<IActionResult> FixImages()
        {
            var cars = await _context.Cars.ToListAsync();
            int updatedCount = 0;
            foreach(var car in cars)
            {
                if(string.IsNullOrEmpty(car.ImageUrl))
                {
                     car.ImageUrl = car.Model.ToLower().Replace(" ", "") + ".png";
                     _context.Update(car);
                     updatedCount++;
                }
            }
            await _context.SaveChangesAsync();
            return Content($"Au fost actualizate {updatedCount} masini.");
        }

        private bool CarExists(int id)
        {
            return _context.Cars.Any(e => e.Id == id);
        }
    }
}