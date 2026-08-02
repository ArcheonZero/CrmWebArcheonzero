using CrmWebArcheonzero.Controllers;
using CrmWebArcheonzero.Interfaces;
using CrmWebArcheonzero.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CrmWebArcheonzero.Tests.Controllers
{
    public class ClientsControllerTests
    {
        // ============================================================
        // ТЕСТОВЫЙ КОНТРОЛЛЕР С ФИКСИРОВАННЫМ USERID
        // ============================================================
        private class TestableClientsController : ClientsController
        {
            private readonly int _testUserId;

            public TestableClientsController(
                IClientRepository clientRepository,
                IHistoryRepository historyRepository,
                ILogger<ClientsController> logger,
                IEmailService emailService,
                int testUserId = 1)
                : base(clientRepository, historyRepository, logger, emailService)
            {
                _testUserId = testUserId;
            }

            protected override int GetCurrentUserId() => _testUserId;
        }

        // ============================================================
        // ВСПОМОГАТЕЛЬНЫЙ МЕТОД ДЛЯ СОЗДАНИЯ КОНТРОЛЛЕРА
        // ============================================================
        private TestableClientsController CreateController(
            out Mock<IClientRepository> clientRepo,
            out Mock<IHistoryRepository> historyRepo,
            out Mock<IEmailService> emailService,
            int userId = 1)
        {
            clientRepo = new Mock<IClientRepository>();
            historyRepo = new Mock<IHistoryRepository>();
            emailService = new Mock<IEmailService>();
            var logger = new Mock<ILogger<ClientsController>>();

            var controller = new TestableClientsController(
                clientRepo.Object,
                historyRepo.Object,
                logger.Object,
                emailService.Object,
                userId);

            // ✅ Добавляем TempData
            controller.TempData = new TempDataDictionary(
                new DefaultHttpContext(),
                Mock.Of<ITempDataProvider>()
            );

            return controller;
        }

        // ============================================================
        // ТЕСТЫ
        // ============================================================

        [Fact]
        public async Task Create_ShouldRedirectToIndex_WhenValid()
        {
            // Arrange
            var controller = CreateController(out var clientRepo, out _, out var emailService);
            var client = new Client { Name = "Тест", Email = "test@test.com" };

            // Act
            var result = await controller.Create(client) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Index", result.ActionName);
            clientRepo.Verify(r => r.AddAsync(client, It.IsAny<int>()), Times.Once);
            emailService.Verify(e => e.SendClientCreatedEmail(client.Email, client.Name), Times.Once);
        }

        [Fact]
        public async Task Details_ShouldReturnNotFound_WhenClientMissing()
        {
            // Arrange
            var controller = CreateController(out var clientRepo, out _, out _);
            clientRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((Client)null);

            // Act
            var result = await controller.Details(999);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Details_ShouldReturnView_WhenClientExists()
        {
            // Arrange
            var controller = CreateController(out var clientRepo, out _, out _);
            var client = new Client { Id = 1, Name = "Тест" };
            clientRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(client);

            // Act
            var result = await controller.Details(1) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(client, result.Model);
        }

        [Fact]
        public async Task Edit_ShouldRedirectToIndex_WhenValid()
        {
            // Arrange
            var controller = CreateController(out var clientRepo, out _, out var emailService);
            var existing = new Client { Id = 1, Name = "Старое имя", Email = "old@test.com" };
            clientRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existing);

            var updated = new Client { Id = 1, Name = "Новое имя", Email = "new@test.com" };

            // Act
            var result = await controller.Edit(1, updated) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Index", result.ActionName);
            clientRepo.Verify(r => r.UpdateAsync(It.IsAny<Client>(), It.IsAny<int>()), Times.Once);
            emailService.Verify(e => e.SendClientUpdatedEmail(updated.Email, updated.Name), Times.Once);
        }

        [Fact]
        public async Task DeleteConfirmed_ShouldSoftDeleteClient()
        {
            // Arrange
            var controller = CreateController(out var clientRepo, out _, out var emailService);
            var client = new Client { Id = 1, Name = "Тест", Email = "test@test.com" };
            clientRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(client);

            // Act
            var result = await controller.DeleteConfirmed(1) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Index", result.ActionName);
            clientRepo.Verify(r => r.SoftDeleteAsync(1, It.IsAny<int>()), Times.Once);
            emailService.Verify(e => e.SendClientDeletedEmail(client.Email, client.Name), Times.Once);
        }

        [Fact]
        public async Task History_ShouldReturnView_WhenClientExists()
        {
            // Arrange
            var controller = CreateController(out var clientRepo, out var historyRepo, out _);
            var client = new Client { Id = 1, Name = "Тест" };
            clientRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(client);
            historyRepo.Setup(r => r.GetByClientAsync(1))
                .ReturnsAsync(new List<AssignmentHistory>());

            // Act
            var result = await controller.History(1) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Тест", result.ViewData["ClientName"]);
        }

        [Fact]
        public async Task History_ShouldReturnNotFound_WhenClientMissing()
        {
            // Arrange
            var controller = CreateController(out var clientRepo, out _, out _);
            clientRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((Client)null);

            // Act
            var result = await controller.History(999);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Restore_ShouldRedirectToIndex()
        {
            // Arrange
            var controller = CreateController(out var clientRepo, out _, out _);

            // Act
            var result = await controller.Restore(1) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Index", result.ActionName);
            clientRepo.Verify(r => r.RestoreAsync(1, It.IsAny<int>()), Times.Once);
        }

        [Fact]
        public async Task Deleted_ShouldReturnViewWithDeletedClients()
        {
            // Arrange
            var controller = CreateController(out var clientRepo, out _, out _);
            var deletedClients = new List<Client>
            {
                new Client { Id = 1, Name = "Удалённый 1", IsDeleted = true },
                new Client { Id = 2, Name = "Удалённый 2", IsDeleted = true }
            };
            clientRepo.Setup(r => r.GetDeletedAsync()).ReturnsAsync(deletedClients);

            // Act
            var result = await controller.Deleted() as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(deletedClients, result.Model);
        }


        [Fact]
        public async Task PermanentDelete_ShouldRedirectToDeleted()
        {
            // Arrange
            var controller = CreateController(out var clientRepo, out _, out _);
            clientRepo.Setup(r => r.PermanentDeleteAsync(It.IsAny<int>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await controller.PermanentDelete(1) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Deleted", result.ActionName);
            clientRepo.Verify(r => r.PermanentDeleteAsync(1), Times.Once);
        }
    }
}