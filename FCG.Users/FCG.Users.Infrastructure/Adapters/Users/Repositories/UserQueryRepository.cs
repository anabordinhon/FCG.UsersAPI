using FCG.Users.Application.Common;
using FCG.Users.Application.Users.Ports;
using FCG.Users.Domain.Users.Entities;
using FCG.Users.Domain.Users.ValueObjects;
using FCG.Users.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FCG.Users.Infrastructure.Adapters.Users.Repositories
{
    public class UserQueryRepository : IUserQueryRepository
    {
        private readonly AppDbContext _dbContext;
        public UserQueryRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<User> GetByIdAsync(Guid publicId, CancellationToken cancellationToken)
        {
            return await _dbContext.Users
                  .AsNoTracking()
                  .FirstAsync(u => u.PublicId == publicId, cancellationToken);
        }

        public async Task<PagedResult<User>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken)
        {
            var totalCount = await _dbContext.Users.AsNoTracking().CountAsync(cancellationToken);

            var users = await _dbContext.Users
              .AsNoTracking()
              .OrderBy(u => u.CreatedAt)
              .Skip((page - 1) * pageSize)
              .Take(pageSize)
              .ToListAsync(cancellationToken);

            return new PagedResult<User>
            {
                Items = users,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }
        //public async Task<User> GetByIdWithPromotionsAsync(int userId, CancellationToken cancellationToken)
        //{
        //    return await _dbContext.Users
        //        .AsNoTracking()
        //        .Include(u => u.PromotionId)
        //        .FirstAsync(u => u.Id == userId, cancellationToken);
        //}

        public async Task<User?> GetByEmailAsync(EmailAddress email, CancellationToken cancellationToken)
        {
            var normalizedEmail = email.Email.ToLowerInvariant();

            return await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Email.Email.ToLower() == normalizedEmail);
        }
    }
}
