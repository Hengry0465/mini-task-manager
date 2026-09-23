using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Api.DTOs;
using TaskManager.Api.Services;

namespace TaskManager.Api.Controllers;

[ApiController]
[Route("api/tasks")]
[Authorize]
public class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;

    public TasksController(ITaskService taskService)
    {
        _taskService = taskService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var tasks = await _taskService.GetAllAsync();
        return Ok(tasks);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var task = await _taskService.GetByIdAsync(id);
        if (task is null) return NotFound(new { message = $"Task {id} not found." });
        return Ok(task);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] TaskCreateDto dto)
    {
        var created = await _taskService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] TaskUpdateDto dto)
    {
        var updated = await _taskService.UpdateAsync(id, dto);
        if (!updated) return NotFound(new { message = $"Task {id} not found." });
        return NoContent();
    }

    [HttpPatch("{id}/status")]
    public async Task<IActionResult> ToggleStatus(int id)
    {
        var updated = await _taskService.ToggleStatusAsync(id);
        if (!updated) return NotFound(new { message = $"Task {id} not found." });
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _taskService.DeleteAsync(id);
        if (!deleted) return NotFound(new { message = $"Task {id} not found." });
        return NoContent();
    }
}