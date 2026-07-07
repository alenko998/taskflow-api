using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.Common.Models;
using TaskFlow.Domain.Enums;
using TaskStatus = TaskFlow.Domain.Enums.TaskStatus;

namespace TaskFlow.Application.Features.Tasks.Commands;

public record UpdateTaskStatusCommand(string TaskId, TaskStatus Status, string UserId) : IRequest<Result>;

public class UpdateTaskStatusCommandHandler : IRequestHandler<UpdateTaskStatusCommand, Result>
{
    private readonly IAppDbContext _context;
    private readonly IUnitOfWork   _unitOfWork;
    private readonly IEmailService _emailService;

    public UpdateTaskStatusCommandHandler(IAppDbContext context, IUnitOfWork unitOfWork, IEmailService emailService)
    {
        _context      = context;
        _unitOfWork   = unitOfWork;
        _emailService = emailService;
    }

    public async Task<Result> Handle(UpdateTaskStatusCommand request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.TaskId, out var taskId))
            return Result.Failure("Invalid task ID.");

        var task = await _context.Tasks
            .Include(t => t.CreatedBy)
            .FirstOrDefaultAsync(t => t.Id == taskId, cancellationToken);

        if (task == null)
            return Result.NotFound("Task not found.");

        task.UpdateStatus(request.Status);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (task.CreatedById != request.UserId)
        {
            await _emailService.SendTaskStatusChangedAsync(
                task.CreatedBy.Email!, task.CreatedBy.FirstName,
                task.Title, request.Status.ToString());
        }

        return Result.Success();
    }
}