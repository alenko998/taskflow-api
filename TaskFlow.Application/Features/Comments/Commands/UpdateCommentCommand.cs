using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.Common.Models;

namespace TaskFlow.Application.Features.Comments.Commands;

public record UpdateCommentCommand(string CommentId, string Content, string UserId) : IRequest<Result>;

public class UpdateCommentCommandHandler : IRequestHandler<UpdateCommentCommand, Result>
{
    private readonly IAppDbContext       _context;
    private readonly IUnitOfWork         _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public UpdateCommentCommandHandler(IAppDbContext context, IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _context     = context;
        _unitOfWork  = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(UpdateCommentCommand request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.CommentId, out var commentId))
            return Result.Failure("Invalid comment ID.");

        var comment = await _context.Comments.FirstOrDefaultAsync(c => c.Id == commentId, cancellationToken);
        if (comment == null)
            return Result.NotFound("Comment not found.");

        if (comment.AuthorId != _currentUser.UserId)
            return Result.Forbidden("You can only edit your own comments.");

        comment.Update(request.Content);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}