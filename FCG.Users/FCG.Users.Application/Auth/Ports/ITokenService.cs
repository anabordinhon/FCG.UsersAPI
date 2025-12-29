using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCG.Users.Application.Auth.Ports;
public interface ITokenService
{
    string GenerateToken(Guid userPublicId, string role);
}
