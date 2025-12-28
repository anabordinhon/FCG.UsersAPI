using FCG.Users.Application.Common;
using FCG.Users.Application.Users.Outputs;

namespace FCG.Users.Application.Users.UseCases.Queries.GetUsersPaged
{
    public interface IGetUsersPagedQueryHandler
    {
        Task<ResultData<PagedResult<UserOutput>>> Handle(GetUsersPagedQuery query, CancellationToken cancellationToken);

    }
}
