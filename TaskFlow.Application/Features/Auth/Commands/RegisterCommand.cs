using MediatR;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.Common.Models;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace TaskFlow.Application.Features.Auth.Commands;

public record RegisterCommand(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    string ConfirmPassword
) : IRequest<Result<string>>;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<string>>
{
    private readonly UserManager<User> _userManager;
    private readonly IAppDbContext     _context;
    private readonly IEmailService     _emailService;
    private readonly IUnitOfWork       _unitOfWork;

    public RegisterCommandHandler(UserManager<User> userManager, IAppDbContext context, IEmailService emailService, IUnitOfWork unitOfWork)
    {
        _userManager  = userManager;
        _context      = context;
        _emailService = emailService;
        _unitOfWork   = unitOfWork;
    }

    public async Task<Result<string>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        if (request.Password != request.ConfirmPassword)
            return Result<string>.Failure("Passwords do not match.");

        var existing = await _userManager.FindByEmailAsync(request.Email);
        if (existing != null)
            return Result<string>.Conflict("Email is already in use.");

        var user = User.Create(request.FirstName, request.LastName, request.Email);

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            return Result<string>.Failure(result.Errors.First().Description);

        // Kreiraj workspace automatski
        var workspaceName = $"{request.FirstName}'s Workspace";
        var workspace     = Workspace.Create(workspaceName, user.Id);
        _context.Workspaces.Add(workspace);

        var member = WorkspaceMember.Create(workspace.Id, user.Id, WorkspaceRole.Owner);
        _context.WorkspaceMembers.Add(member);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        await _emailService.SendEmailVerificationAsync(user.Email!, user.FirstName, user.Id, token);

        return Result<string>.Success("Registration successful. Please verify your email.");
    }
}