using FCG.Users.Application.Common;
using FCG.Users.Application.Users.Mappers;
using FCG.Users.Application.Users.Outputs;
using FCG.Users.Application.Users.Ports;

namespace FCG.Users.Application.Users.UseCases.Commands.DeactivateUser
{
    public class DeactivateUserCommandHandler : IDeactivateUserCommandHandler
    {
        private readonly IUserCommandRepository _userCommandRepository;
        public DeactivateUserCommandHandler(IUserCommandRepository userCommandRepository)
        {
            _userCommandRepository = userCommandRepository;
        }
        public async Task<ResultData<UserOutput>> Handle(DeactivateUserCommand command, CancellationToken cancellationToken)
        {
            var user = await _userCommandRepository.GetByIdAsync(command.PublicId, cancellationToken);

            if (user is null)
                return ResultData<UserOutput>.Error("Usuário não encontrado.");

            user.Deactivate();
            await _userCommandRepository.Update(user, cancellationToken);

            var userOutput = user.ToOutput();

            return ResultData<UserOutput>.Success(userOutput);
        }
    }
}
