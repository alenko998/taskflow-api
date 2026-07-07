using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.Common.Models;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Features.Comments.Commands;

public record CreateCommentCommand(string TaskId, string Content, string AuthorId) : IRequest<Result<Guid>>;

public class CreateCommentCommandHandler : IRequestHandler<CreateCommentCommand, Result<Guid>>
{
    private readonly IAppDbContext _context;
    private readonly IUnitOfWork   _unitOfWork;
    private readonly IEmailService _emailService;

    public CreateCommentCommandHandler(IAppDbContext context, IUnitOfWork unitOfWork, IEmailService emailService)
    {
        _context      = context;
        _unitOfWork   = unitOfWork;
        _emailService = emailService;
    }

    public async Task<Result<Guid>> Handle(CreateCommentCommand request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.TaskId, out var taskId))
            return Result<Guid>.Failure("Invalid task ID.");

        var task = await _context.Tasks
            .Include(t => t.CreatedBy)
            .Include(t => t.Project)
            .FirstOrDefaultAsync(t => t.Id == taskId, cancellationToken);

        if (task == null)
            return Result<Guid>.NotFound("Task not found.");

        var comment = Comment.Create(request.Content, taskId, request.AuthorId);
        _context.Comments.Add(comment);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (task.CreatedById != request.AuthorId)
        {
            await _emailService.SendTaskStatusChangedAsync(
                task.CreatedBy.Email!, task.CreatedBy.FirstName,
                task.Title, "New comment added");
        }

        return Result<Guid>.Created(comment.Id);
    }
}