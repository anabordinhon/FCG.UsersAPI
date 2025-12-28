using FCG.Users.Application.Common;
using FCG.Users.Application.Users.Mappers;
using FCG.Users.Application.Users.Outputs;
using FCG.Users.Application.Users.Ports;

namespace FCG.Users.Application.Users.UseCases.Queries.GetUsersPaged
{
    public class GetUsersPagedQueryHandler : IGetUsersPagedQueryHandler
    {
        private readonly IUserQueryRepository _userQueryRepository;
        public GetUsersPagedQueryHandler(IUserQueryRepository userQueryRepository)
        {
            _userQueryRepository = userQueryRepository;
        }
        public async Task<ResultData<PagedResult<UserOutput>>> Handle(GetUsersPagedQuery query, CancellationToken cancellationToken)
        {
            var pagedResult = await _userQueryRepository.GetPagedAsync(
                    query.Page,
                    query.PageSize,
                    cancellationToken);

            var items = pagedResult.Items.ToOutput();

            var pagedResultOutput = new PagedResult<UserOutput>
            {
                Items = items,
                Page = pagedResult.Page,
                PageSize = pagedResult.PageSize,
                TotalCount = pagedResult.TotalCount
            };

            return ResultData<PagedResult<UserOutput>>.Success(pagedResultOutput);

        }
    }
}
