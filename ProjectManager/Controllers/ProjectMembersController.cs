using MediatR;
using Microsoft.AspNetCore.Mvc;
using ProjectManager.Features.ProjectMembers.Commands;
using ProjectManager.Features.ProjectMembers.Queries;

namespace ProjectManager.Controllers;

[ApiController]
[Route("api/[controller]")]
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

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var member = await _mediator.Send(new GetProjectMemberByIdQuery(id));
        return member is null ? NotFound() : Ok(member);
    }
}
