using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoRentCraiova.Models;
using Microsoft.AspNetCore.Authorization;

namespace AutoRentCraiova.Controllers
{
    [Authorize(Roles = "SystemAdministrator")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var messages = await _context.Contacts
                .OrderBy(c => c.IsRead)
                .ThenByDescending(c => c.Id)
                .ToListAsync();
            return View(messages);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleReadStatus(int id)
        {
            var message = await _context.Contacts.FindAsync(id);
            if (message != null)
            {
                message.IsRead = !message.IsRead;
                await _context.SaveChangesAsync();
                
                return Json(new { success = true, isRead = message.IsRead });
            }
            return NotFound();
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var message = await _context.Contacts.FirstOrDefaultAsync(m => m.Id == id);
            if (message == null) return NotFound();

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_MessageDeletePartial", message);
            }

            return View(message);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var message = await _context.Contacts.FindAsync(id);
            if (message != null)
            {
                _context.Contacts.Remove(message);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> PriceLogs()
        {
            var logs = await _context.PriceLogs
                .Include(l => l.Car)
                .OrderByDescending(l => l.ChangeDate)
                .ToListAsync();
            return View(logs);
        }
    }
}