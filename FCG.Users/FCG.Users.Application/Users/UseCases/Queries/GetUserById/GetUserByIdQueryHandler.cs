using FCG.Users.Application.Common;
using FCG.Users.Application.Users.Mappers;
using FCG.Users.Application.Users.Outputs;
using FCG.Users.Application.Users.Ports;

namespace FCG.Users.Application.Users.UseCases.Queries.GetUserById
{
    public class GetUserByIdQueryHandler : IGetUserByIdQueryHandler
    {
        private readonly IUserQueryRepository _userQueryRepository;
        public GetUserByIdQueryHandler(IUserQueryRepository userQueryRepository)
        {
            _userQueryRepository = userQueryRepository;
        }
        public async Task<ResultData<UserOutput>> Handle(GetUserByIdQuery query, CancellationToken cancellationToken)
        {
            var user = await _userQueryRepository.GetByIdAsync(query.PublicId, cancellationToken);

            if (user is null)
            {
                return ResultData<UserOutput>.Error("Usuário não encontrado.");
            }

            var userOutput = user.ToOutput();

            return ResultData<UserOutput>.Success(userOutput);

        }
    }
}
