using Bitai.WebApi.Common;
using Bitai.WebApi.Server;
using Microsoft.AspNetCore.Http;
using Serilog;
using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using System.Linq;
using System.IO;
using Bitai.LDAPWebApi.Configurations.App;

namespace Bitai.LDAPWebApi.Middleware
{
    /// <summary>
    /// Middleware to handle Web Api Exceptions
    /// </summary>
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;



        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="next">Request delegate, see <see cref="RequestDelegate"./></param>
        /// <param name="logger">See <see cref="ILogger"/>Logger</param>
        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger logger)
        {
            _next = next;
            _logger = logger;
        }



        /// <summary>
        /// Invoke delegate, if necessary.
        /// </summary>
        /// <param name="httpContext">Http context, see <see cref="HttpContext"/>.</param>
        /// <returns></returns>
        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(httpContext, ex);
            }
        }



        /// <summary>
        /// Exception handling
        /// </summary>
        /// <param name="context">Http context, <see cref="HttpContext"/></param>
        /// <param name="exception"><see cref="Exception"/> to handle.</param>
        /// <returns></returns>
        private Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            _logger.Error($"{nameof(ExceptionHandlingMiddleware)}: Interceped error.");
            _logger.Error($"{exception}");

            HttpStatusCode httpStatusCode;
            if (typeof(ResourceNotFoundException).Equals(exception.GetType()))
                httpStatusCode = HttpStatusCode.NotFound;
            else
                httpStatusCode = HttpStatusCode.InternalServerError;

            context.Response.StatusCode = (int)httpStatusCode;
            context.Response.ContentType = MediaTypes.ApplicationJson;

            var contentModel = new MiddlewareExceptionModel(exception);

            return context.Response.WriteAsync(JsonSerializer.Serialize(contentModel));
        }
    }
}