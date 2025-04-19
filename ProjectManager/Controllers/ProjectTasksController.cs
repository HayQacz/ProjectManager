using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectManager.Features.ProjectTasks.Commands;
using ProjectManager.Features.ProjectTasks.Queries;
using ProjectManager.Entities.Enums;

namespace ProjectManager.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProjectTasksController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProjectTasksController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{projectId:guid}/tasks")]
    public async Task<IActionResult> GetTasks(Guid projectId, [FromQuery] string? status, [FromQuery] bool onlyMine = false, [FromQuery] bool unassigned = false)
    {
        ProjectTaskStatus? parsedStatus = null;

        if (!string.IsNullOrEmpty(status) && Enum.TryParse<ProjectTaskStatus>(status, true, out var result))
        {
            parsedStatus = result;
        }

        var query = new GetProjectTasksQuery(projectId, parsedStatus, onlyMine, unassigned);
        var tasks = await _mediator.Send(query);
        return Ok(tasks);
    }


    [HttpPut("{taskId:guid}")]
    public async Task<IActionResult> UpdateTask(Guid taskId, [FromBody] UpdateProjectTaskCommand command)
    {
        if (taskId != command.Id)
            return BadRequest("Mismatched task ID");

        var result = await _mediator.Send(command);
        return result ? NoContent() : Forbid();
    }

    [HttpDelete("{taskId:guid}")]
    public async Task<IActionResult> DeleteTask(Guid taskId)
    {
        var result = await _mediator.Send(new DeleteProjectTaskCommand(taskId));
        return result ? NoContent() : Forbid();
    }
}
