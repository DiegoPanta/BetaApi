using System.Net;
using Polly;
using Polly.Extensions.Http;

namespace Infrastructure.ExternalServices
{
    public static class PollyPolicies
    {
        public static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(msg => msg.StatusCode == HttpStatusCode.InternalServerError)
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                (response, delay, retryCount, context) =>
                {
                    Console.WriteLine($"Tentativa {retryCount}: nova tentativa após {delay.TotalSeconds} segundos.");
                });
        }

        public static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30),
                onBreak: (result, timespan) =>
                {
                    Console.WriteLine($"Circuito aberto! Esperando {timespan.TotalSeconds} segundos...");
                },
                onReset: () =>
                {
                    Console.WriteLine($"Circuito fechado! Retomando chamadas.");
                });
        }

        public static IAsyncPolicy<HttpResponseMessage> GetTimeoutPolicy()
        {
            return Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(3));
        }
    }
}
