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

        public AccountController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }



        [HttpPost]
        public async Task<IActionResult> Login(string username, string password, string returnUrl = "/Clients")
        {
            var user = await _authService.LoginAsync(username, password);
            if (user == null)
            {
                ViewBag.Error = "Неверный логин или пароль";
                return View();
            }

            var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Role, user.Role),   // <-- ОБЯЗАТЕЛЬНО
                    new Claim("UserId", user.Id.ToString())
                };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity));

            return Redirect(returnUrl);
        }
        public async Task<IActionResult> Logout()
        {
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

            // User не может менять роли
            if (currentUser.Role == "User")
            {
                TempData["Error"] = "У вас нет прав на изменение ролей.";
                return RedirectToAction(nameof(Users));
            }

            // Manager может назначать только User
            if (currentUser.Role == "Manager" && role != "User")
            {
                TempData["Error"] = "Вы можете назначать только роль User.";
                return RedirectToAction(nameof(Users));
            }

            // SuperManager может назначать только User и Manager
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

            // Нельзя заблокировать себя
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
            // Нельзя заблокировать администратора, если ты не администратор
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
    }
}