using AutoRentCraiova.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using System.IO;

namespace AutoRentCraiova.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public HomeController(ApplicationDbContext context, IPasswordHasher<User> passwordHasher, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Contact(Contact contact)
        {
            if (ModelState.IsValid)
            {
                contact.IsRead = false;
                _context.Contacts.Add(contact);
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = "Mesajul tau a fost trimis cu succes! Te vom contacta in scurt timp.";
                return RedirectToAction(nameof(Contact));
            }
            return View(contact);
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
                if (user != null)
                {
                    var result = _passwordHasher.VerifyHashedPassword(user, user.Password, model.Password);
                    if (result == PasswordVerificationResult.Success || result == PasswordVerificationResult.SuccessRehashNeeded)
                    {
                        var claims = new[]
                        {
                            new Claim(ClaimTypes.Name, user.FullName),
                            new Claim(ClaimTypes.Role, user.Role.ToString()),
                            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
                        };

                        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                        var authProperties = new AuthenticationProperties
                        {
                            IsPersistent = model.RememberMe
                        };

                        await HttpContext.SignInAsync(
                            CookieAuthenticationDefaults.AuthenticationScheme,
                            new ClaimsPrincipal(claimsIdentity),
                            authProperties);

                        return RedirectToAction("Index", "Home");
                    }
                }
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            }
            return View(model);
        }

        public IActionResult SignUp()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignUp(SignUpViewModel model)
        {
            if (ModelState.IsValid)
            {
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError("Email", "An account with this email already exists.");
                    return View(model);
                }

                var user = new User
                {
                    FullName = model.FullName,
                    Email    = model.Email,
                    Phone    = model.Phone,
                    Role     = UserRole.Customer
                };
                user.Password = _passwordHasher.HashPassword(user, model.Password);

                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                return RedirectToAction("Login");
            }
            return View(model);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out var parsedId))
                return Unauthorized();

            var user = await _context.Users.FindAsync(parsedId);
            if (user == null)
            {
                return NotFound();
            }

            var viewModel = new ProfileViewModel
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                CNP = user.CNP,
                DriverLicenseUrl = user.DriverLicenseUrl,
                IdCardUrl = user.IdCardUrl,
                DocumentsStatus = user.DocumentsStatus
            };
            return View(viewModel);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(ProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out var parsedId))
                return Unauthorized();

            var user = await _context.Users.FindAsync(parsedId);

            if (user == null)
            {
                return NotFound();
            }

            user.FullName = model.FullName;
            user.Email    = model.Email;
            user.CNP      = model.CNP;

            bool documentsUploaded = false;

            string uploadDir = Path.Combine(_webHostEnvironment.WebRootPath, "imagini", "documente");
            if (!Directory.Exists(uploadDir))
            {
                Directory.CreateDirectory(uploadDir);
            }

            if (model.DriverLicenseImage != null)
            {
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + model.DriverLicenseImage.FileName;
                string filePath = Path.Combine(uploadDir, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await model.DriverLicenseImage.CopyToAsync(fileStream);
                }
                user.DriverLicenseUrl = uniqueFileName;
                documentsUploaded = true;
            }

            if (model.IdCardImage != null)
            {
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + model.IdCardImage.FileName;
                string filePath = Path.Combine(uploadDir, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await model.IdCardImage.CopyToAsync(fileStream);
                }
                user.IdCardUrl = uniqueFileName;
                documentsUploaded = true;
            }

            if (documentsUploaded)
            {
                user.DocumentsStatus = DocumentStatus.Pending;
            }

            var changes = await _context.SaveChangesAsync();

            if (changes > 0 || documentsUploaded)
            {
                TempData["SuccessMessage"] = "Profilul a fost actualizat cu succes.";
            }
            else
            {
                TempData["InfoMessage"] = "Nu s-au detectat modificari pentru a fi salvate.";
            }

            return RedirectToAction(nameof(Profile));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}