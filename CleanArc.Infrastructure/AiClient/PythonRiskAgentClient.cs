using System.Net.Http.Json;
using CleanArc.Application.Contracts;
using CleanArc.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CleanArc.Infrastructure.AiClient;

public sealed class PythonRiskAgentClient : IPythonRiskAgentClient
{
    private readonly HttpClient _httpClient;
    private readonly PythonRiskServiceOptions _options;
    private readonly ILogger<PythonRiskAgentClient> _logger;

    public PythonRiskAgentClient(
        HttpClient httpClient,
        IOptions<PythonRiskServiceOptions> options,
        ILogger<PythonRiskAgentClient> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<PythonRiskResult> AnalyzeAsync(
        RiskAnalysisCommand command,
        CancellationToken cancellationToken = default)
    {
        var request = new PythonRiskRequest(command.Age, command.Claims, command.Amount);

        for (var attempt = 1; attempt <= _options.MaxRetries; attempt++)
        {
            try
            {
                using var response = await _httpClient.PostAsJsonAsync(_options.AnalyzePath, request, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    var body = await response.Content.ReadAsStringAsync(cancellationToken);
                    throw new HttpRequestException($"Python service returned {(int)response.StatusCode}: {body}");
                }

                var payload = await response.Content.ReadFromJsonAsync<PythonRiskResponse>(cancellationToken: cancellationToken);

                if (payload is null)
                {
                    throw new InvalidOperationException("Python service returned empty payload.");
                }

                return new PythonRiskResult(
                    payload.Score,
                    payload.Decision ?? string.Empty,
                    payload.Reason ?? string.Empty);
            }
            catch (Exception ex) when (attempt < _options.MaxRetries)
            {
                _logger.LogWarning(
                    ex,
                    "Python risk analysis attempt {Attempt}/{MaxRetries} failed. Retrying.",
                    attempt,
                    _options.MaxRetries);

                await Task.Delay(TimeSpan.FromMilliseconds(250 * attempt), cancellationToken);
            }
        }

        // Final attempt: let exception bubble up with full context.
        using var finalResponse = await _httpClient.PostAsJsonAsync(_options.AnalyzePath, request, cancellationToken);
        finalResponse.EnsureSuccessStatusCode();

        var finalPayload = await finalResponse.Content.ReadFromJsonAsync<PythonRiskResponse>(cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Python service returned empty payload.");

        return new PythonRiskResult(
            finalPayload.Score,
            finalPayload.Decision ?? string.Empty,
            finalPayload.Reason ?? string.Empty);
    }

    private sealed record PythonRiskRequest(int Age, int Claims, decimal Amount);
    private sealed record PythonRiskResponse(double Score, string Decision, string Reason);
}
