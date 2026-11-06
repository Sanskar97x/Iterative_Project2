using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Iterative_Project2.Application.Exceptions
{
    public class NotFoundException : Exception          // 404
    {
        public NotFoundException(string message) : base(message) { }
    }
}
