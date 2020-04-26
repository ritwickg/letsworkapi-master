using LetsWork.Domain.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Threading.Tasks;

namespace LetsWork.API.Middlewares
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;
        public ExceptionMiddleware(RequestDelegate Next, ILoggerFactory LoggerFactory)
        {
            _next = Next;
            _logger = LoggerFactory.CreateLogger("Exception");
        }

        public async Task InvokeAsync(HttpContext Context)
        {
            try
            {
                await _next(Context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(Context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext Context, Exception Exception)
        {
            string error_message = $"Requested operation could not be performed because of some issue at the server, please try again in some time.";
            HttpStatusCode exceptionStatusCode = HttpStatusCode.InternalServerError;

            //Return HTTP 400 if request model is null
            if (Exception is HttpException)
            {
                exceptionStatusCode = HttpStatusCode.BadRequest;
                error_message = $"Bad request, please check the request again before sending!";
            }

            //Log using NLog into log files
            _logger.LogError($"Failed because: {Exception.Message}");

            //Set response body middleware
            string errorMessage = JsonConvert.SerializeObject(new { message = error_message });
            Context.Response.ContentType = "application/json";
            Context.Response.StatusCode = (int)exceptionStatusCode;
            await Context.Response.WriteAsync(errorMessage);
        }
    }
}
