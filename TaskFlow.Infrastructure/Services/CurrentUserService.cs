using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using TaskFlow.Application.Common.Interfaces;

namespace TaskFlow.Infrastructure.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? UserId => _httpContextAccessor.HttpContext?.User
        .FindFirstValue(ClaimTypes.NameIdentifier);

    public string? WorkspaceId => _httpContextAccessor.HttpContext?.User
        .FindFirstValue("workspaceId");

    public string? Role => _httpContextAccessor.HttpContext?.User
        .FindFirstValue(ClaimTypes.Role);

    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User
        .Identity?.IsAuthenticated ?? false;
}