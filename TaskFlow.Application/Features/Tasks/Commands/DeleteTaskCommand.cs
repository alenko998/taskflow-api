using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.Common.Models;

namespace TaskFlow.Application.Features.Tasks.Commands;

public record DeleteTaskCommand(string TaskId, string UserId) : IRequest<Result>;

public class DeleteTaskCommandHandler : IRequestHandler<DeleteTaskCommand, Result>
{
    private readonly IAppDbContext _context;
    private readonly IUnitOfWork   _unitOfWork;

    public DeleteTaskCommandHandler(IAppDbContext context, IUnitOfWork unitOfWork)
    {
        _context    = context;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeleteTaskCommand request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.TaskId, out var taskId))
            return Result.Failure("Invalid task ID.");

        var task = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == taskId, cancellationToken);
        if (task == null)
            return Result.NotFound("Task not found.");

        if (task.CreatedById != request.UserId)
            return Result.Forbidden("Only the task creator can delete it.");

        _context.Tasks.Remove(task);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}