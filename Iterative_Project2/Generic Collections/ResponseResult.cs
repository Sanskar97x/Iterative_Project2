using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Iterative_Project2.Generic_Collections
{
    // T is a generic type parameter that allows Data to hold different data types.
    // such as string,int, UserDto, or a list of objects.
    public class ResponseResult<T>
    {
        public T Data { get; set; }
        public string Message { get; set; } = null;
        public bool Status { get; set; } = false;



        // Static allows calling this method using the class name without creatin an instance.
        public static ResponseResult<T> Success(T data, string Message)
        {
            return new ResponseResult<T>
            {
                Data = data,
                Message = Message,
                Status = true
            };
        }

        public static ResponseResult<T> Failure(T data, string Message)
        {
            return new ResponseResult<T>
            {
                Data = data,
                Message = Message,
                Status = false
            };
        }
    }
}
