using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Iterative_Project2.Application.Exceptions
{
    public class UnauthorizedException : Exception      // 401 (bad login, missing/invalid token)
    {
        public UnauthorizedException(string message) : base(message) { }
    }
}
