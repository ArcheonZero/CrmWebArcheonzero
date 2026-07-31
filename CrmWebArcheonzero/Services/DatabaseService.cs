using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace CrmWebArcheonzero.Services
{
    public class DatabaseService : IDatabaseService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;

        // Конструктор теперь принимает IConfiguration
        public DatabaseService(IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
        }

        private ISession Session => _httpContextAccessor.HttpContext?.Session;

        public string GetProvider()
        {
            // Если в сессии есть выбранный провайдер — используем его
            var sessionProvider = Session?.GetString("DbProvider");
            if (!string.IsNullOrEmpty(sessionProvider))
                return sessionProvider;

            // Иначе берём из appsettings.json
            return _configuration["Database:DefaultProvider"] ?? "Sqlite";
        }

        public string GetConnectionString()
        {
            // Если в сессии есть строка подключения — используем её
            var sessionConn = Session?.GetString("DbConnectionString");
            if (!string.IsNullOrEmpty(sessionConn))
                return sessionConn;

            // Иначе берём из appsettings.json
            var provider = GetProvider();
            var connString = _configuration[$"Database:Providers:{provider}:ConnectionString"];
            return connString ?? "Data Source=crm.db;Mode=ReadWriteCreate;Cache=Shared;";
        }

        public void SetProvider(string provider)
        {
            Session?.SetString("DbProvider", provider);
        }

        public void SetConnectionString(string connectionString)
        {
            Session?.SetString("DbConnectionString", connectionString);
        }
    }
}