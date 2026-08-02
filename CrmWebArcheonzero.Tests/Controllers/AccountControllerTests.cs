using CrmWebArcheonzero.Controllers;
using CrmWebArcheonzero.Interfaces;
using CrmWebArcheonzero.Models;
using CrmWebArcheonzero.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Security.Claims;
using Xunit;
using Microsoft.AspNetCore.Routing;

namespace CrmWebArcheonzero.Tests.Controllers
{
    public class AccountControllerTests
    {
        private AccountController CreateController(
            out Mock<AuthService> authService,
            out Mock<IDatabaseService> dbService,
            int userId = 1,
            string role = "Admin")
        {
            authService = new Mock<AuthService>(null);
            dbService = new Mock<IDatabaseService>();

            var controller = new AccountController(authService.Object, dbService.Object);

            // Настраиваем HttpContext
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddAuthentication().AddCookie();
            services.AddMvc();

            var serviceProvider = services.BuildServiceProvider();

            var httpContext = new DefaultHttpContext
            {
                RequestServices = serviceProvider
            };

            // Настраиваем TempData
            var tempDataProvider = serviceProvider.GetRequiredService<ITempDataProvider>();
            controller.TempData = new TempDataDictionary(httpContext, tempDataProvider);

            // ✅ Добавляем ActionContext
            var actionContext = new ActionContext(
                httpContext,
                new RouteData(),
                new ControllerActionDescriptor());

            controller.ControllerContext = new ControllerContext(actionContext)
            {
                HttpContext = httpContext
            };

            // Подменяем User
            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, "test"),
        new Claim(ClaimTypes.Role, role),
        new Claim("UserId", userId.ToString())
    };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);

            controller.ControllerContext.HttpContext.User = principal;

            return controller;
        }

        [Fact]
        public async Task Login_ShouldRedirect_WhenValidCredentials()
        {
            // Arrange
            var controller = CreateController(out var authService, out _);
            var user = new User { Id = 1, Username = "test", Role = "User" };
            authService.Setup(a => a.LoginAsync("test", "pass"))
                .ReturnsAsync(user);

            // Act
            var result = await controller.Login("test", "pass", null, "/Clients") as RedirectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("/Clients", result.Url);
        }

        [Fact]
        public async Task Login_ShouldReturnView_WhenInvalidCredentials()
        {
            // Arrange
            var controller = CreateController(out var authService, out _);
            authService.Setup(a => a.LoginAsync("test", "wrong"))
                .ReturnsAsync((User)null);

            // Act
            var result = await controller.Login("test", "wrong", null, "/Clients") as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Неверный логин или пароль", controller.ViewBag.Error);
        }

        [Fact]
        public async Task Users_ShouldReturnView_WhenAdmin()
        {
            // Arrange
            var controller = CreateController(out var authService, out _, role: "Admin");
            var users = new List<User> { new User { Id = 1, Username = "test" } };
            authService.Setup(a => a.GetAllUsersAsync())
                .ReturnsAsync(users);

            // Act
            var result = await controller.Users() as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(users, result.Model);
        }

        [Fact]
        public async Task ChangeRole_ShouldRedirectToUsers_WhenValid()
        {
            // Arrange
            var controller = CreateController(out var authService, out _, userId: 1, role: "Admin");
            var currentUser = new User { Id = 1, Role = "Admin" };
            var targetUser = new User { Id = 2, Role = "User" };

            authService.Setup(a => a.GetUserByIdAsync(1)).ReturnsAsync(currentUser);
            authService.Setup(a => a.GetUserByIdAsync(2)).ReturnsAsync(targetUser);

            // Act
            var result = await controller.ChangeRole(2, "Manager") as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Users", result.ActionName);
            authService.Verify(a => a.ChangeRoleAsync(2, "Manager"), Times.Once);
        }

        [Fact]
        public async Task ToggleUserStatus_ShouldRedirectToUsers_WhenValid()
        {
            // Arrange
            var controller = CreateController(out var authService, out _, userId: 1, role: "Admin");
            var currentUser = new User { Id = 1, Role = "Admin" };
            var targetUser = new User { Id = 2, Role = "User", IsActive = true };

            authService.Setup(a => a.GetUserByIdAsync(1)).ReturnsAsync(currentUser);
            authService.Setup(a => a.GetUserByIdAsync(2)).ReturnsAsync(targetUser);

            // Act
            var result = await controller.ToggleUserStatus(2) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Users", result.ActionName);
            authService.Verify(a => a.ToggleUserStatusAsync(2), Times.Once);
        }
    }
}