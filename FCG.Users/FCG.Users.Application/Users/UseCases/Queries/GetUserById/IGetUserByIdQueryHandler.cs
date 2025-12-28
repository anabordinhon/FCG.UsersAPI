using FCG.Users.Application.Common;
using FCG.Users.Application.Users.Outputs;

namespace FCG.Users.Application.Users.UseCases.Queries.GetUserById
{
    public interface IGetUserByIdQueryHandler
    {
        Task<ResultData<UserOutput>> Handle(GetUserByIdQuery query, CancellationToken cancellationToken);

    }
}
