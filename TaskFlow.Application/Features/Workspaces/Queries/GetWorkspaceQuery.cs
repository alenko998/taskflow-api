using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.Common.Models;

namespace TaskFlow.Application.Features.Workspaces.Queries;

public record GetWorkspaceQuery(string WorkspaceId) : IRequest<Result<WorkspaceResponse>>;

public record WorkspaceResponse(
    Guid   Id,
    string Name,
    string Slug,
    string OwnerId,
    int    MemberCount,
    int    ProjectCount,
    DateTime CreatedAt
);

public class GetWorkspaceQueryHandler : IRequestHandler<GetWorkspaceQuery, Result<WorkspaceResponse>>
{
    private readonly IAppDbContext _context;

    public GetWorkspaceQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<Result<WorkspaceResponse>> Handle(GetWorkspaceQuery request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.WorkspaceId, out var workspaceId))
            return Result<WorkspaceResponse>.Failure("Invalid workspace ID.");

        var workspace = await _context.Workspaces
            .Include(w => w.Members)
            .Include(w => w.Projects)
            .FirstOrDefaultAsync(w => w.Id == workspaceId, cancellationToken);

        if (workspace == null)
            return Result<WorkspaceResponse>.NotFound("Workspace not found.");

        return Result<WorkspaceResponse>.Success(new WorkspaceResponse(
            Id:           workspace.Id,
            Name:         workspace.Name,
            Slug:         workspace.Slug,
            OwnerId:      workspace.OwnerId,
            MemberCount:  workspace.Members.Count,
            ProjectCount: workspace.Projects.Count,
            CreatedAt:    workspace.CreatedAt
        ));
    }
}