using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceSubscriber.ConsumerTests.Fakes
{
	public sealed class TestLogger<T> : ILogger<T>
	{
		public readonly List<(LogLevel Level, EventId EventId, string Message, Exception Exception)> Entries
				= new();

		private sealed class NoopScope : IDisposable { public void Dispose() { } }
		private static readonly IDisposable _noop = new NoopScope();

		public IDisposable BeginScope<TState>(TState state) => _noop;

		public bool IsEnabled(LogLevel logLevel) => true;

		public void Log<TState>(LogLevel logLevel, EventId eventId,
				TState state, Exception exception, Func<TState, Exception, string> formatter)
		{
			var message = formatter != null ? formatter(state, exception) : state?.ToString();
			Entries.Add((logLevel, eventId, message ?? string.Empty, exception));
		}
	}
}
