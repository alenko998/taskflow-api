using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.Common.Models;
using TaskFlow.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace TaskFlow.Application.Features.Workspaces.Commands;

public record SwitchWorkspaceCommand(string UserId, string WorkspaceId) : IRequest<Result<string>>;

public class SwitchWorkspaceCommandHandler : IRequestHandler<SwitchWorkspaceCommand, Result<string>>
{
    private readonly IAppDbContext     _context;
    private readonly IJwtService       _jwtService;
    private readonly UserManager<User> _userManager;

    public SwitchWorkspaceCommandHandler(IAppDbContext context, IJwtService jwtService, UserManager<User> userManager)
    {
        _context     = context;
        _jwtService  = jwtService;
        _userManager = userManager;
    }

    public async Task<Result<string>> Handle(SwitchWorkspaceCommand request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.WorkspaceId, out var workspaceId))
            return Result<string>.Failure("Invalid workspace ID.");

        var membership = await _context.WorkspaceMembers
            .Include(m => m.Workspace)
            .FirstOrDefaultAsync(m => m.UserId == request.UserId && m.WorkspaceId == workspaceId, cancellationToken);

        if (membership == null)
            return Result<string>.Forbidden("You are not a member of this workspace.");

        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user == null)
            return Result<string>.NotFound("User not found.");

        var token = _jwtService.GenerateToken(user, membership.WorkspaceId.ToString(), membership.Role.ToString());

        return Result<string>.Success(token);
    }
}