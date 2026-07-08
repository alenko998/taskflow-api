using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.Common.Models;

namespace TaskFlow.Application.Features.Comments.Commands;

public record DeleteCommentCommand(string CommentId, string UserId) : IRequest<Result>;

public class DeleteCommentCommandHandler : IRequestHandler<DeleteCommentCommand, Result>
{
    private readonly IAppDbContext       _context;
    private readonly IUnitOfWork         _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public DeleteCommentCommandHandler(IAppDbContext context, IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _context     = context;
        _unitOfWork  = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(DeleteCommentCommand request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.CommentId, out var commentId))
            return Result.Failure("Invalid comment ID.");

        var comment = await _context.Comments.FirstOrDefaultAsync(c => c.Id == commentId, cancellationToken);
        if (comment == null)
            return Result.NotFound("Comment not found.");

        var role   = _currentUser.Role;
        var userId = _currentUser.UserId;

        if (role != "Owner" && role != "Admin" && comment.AuthorId != userId)
            return Result.Forbidden("You can only delete your own comments.");

        _context.Comments.Remove(comment);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}