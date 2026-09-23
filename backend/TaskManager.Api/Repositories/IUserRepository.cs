using TaskManager.Api.Models;

namespace TaskManager.Api.Repositories;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email);
}