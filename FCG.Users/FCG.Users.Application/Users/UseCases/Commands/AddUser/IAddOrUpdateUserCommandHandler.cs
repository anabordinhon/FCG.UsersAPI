using FCG.Users.Application.Common;
using FCG.Users.Application.Users.Outputs;

namespace FCG.Users.Application.Users.UseCases.Commands.AddUser
{
    public interface IAddOrUpdateUserCommandHandler
    {
        Task<ResultData<UserOutput>> Handle(AddOrUpdateUserCommand command, CancellationToken cancellationToken);
    }
}
