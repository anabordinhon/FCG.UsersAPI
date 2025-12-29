using FCG.Users.Application.Auth.Ports;
using FCG.Users.Application.Common;
using FCG.Users.Application.Common.Ports;
using FCG.Users.Application.Users.Ports;
using FCG.Users.Domain.Common.Enuns;
using System.Security.Claims;
using System.Text;

namespace FCG.Users.Application.Auth.UseCases.Queries.LoginUserQuery;
public class LoginUserQueryHandler : ILoginUserQueryHandler
{
    private readonly IUserQueryRepository _userQueryRepository;
    private readonly IHashHelper _hashHelper;
    private readonly ITokenService _tokenService;

    public LoginUserQueryHandler(
        IUserQueryRepository userQueryRepository,
        IHashHelper hashHelper,
        ITokenService tokenService)
    {
        _userQueryRepository = userQueryRepository;
        _hashHelper = hashHelper;
        _tokenService = tokenService;
    }

    public async Task<ResultData<LoginUserQueryOutput>> Handle(
        LoginUserQuery query,
        CancellationToken cancellationToken)
    {
        var user = await _userQueryRepository
            .GetByEmailAsync(query.EmailAdress, cancellationToken);

        if (user == null || !user.IsActive)
            return ResultData<LoginUserQueryOutput>
                .Error("Usuário ou senha inválidos");

        if (!_hashHelper.VerifyHash(
            query.RawPassword, user.PasswordHash, user.PasswordSalt))
            return ResultData<LoginUserQueryOutput>
                .Error("Usuário ou senha inválidos");

        var token = _tokenService.GenerateToken(
            user.PublicId,
            user.Role.ToString());

        return ResultData<LoginUserQueryOutput>.Success(
            new LoginUserQueryOutput
            {
                Token = token,
                PublicId = user.PublicId,
                Role = user.Role.ToString()
            });
    }
}
