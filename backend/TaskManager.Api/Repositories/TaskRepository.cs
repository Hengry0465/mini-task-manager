using Microsoft.EntityFrameworkCore;
using TaskManager.Api.Data;
using TaskManager.Api.Models;

namespace TaskManager.Api.Repositories;

public class TaskRepository : ITaskRepository
{
    private readonly AppDbContext _context;

    public TaskRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<TaskItem>> GetAllAsync()
    {
        return await _context.Tasks.OrderByDescending(t => t.CreatedAt).ToListAsync();
    }

    public async Task<TaskItem?> GetByIdAsync(int id)
    {
        return await _context.Tasks.FindAsync(id);
    }

    public async Task<TaskItem> CreateAsync(TaskItem task)
    {
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();
        return task;
    }

    public async Task<bool> UpdateAsync(TaskItem task)
    {
        var existing = await _context.Tasks.FindAsync(task.Id);
        if (existing is null) return false;

        existing.Title = task.Title;
        existing.Description = task.Description;
        existing.Priority = task.Priority;
        existing.DueDate = task.DueDate;
        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateStatusAsync(int id, Models.TaskStatus status)
    {
        var existing = await _context.Tasks.FindAsync(id);
        if (existing is null) return false;

        existing.Status = status;
        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var existing = await _context.Tasks.FindAsync(id);
        if (existing is null) return false;

        _context.Tasks.Remove(existing);
        await _context.SaveChangesAsync();
        return true;
    }
}