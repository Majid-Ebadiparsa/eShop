namespace HealthMonitorService.Domain.Entities
{
	/// <summary>
	/// Represents a detailed log entry for a health check execution cycle
	/// </summary>
	public class ServiceExecutionLog
	{
		public long Id { get; set; }
		
		/// <summary>
		/// Name of the service being checked
		/// </summary>
		public string ServiceName { get; set; } = string.Empty;
		
		/// <summary>
		/// When the health check execution started
		/// </summary>
		public DateTime ExecutionStartedAt { get; set; }
		
		/// <summary>
		/// When the health check execution completed
		/// </summary>
		public DateTime ExecutionCompletedAt { get; set; }
		
		/// <summary>
		/// Duration of the health check in milliseconds
		/// </summary>
		public long DurationMs { get; set; }
		
		/// <summary>
		/// Whether the health check completed successfully (not the service health, but the check itself)
		/// </summary>
		public bool ExecutionSucceeded { get; set; }
		
		/// <summary>
		/// Result of the health check (if succeeded)
		/// </summary>
		public bool? ServiceIsHealthy { get; set; }
		
		/// <summary>
		/// Response time of the service (if succeeded)
		/// </summary>
		public long? ServiceResponseTimeMs { get; set; }
		
		/// <summary>
		/// HTTP status code returned (if applicable)
		/// </summary>
		public int? HttpStatusCode { get; set; }
		
		/// <summary>
		/// Error message if execution failed
		/// </summary>
		public string? ErrorMessage { get; set; }
		
		/// <summary>
		/// Error code for categorization
		/// </summary>
		public string? ErrorCode { get; set; }
		
		/// <summary>
		/// Exception type if error occurred
		/// </summary>
		public string? ExceptionType { get; set; }
		
		/// <summary>
		/// Stack trace if error occurred (for debugging)
		/// </summary>
		public string? StackTrace { get; set; }
		
		/// <summary>
		/// Additional metadata or notes about the execution
		/// </summary>
		public string? Metadata { get; set; }
		
		/// <summary>
		/// The service address that was checked
		/// </summary>
		public string? ServiceAddress { get; set; }
		
		/// <summary>
		/// The service port that was checked
		/// </summary>
		public int? ServicePort { get; set; }
	}
}

