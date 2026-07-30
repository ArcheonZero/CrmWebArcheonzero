using CrmWebArcheonzero.Data;
using CrmWebArcheonzero.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
// 0. Регистрируем сервис управления БД
builder.Services.AddScoped<IDatabaseService, DatabaseService>();

// 1. Регистрируем фабрику контекста
builder.Services.AddScoped<Func<ApplicationDbContext>>(provider => () =>
{
    var dbService = provider.GetRequiredService<IDatabaseService>();
    var providerName = dbService.GetProvider();
    var connectionString = dbService.GetConnectionString();
    Console.WriteLine($"Создаём контекст с провайдером: {providerName}");
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

    return new ApplicationDbContext(optionsBuilder.Options);
});

// 2. Регистрируем остальные сервисы
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<IClientRepository, ClientRepository>();
builder.Services.AddScoped<ExportService>();


// 3. Добавляем Авторизацию
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/Login";
    });
builder.Services.AddAuthorization();

// 4. Добавляем контроллеры и представления
builder.Services.AddControllersWithViews();

var app = builder.Build();


// 5. Настройка маршрутизации
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Clients}/{action=Index}/{id?}");

app.Run();