using CrmWebArcheonzero.Data;
using CrmWebArcheonzero.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<AuthService>();
// 1. Добавляем контекст базы данных
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Регистрируем репозиторий
builder.Services.AddScoped<IClientRepository, ClientRepository>();

// 0. Добавляем Авторизацию
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/Login";
    });
// авторизация
builder.Services.AddAuthorization();
// 3. Добавляем контроллеры и представления
builder.Services.AddControllersWithViews();

var app = builder.Build();

// 4. Создаём базу и добавляем тестовые данные при первом запуске
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.EnsureCreated(); // создаёт БД, если её нет
    dbContext.EnsureSeedData(); // добавляет тестовых клиентов и пользователей
}

// 5. Настройка маршрутизации
app.UseRouting();
app.UseAuthentication(); 
app.UseAuthorization();    
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Clients}/{action=Index}/{id?}");

app.Run();