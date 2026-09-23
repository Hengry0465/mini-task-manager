using TaskManager.Api.DTOs;

namespace TaskManager.Api.Services;

public interface ITaskService
{
    Task<List<TaskResponseDto>> GetAllAsync();
    Task<TaskResponseDto?> GetByIdAsync(int id);
    Task<TaskResponseDto> CreateAsync(TaskCreateDto dto);
    Task<bool> UpdateAsync(int id, TaskUpdateDto dto);
    Task<bool> ToggleStatusAsync(int id);
    Task<bool> DeleteAsync(int id);
}