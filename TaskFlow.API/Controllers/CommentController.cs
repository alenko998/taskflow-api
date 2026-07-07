using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.API.Extensions;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.Features.Comments.Commands;

namespace TaskFlow.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CommentController : ControllerBase
{
    private readonly IMediator           _mediator;
    private readonly ICurrentUserService _currentUser;

    public CommentController(IMediator mediator, ICurrentUserService currentUser)
    {
        _mediator    = mediator;
        _currentUser = currentUser;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCommentDto dto)
    {
        var result = await _mediator.Send(new CreateCommentCommand(dto.TaskId, dto.Content, _currentUser.UserId!));
        return result.ToActionResult();
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateCommentDto dto)
    {
        var result = await _mediator.Send(new UpdateCommentCommand(id, dto.Content, _currentUser.UserId!));
        return result.ToActionResult();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var result = await _mediator.Send(new DeleteCommentCommand(id, _currentUser.UserId!));
        return result.ToActionResult();
    }
}

public record CreateCommentDto(string TaskId, string Content);
public record UpdateCommentDto(string Content);