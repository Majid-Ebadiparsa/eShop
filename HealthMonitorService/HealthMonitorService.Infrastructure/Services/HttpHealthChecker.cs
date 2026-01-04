using HealthMonitorService.Application.Interfaces;
using HealthMonitorService.Domain.Entities;

namespace HealthMonitorService.Infrastructure.Services
{
	public class HttpHealthChecker : IHealthChecker
	{
		private readonly IHttpClientFactory _httpClientFactory;

		public HttpHealthChecker(IHttpClientFactory httpClientFactory)
		{
			_httpClientFactory = httpClientFactory;
		}

		public async Task<ServiceHealthStatus> CheckServiceHealthAsync(string serviceName, string address, int port, CancellationToken cancellationToken = default)
		{
			var stopwatch = System.Diagnostics.Stopwatch.StartNew();
			var healthUrl = $"http://{address}:{port}/health/ready";

			try
			{
				using var httpClient = _httpClientFactory.CreateClient();
				httpClient.Timeout = TimeSpan.FromSeconds(10);
				
				var response = await httpClient.GetAsync(healthUrl, cancellationToken);
				stopwatch.Stop();

				return new ServiceHealthStatus
				{
					ServiceName = serviceName,
					IsHealthy = response.IsSuccessStatusCode,
					StatusMessage = response.IsSuccessStatusCode ? "Healthy" : $"HTTP {response.StatusCode}",
					ResponseTimeMs = stopwatch.ElapsedMilliseconds,
					CheckedAt = DateTime.UtcNow,
					ErrorCode = response.IsSuccessStatusCode ? null : $"HTTP_{(int)response.StatusCode}",
					ExceptionType = null,
					StackTrace = null
				};
			}
			catch (Exception ex)
			{
				stopwatch.Stop();
				return new ServiceHealthStatus
				{
					ServiceName = serviceName,
					IsHealthy = false,
					StatusMessage = ex.Message,
					ResponseTimeMs = stopwatch.ElapsedMilliseconds,
					CheckedAt = DateTime.UtcNow,
					ErrorCode = DetermineErrorCode(ex),
					ExceptionType = ex.GetType().FullName,
					StackTrace = ex.StackTrace?.Length > 4000 ? ex.StackTrace[..4000] : ex.StackTrace
				};
			}
		}

		private static string DetermineErrorCode(Exception ex)
		{
			return ex switch
			{
				HttpRequestException => "HTTP_REQUEST_ERROR",
				TaskCanceledException => "TIMEOUT",
				OperationCanceledException => "CANCELLED",
				System.Net.Sockets.SocketException => "NETWORK_ERROR",
				_ => "UNKNOWN_ERROR"
			};
		}
	}
}

