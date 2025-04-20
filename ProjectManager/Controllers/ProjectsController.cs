using MediatR;
using Microsoft.AspNetCore.Mvc;
using ProjectManager.Features.Projects.Commands;
using ProjectManager.Features.Projects.Queries;
using Microsoft.AspNetCore.Authorization;

namespace ProjectManager.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProjectsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProjectsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProjectCommand commandHandler)
    {
        var id = await _mediator.Send(commandHandler);
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
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProjectCommand commandHandler)
    {
        if (id != commandHandler.Id)
            return BadRequest("ID in body and URL do not match.");

        var updated = await _mediator.Send(commandHandler);
        return updated ? NoContent() : NotFound();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _mediator.Send(new DeleteProjectCommand(id));
        return deleted ? NoContent() : NotFound();
    }
}
