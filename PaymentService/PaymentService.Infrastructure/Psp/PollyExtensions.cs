using Microsoft.Extensions.Configuration;
using PaymentService.Infrastructure.Messaging.POCO;
using Polly;

namespace PaymentService.Infrastructure.Psp
{
	public static class PollyExtensions
	{
		public static IAsyncPolicy<TResult> CreateResultPolicy<TResult>(Func<TResult, bool> isTransient)
				=> Policy<TResult>
						.Handle<Exception>()
						.OrResult(isTransient)
						.WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromMilliseconds(200 * retryAttempt));

		public static IAsyncPolicy CreateBreakerPolicy(IConfiguration cfg)
		{
			var options = new CircuitBreakerOptions();
			cfg.GetSection("Polly:CircuitBreaker").Bind(options);

			return Policy
					.Handle<Exception>()
					.AdvancedCircuitBreakerAsync
					(
						failureThreshold: options.FailureThreshold,
						samplingDuration: options.SamplingDurationSeconds,
						minimumThroughput: options.MinimumThroughput,
						durationOfBreak: options.DurationOfBreakSeconds
					);
		}

		public static IAsyncPolicy CreateJitterRetry()
				=> Policy
						.Handle<Exception>()
						.WaitAndRetryAsync(3, _ => TimeSpan.FromMilliseconds(Random.Shared.Next(100, 600)));
	}
}
