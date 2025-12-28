using FCG.Users.Application.Common;
using FCG.Users.Application.Common.Ports;
using FCG.Users.Application.Users.Mappers;
using FCG.Users.Application.Users.Outputs;
using FCG.Users.Application.Users.Ports;
using FCG.Users.Domain.Users.Entities;

namespace FCG.Users.Application.Users.UseCases.Commands.AddUser
{
    public class AddOrUpdateUserCommandHandler : IAddOrUpdateUserCommandHandler
    {
        private readonly IHashHelper _hashHelper;
        private readonly IUserCommandRepository _userCommandRepository;
        public AddOrUpdateUserCommandHandler(IHashHelper hashHelper, IUserCommandRepository userCommandRepository)
        {
            _hashHelper = hashHelper;
            _userCommandRepository = userCommandRepository;
        }
        public async Task<ResultData<UserOutput>> Handle(AddOrUpdateUserCommand command, CancellationToken cancellationToken)
        {
            var userExists = command.PublicId is not null
            && await _userCommandRepository.UserExistsAsync(command.PublicId, cancellationToken);

            var passwordHash = _hashHelper.GenerateHash(command.Password);
            var user = User.Create(command.Name, command.Email, command.NickName, passwordHash.Hash, passwordHash.Salt, command.Role);

            if (userExists)
                await _userCommandRepository.Update(user, cancellationToken);
            else
                await _userCommandRepository.AddAsync(user, cancellationToken);

            var userOutput = user.ToOutput();

            return ResultData<UserOutput>.Success(userOutput);
        }
    }
}
