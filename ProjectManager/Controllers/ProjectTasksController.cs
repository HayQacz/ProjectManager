﻿using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectManager.Features.ProjectTasks.Commands;
using ProjectManager.Features.ProjectTasks.Queries;
using ProjectManager.Entities.Enums;

namespace ProjectManager.Controllers;

[ApiController]
[Route("api/Projects/{projectId:guid}/ProjectTasks")]
[Authorize]
public class ProjectTasksController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProjectTasksController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetTasks(
        Guid projectId, 
        [FromQuery] ProjectTaskStatus? status, 
        [FromQuery] bool onlyAssignedToMe = false, 
        [FromQuery] bool onlyUnassigned = false)
    {
        try
        {
            var query = new GetProjectTasksQuery(projectId, status, onlyAssignedToMe, onlyUnassigned);
            var tasks = await _mediator.Send(query);
            return Ok(tasks);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (Exception ex)
        {
            return StatusCode(500, "An unexpected error occurred.");
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateTask(Guid projectId, [FromBody] CreateProjectTaskCommand commandHandler)
    {
        if (projectId != commandHandler.ProjectId)
            return BadRequest("Project ID mismatch.");

        var taskDto = await _mediator.Send(commandHandler);
        return CreatedAtAction(nameof(GetTasks), new { projectId = projectId }, taskDto);
    }

    [HttpPut("{taskId:guid}")]
    public async Task<IActionResult> UpdateTask(Guid projectId, Guid taskId, [FromBody] UpdateProjectTaskCommand commandHandler)
    {
        if (taskId != commandHandler.Id)
            return BadRequest("Mismatched task ID");

        var result = await _mediator.Send(commandHandler);
        return result ? NoContent() : Forbid();
    }

    [HttpDelete("{taskId:guid}")]
    public async Task<IActionResult> DeleteTask(Guid projectId, Guid taskId)
    {
        var result = await _mediator.Send(new DeleteProjectTaskCommand(taskId));
        return result ? NoContent() : Forbid();
    }
}
