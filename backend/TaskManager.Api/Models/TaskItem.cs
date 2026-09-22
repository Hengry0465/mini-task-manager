using System.ComponentModel.DataAnnotations;

namespace TaskManager.Api.Models;

public enum TaskPriority
{
    Low,
    Medium,
    High
}

public enum TaskStatus
{
    Todo,
    Done
}

public class TaskItem
{
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public TaskPriority Priority { get; set; } = TaskPriority.Medium;

    public DateOnly? DueDate { get; set; }

    public TaskStatus Status { get; set; } = TaskStatus.Todo;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}