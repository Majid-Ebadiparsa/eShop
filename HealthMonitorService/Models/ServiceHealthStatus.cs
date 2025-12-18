namespace HealthMonitorService.Models
{
	public class ServiceHealthStatus
	{
		public int Id { get; set; }
		public string ServiceName { get; set; } = string.Empty;
		public bool IsHealthy { get; set; }
		public string? StatusMessage { get; set; }
		public long ResponseTimeMs { get; set; }
		public DateTime CheckedAt { get; set; }
	}

	public class ServiceHealthHistory
	{
		public int Id { get; set; }
		public string ServiceName { get; set; } = string.Empty;
		public bool IsHealthy { get; set; }
		public string? StatusMessage { get; set; }
		public long ResponseTimeMs { get; set; }
		public DateTime CheckedAt { get; set; }
	}

	public class ServiceStatusDto
	{
		public string ServiceName { get; set; } = string.Empty;
		public bool IsHealthy { get; set; }
		public string? StatusMessage { get; set; }
		public long ResponseTimeMs { get; set; }
		public DateTime LastChecked { get; set; }
		public List<ServiceHealthHistoryDto> History { get; set; } = new();
	}

	public class ServiceHealthHistoryDto
	{
		public bool IsHealthy { get; set; }
		public string? StatusMessage { get; set; }
		public long ResponseTimeMs { get; set; }
		public DateTime CheckedAt { get; set; }
	}
}

