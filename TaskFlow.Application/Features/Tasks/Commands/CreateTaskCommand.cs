using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.Common.Models;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;

namespace TaskFlow.Application.Features.Tasks.Commands;

public record CreateTaskCommand(
    string       Title,
    string       Description,
    TaskPriority Priority,
    DateTime?    DueDate,
    string       ProjectId,
    string       CreatedById,
    string?      AssigneeId
) : IRequest<Result<Guid>>;

public class CreateTaskCommandHandler : IRequestHandler<CreateTaskCommand, Result<Guid>>
{
    private readonly IAppDbContext _context;
    private readonly IUnitOfWork   _unitOfWork;
    private readonly IEmailService _emailService;

    public CreateTaskCommandHandler(IAppDbContext context, IUnitOfWork unitOfWork, IEmailService emailService)
    {
        _context      = context;
        _unitOfWork   = unitOfWork;
        _emailService = emailService;
    }

    public async Task<Result<Guid>> Handle(CreateTaskCommand request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.ProjectId, out var projectId))
            return Result<Guid>.Failure("Invalid project ID.");

        var project = await _context.Projects.FindAsync(new object[] { projectId }, cancellationToken);
        if (project == null)
            return Result<Guid>.NotFound("Project not found.");

        var task = TaskItem.Create(
            request.Title,
            request.Description,
            request.Priority,
            request.DueDate,
            projectId,
            request.CreatedById,
            request.AssigneeId
        );

        _context.Tasks.Add(task);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (request.AssigneeId != null && request.AssigneeId != request.CreatedById)
        {
            var assignee = await _context.Users.FindAsync(new object[] { request.AssigneeId }, cancellationToken);
            if (assignee != null)
                await _emailService.SendTaskAssignedAsync(assignee.Email!, assignee.FirstName, request.Title, project.Name);
        }

        return Result<Guid>.Created(task.Id);
    }
}