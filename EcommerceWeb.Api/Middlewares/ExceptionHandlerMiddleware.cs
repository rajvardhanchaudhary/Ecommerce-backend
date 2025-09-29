using Microsoft.AspNetCore.Diagnostics;
using System.Net;
using System.Text.Json;

namespace EcommerceWeb.Api.Middlewares
{
    public class ExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionHandlerMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var feature = context.Features.Get<IExceptionHandlerFeature>();
            if (feature != null)
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                context.Response.ContentType = "application/json";

                var response = new
                {
                    success = false,
                    message = feature.Error.Message
                };

                var json = JsonSerializer.Serialize(response);
                await context.Response.WriteAsync(json);
                return;
            }

            await _next(context);
        }
    }
}
