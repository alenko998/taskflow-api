using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.Common.Models;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Features.Workspaces.Commands;

public record AcceptInvitationCommand(string Token, string UserId) : IRequest<Result>;

public class AcceptInvitationCommandHandler : IRequestHandler<AcceptInvitationCommand, Result>
{
    private readonly IAppDbContext _context;
    private readonly IUnitOfWork   _unitOfWork;

    public AcceptInvitationCommandHandler(IAppDbContext context, IUnitOfWork unitOfWork)
    {
        _context    = context;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(AcceptInvitationCommand request, CancellationToken cancellationToken)
    {
        var invitation = await _context.Invitations
            .FirstOrDefaultAsync(i => i.Token == request.Token, cancellationToken);

        if (invitation == null)
            return Result.NotFound("Invitation not found.");

        if (invitation.IsExpired)
            return Result.Failure("Invitation has expired.");

        if (invitation.Status != Domain.Enums.InvitationStatus.Pending)
            return Result.Failure("Invitation is no longer valid.");

        invitation.Accept();

        var member = WorkspaceMember.Create(invitation.WorkspaceId, request.UserId, invitation.Role);
        _context.WorkspaceMembers.Add(member);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}