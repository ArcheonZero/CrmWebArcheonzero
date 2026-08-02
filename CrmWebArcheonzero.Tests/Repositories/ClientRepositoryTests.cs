using Microsoft.EntityFrameworkCore;
using CrmWebArcheonzero.Data;
using CrmWebArcheonzero.Models;
using CrmWebArcheonzero.Services;
using Xunit;

namespace CrmWebArcheonzero.Tests.Repositories
{
    public class ClientRepositoryTests
    {
        private ApplicationDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task Add_ShouldAddClient()
        {
            var context = CreateContext();
            var repo = new ClientRepository(context);
            var client = new Client { Name = "Тест", Email = "test@test.com" };

            await repo.AddAsync(client, 1);
            var result = await repo.GetByIdAsync(client.Id);

            Assert.NotNull(result);
            Assert.Equal("Тест", result.Name);
        }

        [Fact]
        public async Task SoftDelete_ShouldMarkAsDeleted()
        {
            var context = CreateContext();
            var repo = new ClientRepository(context);
            var client = new Client { Name = "Тест" };
            await repo.AddAsync(client, 1);

            await repo.SoftDeleteAsync(client.Id, 1);
            var result = await repo.GetByIdAsync(client.Id);

            Assert.True(result.IsDeleted);
            Assert.NotNull(result.DeletedAt);
        }

        [Fact]
        public async Task Restore_ShouldRecoverClient()
        {
            var context = CreateContext();
            var repo = new ClientRepository(context);
            var client = new Client { Name = "Тест" };
            await repo.AddAsync(client, 1);
            await repo.SoftDeleteAsync(client.Id, 1);

            await repo.RestoreAsync(client.Id, 1);
            var result = await repo.GetByIdAsync(client.Id);

            Assert.False(result.IsDeleted);
            Assert.Null(result.DeletedAt);
        }

        [Fact]
        public async Task PermanentDelete_ShouldRemoveClient()
        {
            var context = CreateContext();
            var repo = new ClientRepository(context);
            var client = new Client { Name = "Тест" };
            await repo.AddAsync(client, 1);
            await repo.SoftDeleteAsync(client.Id, 1);

            await repo.PermanentDeleteAsync(client.Id);
            var result = await repo.GetByIdAsync(client.Id);

            Assert.Null(result);
        }

        [Fact]
        public async Task Search_ShouldReturnFilteredClients()
        {
            var context = CreateContext();
            var repo = new ClientRepository(context);
            await repo.AddAsync(new Client { Name = "Анна" }, 1);
            await repo.AddAsync(new Client { Name = "Борис" }, 1);

            var results = await repo.SearchAsync("Анна");

            Assert.Single(results);
            Assert.Equal("Анна", results.First().Name);
        }

        [Fact]
        public async Task GetDeleted_ShouldReturnOnlyDeleted()
        {
            var context = CreateContext();
            var repo = new ClientRepository(context);
            var active = new Client { Name = "Активный" };
            var deleted = new Client { Name = "Удалённый" };
            await repo.AddAsync(active, 1);
            await repo.AddAsync(deleted, 1);
            await repo.SoftDeleteAsync(deleted.Id, 1);

            var results = await repo.GetDeletedAsync();

            Assert.Single(results);
            Assert.Equal("Удалённый", results.First().Name);
        }
    }
}