using CrmWebArcheonzero.Data;
using CrmWebArcheonzero.Interfaces;
using CrmWebArcheonzero.Models;
using CrmWebArcheonzero.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// === Настройка сессий ===
builder.Services.AddHttpContextAccessor();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// === Регистрация сервисов ===
builder.Services.AddScoped<IDatabaseService, DatabaseService>();

// Регистрируем контекст базы данных как Scoped (один экземпляр на запрос)
builder.Services.AddScoped<ApplicationDbContext>(provider =>
{
    var dbService = provider.GetRequiredService<IDatabaseService>();
    var providerName = dbService.GetProvider();
    var connectionString = dbService.GetConnectionString();

    Console.WriteLine($"[Program] Создаём контекст с провайдером: {providerName}");
    Console.WriteLine($"[Program] Строка подключения: {connectionString}");

    var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

    switch (providerName)
    {
        case "PostgreSQL":
            optionsBuilder.UseNpgsql(connectionString);
            break;
        case "SqlServer":
            optionsBuilder.UseSqlServer(connectionString);
            break;
        case "Sqlite":
        default:
            optionsBuilder.UseSqlite(connectionString);
            break;
    }
    Console.WriteLine($"[Program] ПРОВЕРКА: полный путь к БД: {Path.GetFullPath(connectionString.Replace("Data Source=", "").Split(';')[0])}");
    return new ApplicationDbContext(optionsBuilder.Options);
});

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IClientRepository, ClientRepository>();
builder.Services.AddScoped<IExportService, ExportService>();
builder.Services.AddScoped<ITaskRepository, TaskRepository>();
builder.Services.AddScoped<INoteRepository, NoteRepository>();
builder.Services.AddScoped<IInteractionRepository, InteractionRepository>();
builder.Services.AddScoped<IChatRepository, ChatRepository>();
builder.Services.AddScoped<IHistoryRepository, HistoryRepository>();
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddScoped<IEmailService, EmailService>();
// === Авторизация ===
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/Login";
    });
builder.Services.AddAuthorization();

// === Контроллеры и представления ===
builder.Services.AddControllersWithViews();

var app = builder.Build();

// === Создание базы при первом запуске ===
using (var scope = app.Services.CreateScope())
{
    var dbService = scope.ServiceProvider.GetRequiredService<IDatabaseService>();
    var providerName = dbService.GetProvider();
    var connectionString = dbService.GetConnectionString();

    Console.WriteLine($"[Program] Создаём базу с провайдером: {providerName}");
    Console.WriteLine($"[Program] Строка подключения: {connectionString}");

    var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

    switch (providerName)
    {
        case "PostgreSQL":
            optionsBuilder.UseNpgsql(connectionString);
            break;
        case "SqlServer":
            optionsBuilder.UseSqlServer(connectionString);
            break;
        case "Sqlite":
        default:
            optionsBuilder.UseSqlite(connectionString);
            break;
    }

    using var dbContext = new ApplicationDbContext(optionsBuilder.Options);
    dbContext.Database.EnsureCreated();
    dbContext.EnsureSeedData();
}

// === Настройка маршрутизации ===
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Clients}/{action=Index}/{id?}");

app.Run();