using Microsoft.AspNetCore.Http;

namespace CrmWebArcheonzero.Services
{
    public class DatabaseService : IDatabaseService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public DatabaseService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        private ISession Session => _httpContextAccessor.HttpContext?.Session;

        public string GetProvider()
        {
            return Session?.GetString("DbProvider") ?? "Sqlite";
        }

        public string GetConnectionString()
        {
            return Session?.GetString("DbConnectionString") ?? "Data Source=crm.db;Mode=ReadWriteCreate;Cache=Shared;";
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