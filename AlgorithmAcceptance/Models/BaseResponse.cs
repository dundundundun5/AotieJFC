using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgorithmAcceptanceTool.Models
{
    public class BaseResponse
    {
        public int Code { get; set; }
        public string ResultMessage { get; set; }

        public BaseResponse()
        {
        }

        public BaseResponse(int code)
        {
            Code = code;
        }

        public BaseResponse(int code, string resultMessage)
        {
            Code = code;
            ResultMessage = resultMessage;
        }
    }
}
