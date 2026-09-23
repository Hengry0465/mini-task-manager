using TaskManager.Api.Models;

namespace TaskManager.Api.Repositories;

public interface ITaskRepository
{
    Task<List<TaskItem>> GetAllAsync();
    Task<TaskItem?> GetByIdAsync(int id);
    Task<TaskItem> CreateAsync(TaskItem task);
    Task<bool> UpdateAsync(TaskItem task);
    Task<bool> UpdateStatusAsync(int id, Models.TaskStatus status);
    Task<bool> DeleteAsync(int id);
}