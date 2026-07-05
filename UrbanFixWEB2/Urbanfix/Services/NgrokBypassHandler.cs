namespace Urbanfix.Services;

public sealed class NgrokBypassHandler : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        if (request.RequestUri?.Host.Contains("ngrok", StringComparison.OrdinalIgnoreCase) == true
            && !request.Headers.Contains("Ngrok-Skip-Browser-Warning"))
        {
            request.Headers.TryAddWithoutValidation("Ngrok-Skip-Browser-Warning", "true");
        }

        return base.SendAsync(request, cancellationToken);
    }
}
