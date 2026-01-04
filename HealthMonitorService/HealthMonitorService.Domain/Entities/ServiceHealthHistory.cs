namespace HealthMonitorService.Domain.Entities
{
	public class ServiceHealthHistory
	{
		public int Id { get; set; }
		public string ServiceName { get; set; } = string.Empty;
		public bool IsHealthy { get; set; }
		public string? StatusMessage { get; set; }
		public long ResponseTimeMs { get; set; }
		public DateTime CheckedAt { get; set; }

		// Structured error fields
		public string? ErrorCode { get; set; }
		public string? ExceptionType { get; set; }
		public string? StackTrace { get; set; }
	}
}

