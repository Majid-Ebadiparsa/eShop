using System;

namespace SharedService.Contracts.Events
{
    public record ServiceHealthChanged(string ServiceName, bool IsHealthy, long ResponseTimeMs, DateTime CheckedAt);
}
