using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.Common.Models;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;

namespace TaskFlow.Application.Features.Workspaces.Commands;

public record InviteMemberCommand(
    string        Email,
    WorkspaceRole Role,
    string        WorkspaceId = "",
    string        InvitedById = ""
) : IRequest<Result>;

public class InviteMemberCommandHandler : IRequestHandler<InviteMemberCommand, Result>
{
    private readonly IAppDbContext       _context;
    private readonly IEmailService       _emailService;
    private readonly IUnitOfWork         _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public InviteMemberCommandHandler(IAppDbContext context, IEmailService emailService, IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _context      = context;
        _emailService = emailService;
        _unitOfWork   = unitOfWork;
        _currentUser  = currentUser;
    }

    public async Task<Result> Handle(InviteMemberCommand request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.WorkspaceId, out var workspaceId))
            return Result.Failure("Invalid workspace ID.");

        var role = _currentUser.Role;
        if (role != "Owner" && role != "Admin")
            return Result.Forbidden("Only owners and admins can invite members.");

        var workspace = await _context.Workspaces
            .Include(w => w.Members)
            .FirstOrDefaultAsync(w => w.Id == workspaceId, cancellationToken);

        if (workspace == null)
            return Result.NotFound("Workspace not found.");

        var alreadyMember = await _context.WorkspaceMembers
            .Include(m => m.User)
            .AnyAsync(m => m.WorkspaceId == workspaceId && m.User.Email == request.Email, cancellationToken);

        if (alreadyMember)
            return Result.Conflict("User is already a member of this workspace.");

        var existingInvite = await _context.Invitations
            .AnyAsync(i => i.WorkspaceId == workspaceId && i.Email == request.Email &&
                           i.Status == InvitationStatus.Pending, cancellationToken);

        if (existingInvite)
            return Result.Conflict("An invitation has already been sent to this email.");

        var inviter = await _context.Users.FindAsync(new object[] { request.InvitedById }, cancellationToken);
        if (inviter == null)
            return Result.NotFound("Inviter not found.");

        var invitation = Invitation.Create(request.Email, workspaceId, request.InvitedById, request.Role);
        _context.Invitations.Add(invitation);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _emailService.SendWorkspaceInvitationAsync(
            request.Email, inviter.FullName, workspace.Name, invitation.Token);

        return Result.Success();
    }
}