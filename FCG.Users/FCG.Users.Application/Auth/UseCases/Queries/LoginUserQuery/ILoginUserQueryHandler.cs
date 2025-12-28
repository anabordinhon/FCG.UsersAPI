using FCG.Users.Application.Common;

namespace FCG.Users.Application.Auth.UseCases.Queries.LoginUserQuery;
public interface ILoginUserQueryHandler
{
    Task<ResultData<LoginUserQueryOutput>> Handle(LoginUserQuery query, CancellationToken cancellationToken);
}
