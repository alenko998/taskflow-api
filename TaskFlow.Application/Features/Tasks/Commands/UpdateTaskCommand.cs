using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.Common.Models;
using TaskFlow.Domain.Enums;

namespace TaskFlow.Application.Features.Tasks.Commands;

public record UpdateTaskCommand(
    string       TaskId,
    string       Title,
    string       Description,
    TaskPriority Priority,
    DateTime?    DueDate,
    string?      AssigneeId,
    string       UserId
) : IRequest<Result>;

public class UpdateTaskCommandHandler : IRequestHandler<UpdateTaskCommand, Result>
{
    private readonly IAppDbContext _context;
    private readonly IUnitOfWork   _unitOfWork;

    public UpdateTaskCommandHandler(IAppDbContext context, IUnitOfWork unitOfWork)
    {
        _context    = context;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(UpdateTaskCommand request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.TaskId, out var taskId))
            return Result.Failure("Invalid task ID.");

        var task = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == taskId, cancellationToken);
        if (task == null)
            return Result.NotFound("Task not found.");

        task.Update(request.Title, request.Description, request.Priority, request.DueDate, request.AssigneeId);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}