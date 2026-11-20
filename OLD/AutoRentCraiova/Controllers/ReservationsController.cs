using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoRentCraiova.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutoRentCraiova.Controllers
{
    [Authorize]
    public class ReservationsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReservationsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Reservations/MyBookings
        public async Task<IActionResult> MyBookings()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
            {
                return RedirectToAction("Login", "Home");
            }

            var reservations = await _context.Reservations
                .Include(r => r.Car)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.StartDate)
                .ToListAsync();

            return View(reservations);
        }

        // GET: /Reservations/Book/{carId}
        [HttpGet]
        public async Task<IActionResult> Book(int id)
        {
            var car = await _context.Cars.FindAsync(id);
            if (car == null) return NotFound();

            var model = new BookViewModel
            {
                CarId = car.Id,
                Car = car,
                StartDate = DateTime.Today.AddDays(1),
                EndDate = DateTime.Today.AddDays(3)
            };

            return View(model);
        }

        // POST: /Reservations/Book
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Book(BookViewModel model)
        {
            var car = await _context.Cars.FindAsync(model.CarId);
            if (car == null) return NotFound();

            if (model.EndDate <= model.StartDate)
            {
                ModelState.AddModelError("EndDate", "Data de returnare trebuie sa fie dupa data de preluare.");
            }

            if (model.StartDate < DateTime.Today)
            {
                ModelState.AddModelError("StartDate", "Nu poti rezerva pentru o data din trecut.");
            }

            var overlapping = await _context.Reservations
                .AnyAsync(r => r.CarId == model.CarId && 
                               r.Status != ReservationStatus.Cancelled &&
                               r.StartDate < model.EndDate && 
                               r.EndDate > model.StartDate);

            if (overlapping)
            {
                ModelState.AddModelError("", "Masina este deja rezervata in aceasta perioada.");
            }

            if (ModelState.IsValid)
            {
                var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
                {
                    return RedirectToAction("Login", "Home");
                }

                int days = (model.EndDate - model.StartDate).Days;
                if (days < 1) days = 1;

                decimal insuranceCostPerDay = model.Insurance switch
                {
                    InsuranceType.Basic => 0,
                    InsuranceType.Standard => 50,
                    InsuranceType.Full => 100,
                    _ => 0
                };

                decimal extrasPerDay = 0;
                if (model.HasGps) extrasPerDay += 15;
                if (model.HasChildSeat) extrasPerDay += 25;
                if (model.HasRoofRack) extrasPerDay += 35;

                decimal totalPrice = days * (car.PricePerDay + insuranceCostPerDay + extrasPerDay);

                var reservation = new Reservation
                {
                    CarId = model.CarId,
                    UserId = userId,
                    StartDate = model.StartDate,
                    EndDate = model.EndDate,
                    Insurance = model.Insurance,
                    HasGps = model.HasGps,
                    HasChildSeat = model.HasChildSeat,
                    HasRoofRack = model.HasRoofRack,
                    TotalPrice = totalPrice,
                    Status = ReservationStatus.PendingApproval,
                    BookingReference = Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper()
                };

                _context.Reservations.Add(reservation);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Rezervarea a fost creata cu succes si asteapta aprobare!";
                return RedirectToAction(nameof(MyBookings));
            }

            model.Car = car;
            return View(model);
        }
        // GET: /Reservations/Contract/{id}
        [HttpGet]
        public async Task<IActionResult> Contract(int id)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
            {
                return Unauthorized();
            }

            var reservation = await _context.Reservations
                .Include(r => r.Car)
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reservation == null)
            {
                return NotFound();
            }

            if (reservation.UserId != userId && !User.IsInRole("SystemAdministrator"))
            {
                return Forbid();
            }

            if (reservation.Status == ReservationStatus.PendingApproval || reservation.Status == ReservationStatus.Cancelled)
            {
                return BadRequest("Contractul este disponibil doar pentru rezervarile confirmate sau finalizate.");
            }

            return View(reservation);
        }
    }
}
