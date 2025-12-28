using FCG.Users.Application.Common;
using FCG.Users.Application.Users.Outputs;

namespace FCG.Users.Application.Users.UseCases.Commands.DeactivateUser
{
    public interface IDeactivateUserCommandHandler
    {
        Task<ResultData<UserOutput>> Handle(DeactivateUserCommand command, CancellationToken cancellationToken);
    }

}
