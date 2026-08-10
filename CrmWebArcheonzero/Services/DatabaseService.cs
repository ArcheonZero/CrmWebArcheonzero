using CrmWebArcheonzero.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace CrmWebArcheonzero.Services
{
    public class DatabaseService : IDatabaseService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;

        public DatabaseService(IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
        }

        private ISession Session => _httpContextAccessor.HttpContext?.Session;

        public string GetProvider()
        {
            // Сохраняем выбор провайдера в сессии
            return Session?.GetString("DbProvider") ?? _configuration["Database:DefaultProvider"] ?? "Sqlite";
        }

        public void SetProvider(string provider)
        {
            Session?.SetString("DbProvider", provider);
        }

        public string GetConnectionString()
        {
            // Всегда читаем из appsettings.json
            var provider = GetProvider();
            return _configuration[$"Database:Providers:{provider}:ConnectionString"]
                   ?? "Data Source=crm.db;Mode=ReadWriteCreate;Cache=Shared;";
        }

        public void SetConnectionString(string connectionString)
        {
            // Ничего не делаем — строку всегда берём из конфига
            // Можно добавить лог, чтобы было понятно
            Console.WriteLine($"[DatabaseService] SetConnectionString вызван, но игнорируется. Строка всегда из appsettings.json");
        }
    }
}