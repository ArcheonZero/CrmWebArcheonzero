using Microsoft.AspNetCore.Mvc;
using Moq;
using CrmWebArcheonzero.Controllers;
using CrmWebArcheonzero.Interfaces;
using CrmWebArcheonzero.Services;
using Xunit;
using CrmWebArcheonzero.Models;

namespace CrmWebArcheonzero.Tests.Controllers
{
    public class ExportControllerTests
    {

        [Fact]
        public async Task Excel_ShouldReturnFile_WhenClientsExist()
        {
            // Arrange
            var repo = new Mock<IClientRepository>();
            repo.Setup(r => r.GetAllAsync())
                .ReturnsAsync(new List<Client> { new Client { Name = "Тест" } });

            var exportService = new Mock<IExportService>();
            exportService.Setup(s => s.ExportClientsList(It.IsAny<List<Client>>(), "xlsx"))
                .Returns(new byte[] { 1, 2, 3 });

            var controller = new ExportController(repo.Object, exportService.Object);

            // Act
            var result = await controller.Excel() as FileContentResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", result.ContentType);
        }
    }
}