using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WorkNest.Domain.Entities;
using WorkNest.Infrastructure.Persistence;

namespace WorkNest.Api.Controllers;

[ApiController]
[Route("api/projects/{projectId:guid}/tasks")]
[Authorize]
public class TasksController : ControllerBase
{
    private readonly AppDbContext _db;
    public TasksController(AppDbContext db) => _db = db;

    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    private async Task<bool> IsMember(Guid projectId)
        => await _db.ProjectMembers.AnyAsync(x => x.ProjectId == projectId && x.UserId == UserId);

    public record CreateTaskRequest(string Title, string? Description, DateTime? DueDateUtc, string? AssignedUserId);

    [HttpPost]
    public async Task<IActionResult> Create(Guid projectId, CreateTaskRequest req)
    {
        if (!await IsMember(projectId)) return Forbid();

        var task = new TaskItem
        {
            ProjectId = projectId,
            Title = req.Title,
            Description = req.Description,
            DueDateUtc = req.DueDateUtc,
            AssignedUserId = req.AssignedUserId
        };

        _db.TaskItems.Add(task);
        await _db.SaveChangesAsync();
        return Ok(task);
    }

    [HttpGet]
    public async Task<IActionResult> List(
    Guid projectId,
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 10,
    [FromQuery] TaskItemStatus? status = null,
    [FromQuery] string? search = null,
    [FromQuery] string sort = "createdAtDesc"
)
    {
        if (!await IsMember(projectId)) return Forbid();

        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 10 : pageSize;
        pageSize = pageSize > 50 ? 50 : pageSize;

        var query = _db.TaskItems
            .AsNoTracking()
            .Where(t => t.ProjectId == projectId);

        if (status != null)
            query = query.Where(t => t.Status == status);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            query = query.Where(t =>
                t.Title.Contains(s) ||
                (t.Description != null && t.Description.Contains(s)));
        }

        query = sort switch
        {
            "createdAtAsc" => query.OrderBy(t => t.CreatedAtUtc),
            "dueDateAsc" => query.OrderBy(t => t.DueDateUtc),
            "dueDateDesc" => query.OrderByDescending(t => t.DueDateUtc),
            _ => query.OrderByDescending(t => t.CreatedAtUtc) 
        };

        var total = await query.CountAsync();

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new
            {
                t.Id,
                t.Title,
                t.Description,
                Status = t.Status.ToString(),
                t.AssignedUserId,
                t.DueDateUtc,
                t.CreatedAtUtc
            })
            .ToListAsync();

        return Ok(new
        {
            page,
            pageSize,
            total,
            totalPages = (int)Math.Ceiling(total / (double)pageSize),
            items
        });
    }




    public record UpdateTaskRequest(string Title, string? Description, TaskItemStatus Status, DateTime? DueDateUtc, string? AssignedUserId);

    [HttpPut("{taskId:guid}")]
    public async Task<IActionResult> Update(Guid projectId, Guid taskId, UpdateTaskRequest req)
    {
        if (!await IsMember(projectId)) return Forbid();

        var task = await _db.TaskItems.FirstOrDefaultAsync(t => t.Id == taskId && t.ProjectId == projectId);
        if (task == null) return NotFound();

        task.Title = req.Title;
        task.Description = req.Description;
        task.Status = req.Status;
        task.DueDateUtc = req.DueDateUtc;
        task.AssignedUserId = req.AssignedUserId;

        await _db.SaveChangesAsync();
        return Ok("Updated");
    }

    [HttpDelete("{taskId:guid}")]
    public async Task<IActionResult> Delete(Guid projectId, Guid taskId)
    {
        if (!await IsMember(projectId)) return Forbid();

        var task = await _db.TaskItems.FirstOrDefaultAsync(t => t.Id == taskId && t.ProjectId == projectId);
        if (task == null) return NotFound();

        _db.TaskItems.Remove(task);
        await _db.SaveChangesAsync();
        return Ok("Deleted");
    }
}
