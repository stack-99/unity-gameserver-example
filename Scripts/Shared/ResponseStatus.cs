using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Shared
{
    public abstract class ResponseStatus
    {
        public NetOP OPCode { get; set; }

        public bool Success { get; set; }

        public string Message { get; set; }
    }

    public class ErrorResponseStatus : ResponseStatus
    {
        public ErrorResponseStatus()
        {
            Success = false;
        }

        public Exception ExceptionOccurred { get; set; }
    }

    public class SuccessResponseStatus : ResponseStatus
    {
        public SuccessResponseStatus()
        {
            Success = true;
        }
    }
}
