using MediatR;
using TaskFlow.Application.Common.Models;
using TaskFlow.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace TaskFlow.Application.Features.Auth.Commands;

public record ResetPasswordCommand(string UserId, string Token, string NewPassword) : IRequest<Result>;

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, Result>
{
    private readonly UserManager<User> _userManager;

    public ResetPasswordCommandHandler(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    public async Task<Result> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user == null)
            return Result.NotFound("User not found.");

        var result = await _userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
        if (!result.Succeeded)
            return Result.Failure(result.Errors.First().Description);

        return Result.Success();
    }
}