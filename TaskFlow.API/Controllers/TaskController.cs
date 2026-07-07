using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.API.Extensions;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.Features.Tasks.Commands;
using TaskFlow.Application.Features.Tasks.Queries;

namespace TaskFlow.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TaskController : ControllerBase
{
    private readonly IMediator           _mediator;
    private readonly ICurrentUserService _currentUser;

    public TaskController(IMediator mediator, ICurrentUserService currentUser)
    {
        _mediator    = mediator;
        _currentUser = currentUser;
    }

    [HttpGet("my")]
    public async Task<IActionResult> GetMyTasks()
    {
        var result = await _mediator.Send(new GetMyTasksQuery(_currentUser.UserId!, _currentUser.WorkspaceId!));
        return result.ToActionResult();
    }

    [HttpGet("project/{projectId}")]
    public async Task<IActionResult> GetByProject(string projectId)
    {
        var result = await _mediator.Send(new GetTasksQuery(projectId, _currentUser.UserId!));
        return result.ToActionResult();
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(string id)
    {
        var result = await _mediator.Send(new GetTaskQuery(id, _currentUser.UserId!));
        return result.ToActionResult();
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTaskCommand command)
    {
        var result = await _mediator.Send(command with { CreatedById = _currentUser.UserId! });
        return result.ToActionResult();
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateTaskCommand command)
    {
        var result = await _mediator.Send(command with { TaskId = id, UserId = _currentUser.UserId! });
        return result.ToActionResult();
    }

    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateStatus(string id, [FromBody] UpdateStatusDto dto)
    {
        var result = await _mediator.Send(new UpdateTaskStatusCommand(id, dto.Status, _currentUser.UserId!));
        return result.ToActionResult();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var result = await _mediator.Send(new DeleteTaskCommand(id, _currentUser.UserId!));
        return result.ToActionResult();
    }
}

public record UpdateStatusDto(TaskFlow.Domain.Enums.TaskStatus Status);