using Mediatr.Api;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Diagnostics;

namespace MediatR.Application.Pipeline;

// LOG TO ELASTIC SEARCH + VISUALIZE WITH KIBANA

public class LoggingPipeline<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    private readonly ILogger<TRequest> _logger;
    private readonly GuidGenerator _guidGenerator;

    public LoggingPipeline(ILogger<TRequest> logger, GuidGenerator guidGenerator)
    {
        _logger = logger;
        _guidGenerator = guidGenerator;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = request.GetType().Name;

        var requestNameCombinated = $"{requestName} [{_guidGenerator.CorrelationId}]";
        var stopwatch = Stopwatch.StartNew();

        TResponse response = default!;
        _logger.LogInformation($"[START] {requestNameCombinated}");

        try
        {
            _logger.LogInformation($"[PROPS] {requestNameCombinated} {JsonConvert.SerializeObject(request)}");
            response = await next();
        }
        catch (Exception ex)
        {
            _logger.LogError($"[ERROR] {requestNameCombinated} {ex}");
            throw;
        }
        finally
        {
            stopwatch.Stop();
            _logger.LogInformation($"[END] {requestNameCombinated}; Time = {stopwatch.ElapsedMilliseconds} ms");
        }

        return response;
    }
}