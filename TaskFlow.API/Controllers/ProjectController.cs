using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.API.Extensions;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.Features.Projects.Commands;
using TaskFlow.Application.Features.Projects.Queries;

namespace TaskFlow.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProjectController : ControllerBase
{
    private readonly IMediator           _mediator;
    private readonly ICurrentUserService _currentUser;

    public ProjectController(IMediator mediator, ICurrentUserService currentUser)
    {
        _mediator    = mediator;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _mediator.Send(new GetProjectsQuery(_currentUser.WorkspaceId!, _currentUser.UserId!));
        return result.ToActionResult();
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(string id)
    {
        var result = await _mediator.Send(new GetProjectQuery(id, _currentUser.UserId!));
        return result.ToActionResult();
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProjectCommand command)
    {
        var result = await _mediator.Send(command with
        {
            WorkspaceId = _currentUser.WorkspaceId!,
            CreatedById = _currentUser.UserId!
        });
        return result.ToActionResult();
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateProjectCommand command)
    {
        var result = await _mediator.Send(command with { ProjectId = id, UserId = _currentUser.UserId! });
        return result.ToActionResult();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var result = await _mediator.Send(new DeleteProjectCommand(id, _currentUser.UserId!));
        return result.ToActionResult();
    }

    [HttpPost("{id}/members")]
    public async Task<IActionResult> AddMember(string id, [FromBody] AddMemberDto dto)
    {
        var result = await _mediator.Send(new AddProjectMemberCommand(id, dto.UserId, _currentUser.UserId!));
        return result.ToActionResult();
    }

    [HttpDelete("{id}/members/{userId}")]
    public async Task<IActionResult> RemoveMember(string id, string userId)
    {
        var result = await _mediator.Send(new RemoveProjectMemberCommand(id, userId, _currentUser.UserId!));
        return result.ToActionResult();
    }
}

public record AddMemberDto(string UserId);