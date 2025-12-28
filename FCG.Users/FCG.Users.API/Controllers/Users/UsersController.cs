using FCG.Users.API.Common.Extensions;
using FCG.Users.Application.Common.Contracts;
using FCG.Users.Application.Users.UseCases.Commands.AddUser;
using FCG.Users.Application.Users.UseCases.Commands.DeactivateUser;
using FCG.Users.Application.Users.UseCases.Queries.GetUserById;
using FCG.Users.Application.Users.UseCases.Queries.GetUsersPaged;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FCG.Users.API.Controllers.Users;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetUsersPaged(
    [FromServices] IGetUsersPagedQueryHandler handler,
    CancellationToken cancellationToken,
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 10)
    {
        var query = new GetUsersPagedQuery(page, pageSize);
        var result = await handler.Handle(query, cancellationToken);
        return result.ToOkActionResult();
    }

    [HttpGet("{publicId}")]
    public async Task<IActionResult> GetUserById(
        [FromRoute] Guid publicId,
        [FromServices] IGetUserByIdQueryHandler handler,
        CancellationToken cancellationToken)
    {
        var query = new GetUserByIdQuery(publicId);
        var result = await handler.Handle(query, cancellationToken);
        return result.ToOkActionResult();
    }

    [Authorize(Roles = nameof(EUserRoleContract.Admin))]
    [HttpPost]
    public async Task<IActionResult> AddOrUpdateUser(
        [FromBody] AddOrUpdateUserInput input,
        [FromServices] IAddOrUpdateUserCommandHandler handler,
        CancellationToken cancellationToken)
    {
        var command = input.MapToCommand();
        var result = await handler.Handle(command, cancellationToken);
        return result.ToCreatedActionResult($"/api/users/{result.Data.PublicId}");
    }

    [Authorize(Roles = nameof(EUserRoleContract.Admin))]
    [HttpPatch("{publicId}/deactivate")]
    public async Task<IActionResult> DeactivateUser(
        [FromRoute] Guid publicId,
        [FromServices] IDeactivateUserCommandHandler handler,
        CancellationToken cancellationToken)
    {
        var command = new DeactivateUserCommand(publicId);
        var result = await handler.Handle(command, cancellationToken);
        return result.ToNoContentActionResult();
    }
}
