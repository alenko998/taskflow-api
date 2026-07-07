using MediatR;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.Common.Models;
using TaskFlow.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace TaskFlow.Application.Features.Auth.Commands;

public record LoginCommand(string Email, string Password) : IRequest<Result<AuthResponse>>;

public record AuthResponse(
    string Token,
    string UserId,
    string Email,
    string FirstName,
    string LastName,
    string WorkspaceId,
    string WorkspaceName,
    string Role
);

public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<AuthResponse>>
{
    private readonly UserManager<User> _userManager;
    private readonly IAppDbContext     _context;
    private readonly IJwtService       _jwtService;

    public LoginCommandHandler(UserManager<User> userManager, IAppDbContext context, IJwtService jwtService)
    {
        _userManager = userManager;
        _context     = context;
        _jwtService  = jwtService;
    }

    public async Task<Result<AuthResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
            return Result<AuthResponse>.Unauthorized("Invalid email or password.");

        var validPassword = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!validPassword)
            return Result<AuthResponse>.Unauthorized("Invalid email or password.");

        if (!user.IsVerified)
            return Result<AuthResponse>.Forbidden("Please verify your email before logging in.");

        var membership = await _context.WorkspaceMembers
            .Include(m => m.Workspace)
            .FirstOrDefaultAsync(m => m.UserId == user.Id, cancellationToken);

        if (membership == null)
            return Result<AuthResponse>.NotFound("Workspace not found.");

        var token = _jwtService.GenerateToken(user, membership.WorkspaceId.ToString(), membership.Role.ToString());

        return Result<AuthResponse>.Success(new AuthResponse(
            Token:         token,
            UserId:        user.Id,
            Email:         user.Email!,
            FirstName:     user.FirstName,
            LastName:      user.LastName,
            WorkspaceId:   membership.WorkspaceId.ToString(),
            WorkspaceName: membership.Workspace.Name,
            Role:          membership.Role.ToString()
        ));
    }
}