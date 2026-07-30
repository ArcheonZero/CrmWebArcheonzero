using CrmWebArcheonzero.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CrmWebArcheonzero.Controllers
{
    public class AccountController : Controller
    {
        private readonly AuthService _authService;
        private readonly IDatabaseService _databaseService;

        public AccountController(AuthService authService, IDatabaseService databaseService)
        {
            _authService = authService;
            _databaseService = databaseService;
        }

        [HttpGet]
        public IActionResult Login(string returnUrl = "/Clients")
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }
        [HttpPost]
        public IActionResult SelectDatabase(string selectedProvider)
        {
            if (string.IsNullOrEmpty(selectedProvider))
            {
                ViewBag.Error = "База данных не выбрана.";
                return View("Login");
            }

            var dbService = HttpContext.RequestServices.GetRequiredService<IDatabaseService>();
            dbService.SetProvider(selectedProvider);
            var connectionString = GetConnectionStringForProvider(selectedProvider);
            dbService.SetConnectionString(connectionString);

            Console.WriteLine($"Выбрана база: {selectedProvider}");
            ViewBag.DbMessage = $"Выбрана база данных: {selectedProvider}";
            return View("Login");
        }
        [HttpPost]
        public async Task<IActionResult> Login(string username, string password, string selectedProvider, string returnUrl = "/Clients")
        {
            // Сохраняем выбор базы (даже если логин не удался)
            if (!string.IsNullOrEmpty(selectedProvider))
            {
                var dbService = HttpContext.RequestServices.GetRequiredService<IDatabaseService>();
                dbService.SetProvider(selectedProvider);
                var connectionString = GetConnectionStringForProvider(selectedProvider);
                dbService.SetConnectionString(connectionString);

                Console.WriteLine($"База сохранена: {selectedProvider}");
            }

            var user = await _authService.LoginAsync(username, password);
            if (user == null)
            {
                ViewBag.Error = "Неверный логин или пароль";
                return View();
            }

            var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Role, user.Role),
                    new Claim("UserId", user.Id.ToString())
                };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity));

            return Redirect(returnUrl);
        }

        public async Task<IActionResult> Logout()
        {
            // Сбрасываем выбор базы на SQLite
            _databaseService.SetProvider("Sqlite");
            _databaseService.SetConnectionString("Data Source=crm.db;Mode=ReadWriteCreate;Cache=Shared;");

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Users()
        {
            var users = await _authService.GetAllUsersAsync();
            return View(users);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ChangeRole(int userId, string role)
        {
            var currentUserId = int.Parse(User.FindFirst("UserId").Value);
            var currentUser = await _authService.GetUserByIdAsync(currentUserId);
            var targetUser = await _authService.GetUserByIdAsync(userId);

            if (targetUser == null)
            {
                TempData["Error"] = "Пользователь не найден.";
                return RedirectToAction(nameof(Users));
            }

            if (userId == currentUserId)
            {
                TempData["Error"] = "Вы не можете изменить роль своего аккаунта.";
                return RedirectToAction(nameof(Users));
            }

            if (targetUser.Role == "Admin" && currentUser.Role != "Admin")
            {
                TempData["Error"] = "Вы не можете изменить роль администратора.";
                return RedirectToAction(nameof(Users));
            }

            if (currentUser.Role == "User")
            {
                TempData["Error"] = "У вас нет прав на изменение ролей.";
                return RedirectToAction(nameof(Users));
            }

            if (currentUser.Role == "Manager" && role != "User")
            {
                TempData["Error"] = "Вы можете назначать только роль User.";
                return RedirectToAction(nameof(Users));
            }

            if (currentUser.Role == "SuperManager" && (role != "User" && role != "Manager"))
            {
                TempData["Error"] = "Вы можете назначать только роли User и Manager.";
                return RedirectToAction(nameof(Users));
            }

            if (currentUser.Role == "Admin")
            {
                await _authService.ChangeRoleAsync(userId, role);
                TempData["Success"] = "Роль пользователя изменена.";
                return RedirectToAction(nameof(Users));
            }

            TempData["Error"] = "Недостаточно прав для изменения роли.";
            return RedirectToAction(nameof(Users));
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ToggleUserStatus(int userId)
        {
            var currentUserId = int.Parse(User.FindFirst("UserId").Value);
            var currentUser = await _authService.GetUserByIdAsync(currentUserId);
            var targetUser = await _authService.GetUserByIdAsync(userId);

            if (targetUser == null)
            {
                TempData["Error"] = "Пользователь не найден.";
                return RedirectToAction(nameof(Users));
            }

            if (userId == currentUserId)
            {
                TempData["Error"] = "Вы не можете изменить статус своего аккаунта.";
                return RedirectToAction(nameof(Users));
            }

            if (targetUser.Role == "Admin")
            {
                TempData["Error"] = "Вы не можете изменить статус другого администратора.";
                return RedirectToAction(nameof(Users));
            }

            if (targetUser.Role == "Admin" && currentUser.Role != "Admin")
            {
                TempData["Error"] = "Вы не можете изменить статус администратора.";
                return RedirectToAction(nameof(Users));
            }

            await _authService.ToggleUserStatusAsync(userId);
            TempData["Success"] = $"Статус пользователя {targetUser.FullName} изменён.";
            return RedirectToAction(nameof(Users));
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateUser(string username, string password, string email, string role)
        {
            await _authService.RegisterAsync(username, password, email, username, role);
            return RedirectToAction(nameof(Users));
        }

        private string GetConnectionStringForProvider(string provider)
        {
            // Здесь можно загрузить строки подключения из appsettings.json
            // или использовать заранее заданные значения
            return provider switch
            {
                "PostgreSQL" => "Host=aws-0-eu-west-1.pooler.supabase.com;Port=5432;Database=postgres;Username=postgres.qnlvugqiokfjcerpvobx;Password=qqRWeKgP6Aoibruz;SSL Mode=Disable;",
                "SqlServer" => "Server=localhost\\SQLEXPRESS;Database=CrmDb;User Id=crm_user;Password=CrmUser123;MultipleActiveResultSets=true;Encrypt=false;",
                _ => "Data Source=crm.db;Mode=ReadWriteCreate;Cache=Shared;"
            };
        }
    }
}