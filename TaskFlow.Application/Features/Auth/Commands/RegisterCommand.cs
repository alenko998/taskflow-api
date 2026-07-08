using MediatR;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.Common.Models;
using TaskFlow.Domain.Entities;
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
    private readonly IEmailService     _emailService;

    public RegisterCommandHandler(UserManager<User> userManager, IEmailService emailService)
    {
        _userManager  = userManager;
        _emailService = emailService;
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

        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        await _emailService.SendEmailVerificationAsync(user.Email!, user.FirstName, user.Id, token);

        return Result<string>.Success("Registration successful. Please verify your email.");
    }
}