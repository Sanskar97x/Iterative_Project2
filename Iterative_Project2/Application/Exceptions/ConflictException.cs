using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Iterative_Project2.Application.Exceptions
{
    public class ConflictException : Exception          // 409 (duplicate email, username, application)
    {
        public ConflictException(string message) : base(message) { }
    }
}
