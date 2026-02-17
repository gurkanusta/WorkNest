using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using WorkNest.Infrastructure.Persistence;

namespace WorkNest.Api.Authorization;

public sealed class ProjectMemberHandler
    : AuthorizationHandler<ProjectMemberRequirement>
{
    private readonly AppDbContext _db;

    public ProjectMemberHandler(AppDbContext db)
    {
        _db = db;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ProjectMemberRequirement requirement)
    {
        
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return;

        if (context.Resource is not HttpContext httpContext)
            return;

        if (!httpContext.Request.RouteValues.TryGetValue("projectId", out var projectIdObj))
            return;

        if (!Guid.TryParse(projectIdObj?.ToString(), out var projectId))
            return;

        var isMember = await _db.ProjectMembers
            .AsNoTracking()
            .AnyAsync(pm =>
                pm.ProjectId == projectId &&
                pm.UserId == userId
            );

        if (isMember)
            context.Succeed(requirement);
    }
}