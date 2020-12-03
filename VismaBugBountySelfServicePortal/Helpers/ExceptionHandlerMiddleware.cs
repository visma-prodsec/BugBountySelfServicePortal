using System;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;

namespace VismaBugBountySelfServicePortal.Helpers
{
    public class ExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IActionResultExecutor<ObjectResult> _executor;
        private readonly ILogger _logger;
        private static readonly ActionDescriptor EmptyActionDescriptor = new ActionDescriptor();

        public ExceptionHandlerMiddleware(RequestDelegate next, IActionResultExecutor<ObjectResult> executor, ILogger<ExceptionHandlerMiddleware> logger)
        {
            _next = next;
            _executor = executor;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (SecurityTokenException ex)
            {
                _logger.LogError(ex.Message);

                var result = new ObjectResult(null)
                {
                    StatusCode = (int)HttpStatusCode.Unauthorized,
                };
                var actionContext = new ActionContext(context, new RouteData(), EmptyActionDescriptor);
                await _executor.ExecuteAsync(actionContext, result);
            }
            catch (ArgumentException argumentException)
            {
                if (context.Response.HasStarted)
                {
                    throw;
                }

                var routeData = context.GetRouteData() ?? new RouteData();

                ClearCacheHeaders(context.Response);

                var actionContext = new ActionContext(context, routeData, EmptyActionDescriptor);

                var result = new ObjectResult(new ErrorResponse($"Bad request: {argumentException.Message}."))
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                };

                await _executor.ExecuteAsync(actionContext, result);
            }
            catch (Exception ex)
            {
                var message = $"An unhandled exception has occurred while executing the request. Url: {context.Request.GetDisplayUrl()}. Request Data: " + GetRequestData(context);
                _logger.LogError(ex, message);
                if (context.Response.HasStarted)
                {
                    throw;
                }

                var routeData = context.GetRouteData() ?? new RouteData();

                ClearCacheHeaders(context.Response);

                var actionContext = new ActionContext(context, routeData, EmptyActionDescriptor);

                var result = new ObjectResult(new ErrorResponse($"Error processing request. Server error: {ex.Message}."))
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                };

                await _executor.ExecuteAsync(actionContext, result);
            }
        }

        private static string GetRequestData(HttpContext context)
        {
            var sb = new StringBuilder();

            if (context.Request.HasFormContentType && context.Request.Form.Any())
            {
                sb.Append("Form variables:");
                foreach (var x in context.Request.Form)
                {
                    sb.AppendFormat("Key={0}, Value={1}<br/>", x.Key, x.Value);
                }
            }

            sb.AppendLine("Method: " + context.Request.Method);

            return sb.ToString();
        }

        private static void ClearCacheHeaders(HttpResponse response)
        {
            response.Headers[HeaderNames.CacheControl] = "no-cache";
            response.Headers[HeaderNames.Pragma] = "no-cache";
            response.Headers[HeaderNames.Expires] = "-1";
            response.Headers.Remove(HeaderNames.ETag);
        }

        [DataContract(Name = "ErrorResponse")]
        public class ErrorResponse
        {
            [DataMember(Name = "Message")]
            public string Message { get; set; }

            public ErrorResponse(string message)
            {
                Message = message;
            }
        }
    }
}
