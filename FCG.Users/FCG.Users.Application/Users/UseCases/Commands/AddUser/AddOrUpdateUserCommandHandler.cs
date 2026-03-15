using FCG.Users.Application.Common;
using FCG.Users.Application.Common.Ports;
using FCG.Users.Application.Users.Events;
using FCG.Users.Application.Users.Mappers;
using FCG.Users.Application.Users.Outputs;
using FCG.Users.Application.Users.Ports;
using FCG.Users.Domain.Users.Entities;
using Microsoft.Extensions.Logging;

namespace FCG.Users.Application.Users.UseCases.Commands.AddUser
{
    public class AddOrUpdateUserCommandHandler : IAddOrUpdateUserCommandHandler
    {
        private readonly IHashHelper _hashHelper;
        private readonly IUserCommandRepository _userCommandRepository;
        private readonly ILogger<AddOrUpdateUserCommandHandler> _logger;
        private readonly IEventPublisher _eventPublisher;

        public AddOrUpdateUserCommandHandler(
            IHashHelper hashHelper,
            IUserCommandRepository userCommandRepository,
            ILogger<AddOrUpdateUserCommandHandler> logger,
            IEventPublisher eventPublisher
)
        {
            _hashHelper = hashHelper;
            _userCommandRepository = userCommandRepository;
            _logger = logger;
            _eventPublisher = eventPublisher;

        }
        public async Task<ResultData<UserOutput>> Handle(AddOrUpdateUserCommand command, CancellationToken cancellationToken)
        {
            var correlationId = Guid.NewGuid();

            try
            {
                var isUpdate = command.PublicId is not null
                && await _userCommandRepository.UserExistsAsync(command.PublicId, cancellationToken);

                var passwordHash = _hashHelper.GenerateHash(command.Password);

                User user;

                if (isUpdate)
                {

                    user = await _userCommandRepository.GetByIdAsync(command.PublicId.Value, cancellationToken);

                    user.UpdateDetails(command.Name, command.Email, command.NickName, passwordHash.Hash, passwordHash.Salt, command.Role);

                    await _userCommandRepository.Update(user, cancellationToken);
                }
                else
                {
                    user = User.Create(command.Name, command.Email, command.NickName, passwordHash.Hash, passwordHash.Salt, command.Role);
                    await _userCommandRepository.AddAsync(user, cancellationToken);

                    _logger.LogInformation(
                        "Cadastro persistido no banco. UserId: {UserId}, CorrelationId: {CorrelationId}",
                        user.PublicId,
                        correlationId);

                    await _eventPublisher.PublishAsync(new NotificationEvent { Type = "welcome" }, cancellationToken);
                    _logger.LogInformation(
                        "Notificação 'welcome' publicada no SQS. UserId: {UserId}, CorrelationId: {CorrelationId}",
                        user.PublicId,
                        correlationId);
                }

                var userOutput = user.ToOutput();
                return ResultData<UserOutput>.Success(userOutput);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha crítica ao processar usuário. CorrelationId: {CorrelationId}", correlationId);
                throw;
            }
        }
    }
}
