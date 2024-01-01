using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Mediatr.Api.Filters.ActionFilters
{
    public class CachedFilter : Attribute, IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var path = context.HttpContext.Request.Path;
            
            // check if redis or in-memory cahce contains data for such path
            if (path == "/api/v1/products")
            {
                // return from cache
                context.Result = new OkObjectResult("abc");
                return;
            }

            var executedContext = await next();

            if (executedContext.Result is OkObjectResult result)
            {
                // cache in redis or in-memory cache whatever..
            }
        }
    }
}
