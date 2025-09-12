namespace PaymentService.Infrastructure.Messaging.POCO
{
	public class CircuitBreakerOptions
	{
		public double FailureThreshold { get; set; }
		public TimeSpan SamplingDurationSeconds { get; set; }
		public int MinimumThroughput { get; set; }
		public TimeSpan DurationOfBreakSeconds { get; set; }
	}
}
