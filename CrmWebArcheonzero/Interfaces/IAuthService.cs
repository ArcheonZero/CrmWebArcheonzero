using CrmWebArcheonzero.Models;

namespace CrmWebArcheonzero.Interfaces
{
    public interface IAuthService
    {
        Task<User?> GetUserByIdAsync(int userId);
        Task<User?> LoginAsync(string username, string password);
        Task<bool> RegisterAsync(string username, string password, string email, string fullName, string role = "User");
        Task<List<User>> GetAllUsersAsync();
        Task ChangeRoleAsync(int userId, string newRole);
        Task ToggleUserStatusAsync(int userId);
    }
}