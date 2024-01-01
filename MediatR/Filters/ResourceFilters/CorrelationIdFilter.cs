using Microsoft.AspNetCore.Mvc.Filters;

namespace Mediatr.Api.Filters.ResourceFilters
{
    public class CorrelationIdFilter : IAsyncResourceFilter
    {
        // GuidGenerator its scoped, so response will contain same correlationId 
        // as correlationId logged in LogginPipeline from MediatR pipeline
        private readonly GuidGenerator _guidGenerator;

        public CorrelationIdFilter(GuidGenerator guidGenerator)
        {
            _guidGenerator = guidGenerator;
        }

        public async Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
        {
            context.HttpContext.Response.Headers.Add("rimb-correlation-id", _guidGenerator.CorrelationId.ToString());
            await next();
        }
    }
}
