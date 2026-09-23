using TaskManager.Api.DTOs;
using TaskManager.Api.Models;
using TaskManager.Api.Repositories;

namespace TaskManager.Api.Services;

public class TaskService : ITaskService
{
    private readonly ITaskRepository _taskRepository;

    public TaskService(ITaskRepository taskRepository)
    {
        _taskRepository = taskRepository;
    }

    public async Task<List<TaskResponseDto>> GetAllAsync()
    {
        var tasks = await _taskRepository.GetAllAsync();
        return tasks.Select(ToDto).ToList();
    }

    public async Task<TaskResponseDto?> GetByIdAsync(int id)
    {
        var task = await _taskRepository.GetByIdAsync(id);
        return task is null ? null : ToDto(task);
    }

    public async Task<TaskResponseDto> CreateAsync(TaskCreateDto dto)
    {
        var task = new TaskItem
        {
            Title = dto.Title,
            Description = dto.Description,
            Priority = dto.Priority,
            DueDate = dto.DueDate,
            Status = Models.TaskStatus.Todo
        };

        var created = await _taskRepository.CreateAsync(task);
        return ToDto(created);
    }

    public async Task<bool> UpdateAsync(int id, TaskUpdateDto dto)
    {
        var task = new TaskItem
        {
            Id = id,
            Title = dto.Title,
            Description = dto.Description,
            Priority = dto.Priority,
            DueDate = dto.DueDate
        };

        return await _taskRepository.UpdateAsync(task);
    }

    public async Task<bool> ToggleStatusAsync(int id)
    {
        var existing = await _taskRepository.GetByIdAsync(id);
        if (existing is null) return false;

        var newStatus = existing.Status == Models.TaskStatus.Todo
            ? Models.TaskStatus.Done
            : Models.TaskStatus.Todo;

        return await _taskRepository.UpdateStatusAsync(id, newStatus);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        return await _taskRepository.DeleteAsync(id);
    }

    private static TaskResponseDto ToDto(TaskItem task) => new()
    {
        Id = task.Id,
        Title = task.Title,
        Description = task.Description,
        Priority = task.Priority,
        DueDate = task.DueDate,
        Status = task.Status,
        CreatedAt = task.CreatedAt,
        UpdatedAt = task.UpdatedAt
    };
}