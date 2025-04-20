using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectManager.Features.ProjectMembers.Commands;
using ProjectManager.Features.ProjectMembers.Queries;
using ProjectManager.Services.Interfaces;
using System.Security.Claims;

namespace ProjectManager.Controllers;

[ApiController]
[Route("api/Projects/{projectId:guid}/ProjectMembers")]
[Authorize]
public class ProjectMembersController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProjectMembersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    private Guid GetUserIdFromClaims()
    {
        var userIdClaim = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null)
            throw new UnauthorizedAccessException("User ID not found in claims.");

        return Guid.Parse(userIdClaim); 
    }

    [HttpPost("add/{userId:guid}")]
    public async Task<IActionResult> AddToProject(Guid projectId, Guid userId)
    {
        var requestingUserId = GetUserIdFromClaims();
        var result = await _mediator.Send(new AddProjectMemberToProjectCommand(projectId, userId, requestingUserId));
        return result ? Ok() : BadRequest("Failed to add member to project.");
    }

    [HttpDelete("remove/{userId:guid}")]
    public async Task<IActionResult> RemoveFromProject(Guid projectId, Guid userId)
    {
        var requestingUserId = GetUserIdFromClaims();
        var result = await _mediator.Send(new RemoveProjectMemberFromProjectCommand(projectId, userId, requestingUserId));
        return result ? Ok() : BadRequest("Failed to remove member from project.");
    }

    [HttpPut("change-role/{userId:guid}")]
    public async Task<IActionResult> ChangeRole(Guid projectId, Guid userId, [FromBody] ChangeProjectMemberRoleCommand command)
    {
        if (userId != command.RequestingUserId)
            return BadRequest("User ID mismatch.");

        var result = await _mediator.Send(command);

        if (result == null)
            return BadRequest("Failed to change role.");

        return Ok(result);
    }
    

    [HttpGet("{userId:guid}")]
    public async Task<IActionResult> GetById(Guid projectId, Guid userId)
    {
        var member = await _mediator.Send(new GetProjectMemberByUserIdQuery(projectId, userId));

        return member is null ? NotFound() : Ok(member);
    }

    [HttpGet]
    public async Task<IActionResult> GetByProjectId(Guid projectId)
    {
        var members = await _mediator.Send(new GetProjectMembersQuery(projectId));
        return Ok(members);
    }
}
