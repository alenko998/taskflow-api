using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.API.Extensions;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.Features.Workspaces.Commands;
using TaskFlow.Application.Features.Workspaces.Queries;

namespace TaskFlow.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WorkspaceController : ControllerBase
{
    private readonly IMediator           _mediator;
    private readonly ICurrentUserService _currentUser;

    public WorkspaceController(IMediator mediator, ICurrentUserService currentUser)
    {
        _mediator    = mediator;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var result = await _mediator.Send(new GetWorkspaceQuery(_currentUser.WorkspaceId!));
        return result.ToActionResult();
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateWorkspaceDto dto)
    {
        var result = await _mediator.Send(new UpdateWorkspaceCommand(
            _currentUser.WorkspaceId!, dto.Name, _currentUser.UserId!));
        return result.ToActionResult();
    }

    [HttpGet("members")]
    public async Task<IActionResult> GetMembers()
    {
        var result = await _mediator.Send(new GetMembersQuery(_currentUser.WorkspaceId!));
        return result.ToActionResult();
    }

    [HttpPost("invite")]
    public async Task<IActionResult> Invite([FromBody] InviteMemberCommand command)
    {
        var result = await _mediator.Send(command with
        {
            WorkspaceId = _currentUser.WorkspaceId!,
            InvitedById = _currentUser.UserId!
        });
        return result.ToActionResult();
    }

    [HttpPost("accept-invitation")]
    public async Task<IActionResult> AcceptInvitation([FromBody] AcceptInvitationDto dto)
    {
        var result = await _mediator.Send(new AcceptInvitationCommand(dto.Token, _currentUser.UserId!));
        return result.ToActionResult();
    }

    [HttpPut("members/{userId}/role")]
    public async Task<IActionResult> UpdateMemberRole(string userId, [FromBody] UpdateMemberRoleDto dto)
    {
        var result = await _mediator.Send(new UpdateMemberRoleCommand(
            _currentUser.WorkspaceId!, userId, dto.Role, _currentUser.UserId!));
        return result.ToActionResult();
    }

    [HttpDelete("members/{userId}")]
    public async Task<IActionResult> RemoveMember(string userId)
    {
        var result = await _mediator.Send(new RemoveMemberCommand(
            _currentUser.WorkspaceId!, userId, _currentUser.UserId!));
        return result.ToActionResult();
    }

    [HttpGet("my")]
    public async Task<IActionResult> GetMyWorkspaces()
    {
        var result = await _mediator.Send(new GetUserWorkspacesQuery(_currentUser.UserId!));
        return result.ToActionResult();
    }

    [HttpPost("switch")]
    public async Task<IActionResult> Switch([FromBody] SwitchWorkspaceDto dto)
    {
        var result = await _mediator.Send(new SwitchWorkspaceCommand(_currentUser.UserId!, dto.WorkspaceId));
        return result.ToActionResult();
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateWorkspaceCommand command)
    {
        var result = await _mediator.Send(command with { UserId = _currentUser.UserId! });
        return result.ToActionResult();
    }
}

public record UpdateWorkspaceDto(string Name);

public record SwitchWorkspaceDto(string WorkspaceId);
public record AcceptInvitationDto(string Token);
public record UpdateMemberRoleDto(TaskFlow.Domain.Enums.WorkspaceRole Role);