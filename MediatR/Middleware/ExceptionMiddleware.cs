using MediatR.Application.Exceptions;
using Newtonsoft.Json;

namespace Mediatr.Api.Middleware
{
    public class ExceptionMiddleware : IMiddleware
    {
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                await next(context);
            }
            catch (ModelStateException ex)
            {
                context.Response.StatusCode = 400;
                context.Response.ContentType = "application/json";
                var payload = JsonConvert.SerializeObject(ex);
                await context.Response.WriteAsync(payload);
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = 500;
                // separate between env - development, test and prod
                await context.Response.WriteAsync($"Error: {ex.Message}");
            }
        }
    }
}
