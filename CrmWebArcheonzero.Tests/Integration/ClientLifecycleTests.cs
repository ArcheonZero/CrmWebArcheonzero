using Microsoft.EntityFrameworkCore;
using CrmWebArcheonzero.Data;
using CrmWebArcheonzero.Models;
using CrmWebArcheonzero.Services;
using Xunit;

namespace CrmWebArcheonzero.Tests.Integration
{
    public class ClientLifecycleTests
    {
        [Fact]
        public async Task FullCycle_ShouldWorkCorrectly()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            using var context = new ApplicationDbContext(options);
            var repo = new ClientRepository(context);

            // 1. Создание
            var client = new Client { Name = "Интеграционный тест", Email = "integration@test.com" };
            await repo.AddAsync(client, 1);

            // 2. Поиск
            var found = await repo.GetByIdAsync(client.Id);
            Assert.NotNull(found);

            // 3. Обновление
            found.Name = "Обновлённый";
            await repo.UpdateAsync(found, 1);
            var updated = await repo.GetByIdAsync(client.Id);
            Assert.Equal("Обновлённый", updated.Name);

            // 4. Мягкое удаление
            await repo.SoftDeleteAsync(client.Id, 1);
            var deleted = await repo.GetByIdAsync(client.Id);
            Assert.True(deleted.IsDeleted);

            // 5. Восстановление
            await repo.RestoreAsync(client.Id, 1);
            var restored = await repo.GetByIdAsync(client.Id);
            Assert.False(restored.IsDeleted);

            // 6. Окончательное удаление
            await repo.SoftDeleteAsync(client.Id, 1);
            await repo.PermanentDeleteAsync(client.Id);
            var final = await repo.GetByIdAsync(client.Id);
            Assert.Null(final);
        }
    }
}