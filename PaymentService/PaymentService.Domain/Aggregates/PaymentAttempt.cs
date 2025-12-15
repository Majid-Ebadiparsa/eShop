using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaymentService.Domain.Aggregates
{
	public sealed record PaymentAttempt
	{
		public Guid Id { get; init; } = Guid.NewGuid();
		public DateTime At { get; init; } = DateTime.UtcNow;
		public string Operation { get; init; } = "";
		public bool Succeeded { get; init; }
		public string CodeOrReason { get; init; } = "";

		public static PaymentAttempt Success(string op, string code)
				=> new() { Operation = op, Succeeded = true, CodeOrReason = code };
		public static PaymentAttempt Fail(string op, string reason)
				=> new() { Operation = op, Succeeded = false, CodeOrReason = reason };
	}
}
