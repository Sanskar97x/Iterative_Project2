using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Iterative_Project2.Core.Interface
{
    public interface IJwtService
    {
        string GenerateToken(Guid userId, string email);
    }
}
