using ASP.Helpers;
using ASP.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ASP.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ILogger<AccountController> _logger;

        public AccountController(AppDbContext context, ILogger<AccountController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public IActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var users = _context.Users.AsQueryable();
            User? user = null;

            if (DateTime.TryParse(model.Identifier, out var birthDate))
            {
                user = users.FirstOrDefault(u =>
                    u.BirthDate.Date == birthDate.Date &&
                    u.DeletedAt == null);
            }
            else if (model.Identifier.Contains('@'))
            {
                user = users.FirstOrDefault(u =>
                    u.Email.ToLower() == model.Identifier.ToLower());
            }
            else
            {
                user = users.FirstOrDefault(u =>
                    u.Name.ToLower() == model.Identifier.ToLower());
            }

            if (user == null)
            {
                ViewBag.Error = "Користувача не знайдено";
                return View(model);
            }

            if (!PasswordHelper.VerifyPasswordHash(model.Password, user.PasswordHash, user.PasswordSalt))
            {
                ViewBag.Error = "Невірний пароль";
                return View(model);
            }

            if (user.DeletedAt != null)
            {
                if (model.RestoreDate.HasValue && user.CreatedAt.Date == model.RestoreDate.Value.Date)
                {
                    user.DeletedAt = null;
                    _context.SaveChanges();
                    ViewBag.Message = "Користувача успішно відновлено!";
                }
                else
                {
                    ViewBag.Error = $"Користувач був видалений. Для відновлення введіть дату реєстрації: {user.CreatedAt:yyyy-MM-dd}";
                    return View(model);
                }
            }

            HttpContext.Session.SetString("UserName", user.Name);
            HttpContext.Session.SetString("UserEmail", user.Email);
            return RedirectToAction("Profile");
        }


        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }


        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ChangePassword(ChangePasswordViewModel model)
        {
            var email = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(email)) return RedirectToAction("Login");

            var user = _context.Users.FirstOrDefault(u => u.Email == email);
            if (user == null) return RedirectToAction("Login");

            if (!PasswordHelper.VerifyPasswordHash(model.OldPassword, user.PasswordHash, user.PasswordSalt))
            {
                ModelState.AddModelError("", "Старий пароль невірний");
                return View(model);
            }

            PasswordHelper.CreatePasswordHash(model.NewPassword, out var newHash, out var newSalt);
            user.PasswordHash = newHash;
            user.PasswordSalt = newSalt;

            _context.SaveChanges();

            ViewBag.Message = "Пароль змінено успішно";
            return View();
        }

        public IActionResult Profile()
        {
            var email = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(email))
                return RedirectToAction("Login");

            var user = _context.Users.FirstOrDefault(u => u.Email == email);
            if (user == null)
                return RedirectToAction("Login");

            var feedbacks = _context.Feedbacks
                .Where(f => f.UserId == user.Id.ToString())
                .ToList();

            var model = new ProfileViewModel
            {
                User = user,
                Feedbacks = feedbacks
            };

            return View(model);
        }

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        public IActionResult Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            if (_context.Users.Any(u => u.Email == model.Email))
            {
                ModelState.AddModelError("Email", "Користувач з таким email вже існує");
                return View(model);
            }

            PasswordHelper.CreatePasswordHash(model.Password, out var hash, out var salt);

            var user = new User
            {
                Name = model.Name,
                Email = model.Email,
                BirthDate = model.BirthDate,
                PasswordHash = hash,
                PasswordSalt = salt
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            HttpContext.Session.SetString("UserEmail", user.Email);
            HttpContext.Session.SetString("UserName", user.Name);
            HttpContext.Session.SetString("UserRole", user.Role);

            return RedirectToAction("Profile");
        }

        [HttpPost]
        public IActionResult DeleteAccount()
        {
            var email = HttpContext.Session.GetString("UserEmail");
            if (email == null) return RedirectToAction("Login");

            var user = _context.Users.FirstOrDefault(u => u.Email == email);
            if (user != null)
            {
                user.DeletedAt = DateTime.Now;
                _context.SaveChanges();
            }

            HttpContext.Session.Clear();
            TempData["RestoreInfo"] = $"Для відновлення введіть дату реєстрації: {user.CreatedAt:yyyy-MM-dd}";
            return RedirectToAction("Login");
        }
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}