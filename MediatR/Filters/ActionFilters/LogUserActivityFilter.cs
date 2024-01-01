using Microsoft.AspNetCore.Mvc.Filters;

namespace Mediatr.Api.Filters.ActionFilters
{
    public class LogUserActivityFilter : IAsyncActionFilter
    {
        public Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            Console.WriteLine($"User activity last time {DateTime.Now}");
            return next();
        }
    }
}
