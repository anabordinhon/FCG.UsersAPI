using FCG.Users.Domain.Users.ValueObjects;

namespace FCG.Users.Application.Common.Ports
{
    public interface IHashHelper
    {
        public (string Hash, string Salt) GenerateHash(RawPassword password);
        bool VerifyHash(RawPassword password, string hash, string salt);
    }
}
