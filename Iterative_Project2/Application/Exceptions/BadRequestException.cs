using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Iterative_Project2.Application.Exceptions
{
    public class BadRequestException : Exception        // 400
    {
        public BadRequestException(string message) : base(message) { }
    }

}
