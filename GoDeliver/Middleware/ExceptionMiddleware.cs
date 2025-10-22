using GoDeliver.Application.Common.Models;
using GoDeliver.Application.Exceptions;
using System.Net;
using System.Text;
using System.Text.Json;

namespace GoDeliver.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IHostEnvironment _env;

        public ExceptionMiddleware(RequestDelegate next, IHostEnvironment env)
        {
            _next = next;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (NotFoundException nfEx)
            {
                await HandleExceptionAsync(context, HttpStatusCode.NotFound, nfEx.Message);
            }
            catch (ValidationException vEx)
            {
                await HandleExceptionAsync(context, HttpStatusCode.BadRequest, vEx.Message, vEx.ValidationErrors);
            }
            catch (Exception ex)
            {
                string message;

                if (_env.IsDevelopment())
                {
                    var sb = new StringBuilder();
                    sb.AppendLine($"Message: {ex.Message}");
                    if (ex.InnerException != null)
                    {
                        sb.AppendLine($"Inner Exception: {ex.InnerException.Message}");
                    }
                    message = sb.ToString();
                }
                else
                {
                    message = "An unexpected error occurred. Please contact support.";
                }

                await HandleExceptionAsync(context, HttpStatusCode.InternalServerError, message);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, HttpStatusCode statusCode, string message, List<string>? errors = null)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            var response = new ErrorResponse((int)statusCode, message, errors);

            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            await context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
        }
    }
}
