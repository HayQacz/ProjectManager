using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectManager.Features.ProjectMembers.Commands;
using ProjectManager.Features.ProjectMembers.Queries;

namespace ProjectManager.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProjectMembersController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProjectMembersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProjectMemberCommand command)
    {
        var member = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = member.Id }, member);
    }

    [HttpPost("add-to-project")]
    public async Task<IActionResult> AddToProject([FromBody] AddProjectMemberToProjectCommand command)
    {
        var result = await _mediator.Send(command);
        return result ? Ok() : BadRequest("Failed to add member to project.");
    }

    [HttpDelete("remove-from-project")]
    public async Task<IActionResult> RemoveFromProject([FromBody] RemoveProjectMemberFromProjectCommand command)
    {
        var result = await _mediator.Send(command);
        return result ? Ok() : BadRequest("Failed to remove member from project.");
    }

    [HttpPut("change-role")]
    public async Task<IActionResult> ChangeRole([FromBody] ChangeProjectMemberRoleCommand command)
    {
        var result = await _mediator.Send(command);

        if (result is null)
            return BadRequest("Failed to change role.");

        return Ok(result);
    }


    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var member = await _mediator.Send(new GetProjectMemberByIdQuery(id));
        return member is null ? NotFound() : Ok(member);
    }

    [HttpGet("project/{projectId:guid}")]
    public async Task<IActionResult> GetByProjectId(Guid projectId)
    {
        var members = await _mediator.Send(new GetProjectMembersQuery(projectId));
        return Ok(members);
    }
}
