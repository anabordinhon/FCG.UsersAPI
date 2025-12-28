using FCG.Users.Application.Common.Contracts;
using FCG.Users.Domain.Common.Enuns;

namespace FCG.Users.Application.Common.Mappers
{
    public static class UserRoleMapper
    {
        public static EUserRole ToDomain(EUserRoleContract contract)
        {
            return contract switch
            {
                EUserRoleContract.Admin => EUserRole.Admin,
                EUserRoleContract.User => EUserRole.User,
                _ => throw new ArgumentOutOfRangeException(
                    nameof(contract),
                    contract,
                    "User role contract inválido")
            };
        }

        public static EUserRoleContract ToContract(EUserRole domain)
        {
            return domain switch
            {
                EUserRole.Admin => EUserRoleContract.Admin,
                EUserRole.User => EUserRoleContract.User,
                _ => throw new ArgumentOutOfRangeException(
                    nameof(domain),
                    domain,
                    "User role domain inválido")
            };
        }
    }
}
