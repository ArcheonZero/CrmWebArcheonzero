using CrmWebArcheonzero.Models;
using CrmWebArcheonzero.Data;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;

namespace CrmWebArcheonzero.Services
{
    public class AuthService
    {
        private readonly Func<ApplicationDbContext> _contextFactory;

        public AuthService(Func<ApplicationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<User?> GetUserByIdAsync(int userId)
        {
            if (userId <= 0)
                return null;

            using var context = _contextFactory();
            return await context.Users
                .FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task<User?> LoginAsync(string username, string password)
        {
            using var context = _contextFactory();
            var user = await context.Users
                .FirstOrDefaultAsync(u => u.Username == username && u.IsActive);

            if (user == null)
                return null;

            if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                return null;

            return user;
        }

        public async Task<bool> RegisterAsync(string username, string password, string email, string fullName, string role = "User")
        {
            using var context = _contextFactory();
            if (await context.Users.AnyAsync(u => u.Username == username))
                return false;

            var user = new User
            {
                Username = username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                Email = email,
                FullName = fullName,
                Role = role,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            using var context = _contextFactory();
            return await context.Users
                .OrderBy(u => u.Username)
                .ToListAsync();
        }

        public async Task ChangeRoleAsync(int userId, string newRole)
        {
            using var context = _contextFactory();
            var user = await context.Users.FindAsync(userId);
            if (user != null)
            {
                user.Role = newRole;
                await context.SaveChangesAsync();
            }
        }

        public async Task ToggleUserStatusAsync(int userId)
        {
            using var context = _contextFactory();
            var user = await context.Users.FindAsync(userId);
            if (user != null)
            {
                user.IsActive = !user.IsActive;
                await context.SaveChangesAsync();
            }
        }
    }
}