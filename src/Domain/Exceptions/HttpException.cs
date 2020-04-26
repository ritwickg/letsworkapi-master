using System;
using System.Net;

namespace LetsWork.Domain.Exceptions
{
    public class HttpException : Exception
    {
        public HttpException(string ExceptionMessage, HttpStatusCode StatusCode) : base(ExceptionMessage)
        {
            this.StatusCode = StatusCode;
        }

        public HttpStatusCode StatusCode { get; private set; }
    }
}
