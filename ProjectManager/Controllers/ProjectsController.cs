using MediatR;
using Microsoft.AspNetCore.Mvc;
using ProjectManager.Features.Projects.Commands;
using ProjectManager.Features.Projects.Queries;

namespace ProjectManager.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProjectsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProjectsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProjectCommand command)
    {
        var id = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id }, null);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var project = await _mediator.Send(new GetProjectByIdQuery(id));
        return project is null ? NotFound() : Ok(project);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var projects = await _mediator.Send(new GetAllProjectsQuery());
        return Ok(projects);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProjectCommand command)
    {
        if (id != command.Id)
            return BadRequest("ID in body and URL do not match.");

        var updated = await _mediator.Send(command);
        return updated ? NoContent() : NotFound();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _mediator.Send(new DeleteProjectCommand(id));
        return deleted ? NoContent() : NotFound();
    }
}
