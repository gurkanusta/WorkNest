using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WorkNest.Domain.Entities;
using WorkNest.Infrastructure.Persistence;

namespace WorkNest.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProjectsController : ControllerBase
{
    private readonly AppDbContext _db;
    public ProjectsController(AppDbContext db) => _db = db;

    public record CreateProjectRequest(string Name);

    [HttpPost]
    public async Task<IActionResult> Create(CreateProjectRequest req)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var project = new Project
        {
            Name = req.Name,
            OwnerId = userId
        };

        _db.Projects.Add(project);

        _db.ProjectMembers.Add(new ProjectMember
        {
            ProjectId = project.Id,
            UserId = userId,
            Role = ProjectRole.Owner
        });

        await _db.SaveChangesAsync();
        return Ok(project);
    }


    [HttpGet]
    public async Task<IActionResult> MyProjects()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var list = await _db.ProjectMembers
            .Where(pm => pm.UserId == userId)
            .Select(pm => new
            {
                pm.Project.Id,
                pm.Project.Name,
                Role = pm.Role.ToString(),
                pm.Project.CreatedAtUtc
            })
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync();

        return Ok(list);
    }


    public record InviteRequest(string Email);

    [HttpPost("{projectId:guid}/invite")]
    public async Task<IActionResult> Invite(Guid projectId, InviteRequest req)
    {
        var callerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var callerRole = await _db.ProjectMembers
            .Where(x => x.ProjectId == projectId && x.UserId == callerId)
            .Select(x => x.Role)
            .FirstOrDefaultAsync();

        if (callerRole != ProjectRole.Owner)
            return Forbid("Only project owner can invite members.");

        
        var targetUser = await _db.Users.FirstOrDefaultAsync(u => u.Email == req.Email);
        if (targetUser == null)
            return NotFound("User not found with this email.");

        var exists = await _db.ProjectMembers
            .AnyAsync(x => x.ProjectId == projectId && x.UserId == targetUser.Id);

        if (exists)
            return BadRequest("User is already a member of this project.");

        _db.ProjectMembers.Add(new ProjectMember
        {
            ProjectId = projectId,
            UserId = targetUser.Id,
            Role = ProjectRole.Member
        });

        await _db.SaveChangesAsync();
        return Ok("Invited");
    }
}



