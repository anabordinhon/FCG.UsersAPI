using FCG.Users.Application.Common;
using FCG.Users.Domain.Users.Entities;
using FCG.Users.Domain.Users.ValueObjects;

namespace FCG.Users.Application.Users.Ports
{
    public interface IUserQueryRepository
    {
        Task<User> GetByIdAsync(Guid PublicId, CancellationToken cancellationToken);
        Task<PagedResult<User>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken);
        Task<User> GetByIdWithPromotionsAsync(int userId, CancellationToken cancellationToken);
        Task<User?> GetByEmailAsync(EmailAddress email, CancellationToken cancellationToken);
    }
}
