using BCrypt.Net;
using CrmWebArcheonzero.Data;
using CrmWebArcheonzero.Interfaces;
using CrmWebArcheonzero.Models;
using Microsoft.EntityFrameworkCore;

namespace CrmWebArcheonzero.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;

        public AuthService(ApplicationDbContext context)
        {
            _context = context;
        }

        public virtual async Task<User?> GetUserByIdAsync(int userId)
        {
            if (userId <= 0)
                return null;

            return await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId);
        }

        public virtual async Task<User?> LoginAsync(string username, string password)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username && u.IsActive);

            if (user == null)
                return null;

            if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                return null;

            return user;
        }

        public virtual async Task<bool> RegisterAsync(string username, string password, string email, string fullName, string role = "User")
        {
            if (await _context.Users.AnyAsync(u => u.Username == username))
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

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return true;
        }

        public virtual async Task<List<User>> GetAllUsersAsync()
        {
            var users = await _context.Users
                .OrderBy(u => u.Username)
                .ToListAsync();

            Console.WriteLine($"[GetAllUsersAsync] Загружено пользователей: {users.Count}");
            foreach (var u in users)
            {
                Console.WriteLine($"  - {u.Id}: {u.Username} ({u.Role})");
            }

            return users;
        }

        public virtual async Task ChangeRoleAsync(int userId, string newRole)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                user.Role = newRole;
                await _context.SaveChangesAsync();
            }
        }
        public async Task UpdateUserAsync(int userId, string username, string fullName, string email)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                user.Username = username;
                user.FullName = fullName;
                user.Email = email;
                await _context.SaveChangesAsync();
            }
        }
        public virtual async Task ToggleUserStatusAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                user.IsActive = !user.IsActive;
                await _context.SaveChangesAsync();
            }
        }
        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username);
        }
    }
}