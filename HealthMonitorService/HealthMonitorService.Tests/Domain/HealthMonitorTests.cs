using HealthMonitorService.Domain.Entities;

namespace HealthMonitorService.Tests.Domain
{
	public class ServiceHealthStatusTests
	{
		[Fact]
		public void ServiceHealthStatus_Should_Initialize_With_Default_Values()
		{
			// Act
			var status = new ServiceHealthStatus();

			// Assert
			Assert.Equal(0, status.Id);
			Assert.Equal(string.Empty, status.ServiceName);
			Assert.False(status.IsHealthy);
			Assert.Null(status.StatusMessage);
			Assert.Equal(0, status.ResponseTimeMs);
		}

		[Fact]
		public void ServiceHealthStatus_Should_Store_Basic_Health_Info()
		{
			// Arrange
			var serviceName = "orderservice";
			var isHealthy = true;
			var responseTime = 150L;
			var checkedAt = DateTime.UtcNow;

			// Act
			var status = new ServiceHealthStatus
			{
				ServiceName = serviceName,
				IsHealthy = isHealthy,
				ResponseTimeMs = responseTime,
				CheckedAt = checkedAt,
				StatusMessage = "OK"
			};

			// Assert
			Assert.Equal(serviceName, status.ServiceName);
			Assert.True(status.IsHealthy);
			Assert.Equal(responseTime, status.ResponseTimeMs);
			Assert.Equal(checkedAt, status.CheckedAt);
			Assert.Equal("OK", status.StatusMessage);
		}

		[Fact]
		public void ServiceHealthStatus_Should_Store_Error_Details()
		{
			// Arrange
			var errorCode = "CONNECTION_TIMEOUT";
			var exceptionType = "System.Net.Http.HttpRequestException";
			var stackTrace = "at System.Net.Http.HttpClient.Send()...";

			// Act
			var status = new ServiceHealthStatus
			{
				ServiceName = "paymentservice",
				IsHealthy = false,
				ErrorCode = errorCode,
				ExceptionType = exceptionType,
				StackTrace = stackTrace,
				StatusMessage = "Connection timeout after 5000ms"
			};

			// Assert
			Assert.Equal(errorCode, status.ErrorCode);
			Assert.Equal(exceptionType, status.ExceptionType);
			Assert.Equal(stackTrace, status.StackTrace);
			Assert.False(status.IsHealthy);
		}

		[Theory]
		[InlineData("orderservice", true, 100)]
		[InlineData("inventoryservice", false, 5000)]
		[InlineData("paymentservice", true, 250)]
		public void ServiceHealthStatus_Should_Handle_Various_Scenarios(string serviceName, bool isHealthy, long responseTime)
		{
			// Act
			var status = new ServiceHealthStatus
			{
				ServiceName = serviceName,
				IsHealthy = isHealthy,
				ResponseTimeMs = responseTime,
				CheckedAt = DateTime.UtcNow
			};

			// Assert
			Assert.Equal(serviceName, status.ServiceName);
			Assert.Equal(isHealthy, status.IsHealthy);
			Assert.Equal(responseTime, status.ResponseTimeMs);
		}
	}

	public class ServiceExecutionLogTests
	{
		[Fact]
		public void ServiceExecutionLog_Should_Initialize_With_Default_Values()
		{
			// Act
			var log = new ServiceExecutionLog();

			// Assert
			Assert.Equal(0, log.Id);
			Assert.Equal(string.Empty, log.ServiceName);
			Assert.False(log.ExecutionSucceeded);
			Assert.Null(log.ServiceIsHealthy);
		}

		[Fact]
		public void ServiceExecutionLog_Should_Store_Successful_Execution()
		{
			// Arrange
			var serviceName = "orderservice";
			var startTime = DateTime.UtcNow;
			var endTime = startTime.AddMilliseconds(150);

			// Act
			var log = new ServiceExecutionLog
			{
				ServiceName = serviceName,
				ExecutionStartedAt = startTime,
				ExecutionCompletedAt = endTime,
				DurationMs = 150,
				ExecutionSucceeded = true,
				ServiceIsHealthy = true,
				ServiceResponseTimeMs = 145,
				HttpStatusCode = 200,
				ServiceAddress = "orderservice",
				ServicePort = 8080
			};

			// Assert
			Assert.Equal(serviceName, log.ServiceName);
			Assert.True(log.ExecutionSucceeded);
			Assert.True(log.ServiceIsHealthy);
			Assert.Equal(150, log.DurationMs);
			Assert.Equal(200, log.HttpStatusCode);
		}

		[Fact]
		public void ServiceExecutionLog_Should_Store_Failed_Execution()
		{
			// Arrange
			var serviceName = "paymentservice";
			var errorMessage = "Connection refused";
			var errorCode = "CONNECTION_REFUSED";

			// Act
			var log = new ServiceExecutionLog
			{
				ServiceName = serviceName,
				ExecutionStartedAt = DateTime.UtcNow,
				ExecutionCompletedAt = DateTime.UtcNow.AddSeconds(5),
				DurationMs = 5000,
				ExecutionSucceeded = false,
				ErrorMessage = errorMessage,
				ErrorCode = errorCode,
				ExceptionType = "System.Net.Sockets.SocketException",
				HttpStatusCode = null
			};

			// Assert
			Assert.Equal(serviceName, log.ServiceName);
			Assert.False(log.ExecutionSucceeded);
			Assert.Equal(errorMessage, log.ErrorMessage);
			Assert.Equal(errorCode, log.ErrorCode);
			Assert.Null(log.ServiceIsHealthy);
		}

		[Fact]
		public void ServiceExecutionLog_Should_Calculate_Duration_Correctly()
		{
			// Arrange
			var startTime = DateTime.UtcNow;
			var duration = 250L;
			var endTime = startTime.AddMilliseconds(duration);

			// Act
			var log = new ServiceExecutionLog
			{
				ServiceName = "inventoryservice",
				ExecutionStartedAt = startTime,
				ExecutionCompletedAt = endTime,
				DurationMs = duration,
				ExecutionSucceeded = true
			};

			// Assert
			Assert.Equal(duration, log.DurationMs);
			var actualDuration = (log.ExecutionCompletedAt - log.ExecutionStartedAt).TotalMilliseconds;
			Assert.True(Math.Abs(actualDuration - duration) < 10); // Allow 10ms tolerance
		}

		[Fact]
		public void ServiceExecutionLog_Should_Store_Service_Address_And_Port()
		{
			// Arrange
			var serviceAddress = "orderservice";
			var servicePort = 8080;

			// Act
			var log = new ServiceExecutionLog
			{
				ServiceName = "orderservice",
				ServiceAddress = serviceAddress,
				ServicePort = servicePort,
				ExecutionStartedAt = DateTime.UtcNow,
				ExecutionCompletedAt = DateTime.UtcNow.AddMilliseconds(100),
				DurationMs = 100,
				ExecutionSucceeded = true
			};

			// Assert
			Assert.Equal(serviceAddress, log.ServiceAddress);
			Assert.Equal(servicePort, log.ServicePort);
		}

		[Fact]
		public void ServiceExecutionLog_Should_Store_Metadata()
		{
			// Arrange
			var metadata = "Additional debug info";

			// Act
			var log = new ServiceExecutionLog
			{
				ServiceName = "deliveryservice",
				Metadata = metadata,
				ExecutionStartedAt = DateTime.UtcNow,
				ExecutionCompletedAt = DateTime.UtcNow,
				DurationMs = 0,
				ExecutionSucceeded = true
			};

			// Assert
			Assert.Equal(metadata, log.Metadata);
		}

		[Theory]
		[InlineData(200, true)]
		[InlineData(500, false)]
		[InlineData(404, false)]
		[InlineData(503, false)]
		public void ServiceExecutionLog_Should_Handle_Various_Http_Status_Codes(int statusCode, bool shouldBeHealthy)
		{
			// Act
			var log = new ServiceExecutionLog
			{
				ServiceName = "testservice",
				HttpStatusCode = statusCode,
				ServiceIsHealthy = shouldBeHealthy,
				ExecutionStartedAt = DateTime.UtcNow,
				ExecutionCompletedAt = DateTime.UtcNow.AddMilliseconds(100),
				DurationMs = 100,
				ExecutionSucceeded = true
			};

			// Assert
			Assert.Equal(statusCode, log.HttpStatusCode);
			Assert.Equal(shouldBeHealthy, log.ServiceIsHealthy);
		}

		[Fact]
		public void ServiceExecutionLog_Should_Store_Stack_Trace_For_Debugging()
		{
			// Arrange
			var stackTrace = "at HealthMonitorService.CheckHealth()\n   at System.Threading.Tasks.Task.Run()";

			// Act
			var log = new ServiceExecutionLog
			{
				ServiceName = "testservice",
				ExecutionSucceeded = false,
				StackTrace = stackTrace,
				ExceptionType = "System.Exception",
				ErrorMessage = "Test error",
				ExecutionStartedAt = DateTime.UtcNow,
				ExecutionCompletedAt = DateTime.UtcNow,
				DurationMs = 0
			};

			// Assert
			Assert.Equal(stackTrace, log.StackTrace);
			Assert.Equal("System.Exception", log.ExceptionType);
		}

		[Fact]
		public void ServiceExecutionLog_Can_Distinguish_Between_Service_Unhealthy_And_Check_Failed()
		{
			// Scenario 1: Check succeeded, but service is unhealthy
			var log1 = new ServiceExecutionLog
			{
				ServiceName = "service1",
				ExecutionSucceeded = true,  // Check itself succeeded
				ServiceIsHealthy = false,    // But service reported unhealthy
				HttpStatusCode = 503,
				ExecutionStartedAt = DateTime.UtcNow,
				ExecutionCompletedAt = DateTime.UtcNow.AddMilliseconds(100),
				DurationMs = 100
			};

			// Scenario 2: Check failed (couldn't reach service)
			var log2 = new ServiceExecutionLog
			{
				ServiceName = "service2",
				ExecutionSucceeded = false,  // Check itself failed
				ServiceIsHealthy = null,     // Don't know service health
				ErrorMessage = "Connection timeout",
				ExecutionStartedAt = DateTime.UtcNow,
				ExecutionCompletedAt = DateTime.UtcNow.AddSeconds(5),
				DurationMs = 5000
			};

			// Assert
			Assert.True(log1.ExecutionSucceeded);
			Assert.False(log1.ServiceIsHealthy);
			
			Assert.False(log2.ExecutionSucceeded);
			Assert.Null(log2.ServiceIsHealthy);
		}
	}
}

