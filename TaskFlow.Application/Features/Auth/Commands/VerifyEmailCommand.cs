using MediatR;
using TaskFlow.Application.Common.Models;
using TaskFlow.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace TaskFlow.Application.Features.Auth.Commands;

public record VerifyEmailCommand(string UserId, string Token) : IRequest<Result>;

public class VerifyEmailCommandHandler : IRequestHandler<VerifyEmailCommand, Result>
{
    private readonly UserManager<User> _userManager;

    public VerifyEmailCommandHandler(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    public async Task<Result> Handle(VerifyEmailCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user == null)
            return Result.NotFound("User not found.");

        var result = await _userManager.ConfirmEmailAsync(user, request.Token);
        if (!result.Succeeded)
            return Result.Failure("Invalid or expired verification link.");

        user.Verify();
        await _userManager.UpdateAsync(user);

        return Result.Success();
    }
}