using FCG.Users.Application.Common;
using FCG.Users.Application.Common.Ports;
using FCG.Users.Application.Users.Events;
using FCG.Users.Application.Users.Mappers;
using FCG.Users.Application.Users.Outputs;
using FCG.Users.Application.Users.Ports;
using FCG.Users.Domain.Users.Entities;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace FCG.Users.Application.Users.UseCases.Commands.AddUser
{
    public class AddOrUpdateUserCommandHandler : IAddOrUpdateUserCommandHandler
    {
        private readonly IHashHelper _hashHelper;
        private readonly IUserCommandRepository _userCommandRepository;
        private readonly ILogger<AddOrUpdateUserCommandHandler> _logger;
        private readonly IPublishEndpoint _publishEndpoint;
        public AddOrUpdateUserCommandHandler(
            IHashHelper hashHelper,
            IUserCommandRepository userCommandRepository,
            ILogger<AddOrUpdateUserCommandHandler> logger,
            IPublishEndpoint publishEndpoint)
        {
            _hashHelper = hashHelper;
            _userCommandRepository = userCommandRepository;
            _logger = logger;
            _publishEndpoint = publishEndpoint;
        }
        public async Task<ResultData<UserOutput>> Handle(AddOrUpdateUserCommand command, CancellationToken cancellationToken)
        {
            var correlationId = Guid.NewGuid();

            try
            {
                var isUpdate = command.PublicId is not null
                && await _userCommandRepository.UserExistsAsync(command.PublicId, cancellationToken);

                var passwordHash = _hashHelper.GenerateHash(command.Password);

                var user = User.Create(command.Name, command.Email, command.NickName, passwordHash.Hash, passwordHash.Salt, command.Role);

                if (isUpdate)
                {
                    await _userCommandRepository.Update(user, cancellationToken);
                }
                else
                {
                    await _userCommandRepository.AddAsync(user, cancellationToken);

                    _logger.LogInformation(
                        "Cadastro persistido no banco. UserId: {UserId}, CorrelationId: {CorrelationId}",
                        user.PublicId,
                        correlationId);

                    var userCreatedEvent = new UserCreatedEvent
                    {
                        UserId = user.PublicId,
                        Email = user.Email.Email,
                        Name = user.FullName.Name,
                        NickName = user.NickName.Nick,
                        Role = user.Role.ToString(),
                        CreatedAt = DateTime.UtcNow,
                        CorrelationId = correlationId
                    };

                    await _publishEndpoint.Publish(userCreatedEvent, cancellationToken);

                    _logger.LogInformation(
                        "UserCreatedEvent publicado. EventId: {EventId}, CorrelationId: {CorrelationId}, Destino: RabbitMQ",
                        userCreatedEvent.EventId,
                        userCreatedEvent.CorrelationId);
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
