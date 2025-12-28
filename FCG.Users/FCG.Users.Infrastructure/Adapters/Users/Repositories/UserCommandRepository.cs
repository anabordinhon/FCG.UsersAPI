using FCG.Users.Application.Users.Ports;
using FCG.Users.Domain.Users.Entities;
using FCG.Users.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FCG.Users.Infrastructure.Adapters.Users.Repositories
{
    public class UserCommandRepository : IUserCommandRepository
    {
        private readonly AppDbContext _dbContext;

        public UserCommandRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<User> AddAsync(User user, CancellationToken cancellationToken)
        {
            await _dbContext.Users.AddAsync(user, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return user;
        }

        public async Task<User> Update(User user, CancellationToken cancellationToken)
        {
            _dbContext.Users.Update(user);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return user;
        }

        public async Task<bool> UserExistsAsync(Guid? publicId, CancellationToken cancellationToken)
        {
            if (publicId is null)
                return false;

            return await _dbContext.Users.AnyAsync(u => u.PublicId == publicId, cancellationToken);
        }

        public async Task<User> GetByIdAsync(Guid publicId, CancellationToken cancellationToken)
        {
            return await _dbContext.Users
                  .FirstAsync(u => u.PublicId == publicId, cancellationToken);
        }
    }
}
