using FCG.Users.Domain.Users.Entities;

namespace FCG.Users.Application.Users.Ports
{
    public interface IUserCommandRepository
    {
        Task<User> AddAsync(User user, CancellationToken cancellationToken);
        Task<User> Update(User user, CancellationToken cancellationToken);
        Task<bool> UserExistsAsync(Guid? publicId, CancellationToken cancellationToken);
        Task<User> GetByIdAsync(Guid PublicId, CancellationToken cancellationToken);

    }
}
