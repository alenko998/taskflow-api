using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.Common.Models;

namespace TaskFlow.API.Extensions;

public static class ResultExtensions
{
    public static IActionResult ToActionResult<T>(this Result<T> result)
    {
        return result.StatusCode switch
        {
            200 => new OkObjectResult(result.Value),
            201 => new ObjectResult(result.Value) { StatusCode = 201 },
            400 => new BadRequestObjectResult(new { message = result.Error }),
            401 => new UnauthorizedObjectResult(new { message = result.Error }),
            403 => new ObjectResult(new { message = result.Error }) { StatusCode = 403 },
            404 => new NotFoundObjectResult(new { message = result.Error }),
            409 => new ConflictObjectResult(new { message = result.Error }),
            _   => new BadRequestObjectResult(new { message = result.Error }),
        };
    }

    public static IActionResult ToActionResult(this Result result)
    {
        return result.StatusCode switch
        {
            200 => new OkResult(),
            400 => new BadRequestObjectResult(new { message = result.Error }),
            401 => new UnauthorizedObjectResult(new { message = result.Error }),
            403 => new ObjectResult(new { message = result.Error }) { StatusCode = 403 },
            404 => new NotFoundObjectResult(new { message = result.Error }),
            409 => new ConflictObjectResult(new { message = result.Error }),
            _   => new BadRequestObjectResult(new { message = result.Error }),
        };
    }
}