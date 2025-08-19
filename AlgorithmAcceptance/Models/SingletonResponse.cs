using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgorithmAcceptanceTool.Models
{
    public class SingletonResponse<T> : BaseResponse
    {
        public T Data { get; set; }

        public SingletonResponse()
        {
        }

        public SingletonResponse(T data)
        {
            Data = data;
        }

        public SingletonResponse(int code) : base(code)
        {
        }

        public SingletonResponse(int code, string message) : base(code, message)
        {
        }
    }
}
