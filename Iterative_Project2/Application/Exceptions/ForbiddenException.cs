using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Iterative_Project2.Application.Exceptions
{
    public class ForbiddenException : Exception         // 403 (logged in, but not allowed)
    {
        public ForbiddenException(string message) : base(message) { }
    }
}
