using HealthMonitorService.Data;
using HealthMonitorService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HealthMonitorService.Controllers
{
    [ApiController]
    [Route("api/health/services")]
    public class HealthController : ControllerBase
    {
        private readonly HealthMonitorDbContext _context;
        private readonly ILogger<HealthController> _logger;
        private readonly SharedService.Caching.Abstractions.IRedisCacheClient _cache;

        private static readonly TimeSpan CacheTtl = TimeSpan.FromSeconds(20);

        public HealthController(HealthMonitorDbContext context, ILogger<HealthController> logger, SharedService.Caching.Abstractions.IRedisCacheClient cache)
        {
            _context = context;
            _logger = logger;
            _cache = cache;
        }

        // GET /api/health/services
        [HttpGet]
        public async Task<ActionResult<List<ServiceStatusDto>>> GetServices()
        {
                // Try cache first
                var cacheKey = "health:services:summary";
                var cached = await _cache.GetAsync<List<ServiceStatusDto>>(cacheKey);
                if (cached is not null)
                {
                    return Ok(cached);
                }

                var statuses = await _context.ServiceHealthStatuses
                    .AsNoTracking()
                    .OrderBy(s => s.ServiceName)
                    .ToListAsync();

                var result = new List<ServiceStatusDto>();

                foreach (var status in statuses)
                {
                    var history = await _context.ServiceHealthHistories
                        .AsNoTracking()
                        .Where(h => h.ServiceName == status.ServiceName)
                        .OrderByDescending(h => h.CheckedAt)
                        .Take(50)
                        .Select(h => new ServiceHealthHistoryDto
                        {
                            IsHealthy = h.IsHealthy,
                            StatusMessage = h.StatusMessage,
                            ResponseTimeMs = h.ResponseTimeMs,
                            CheckedAt = h.CheckedAt
                        })
                        .ToListAsync();

                    result.Add(new ServiceStatusDto
                    {
                        ServiceName = status.ServiceName,
                        IsHealthy = status.IsHealthy,
                        StatusMessage = status.StatusMessage,
                        ResponseTimeMs = status.ResponseTimeMs,
                        LastChecked = status.CheckedAt,
                        History = history
                    });
                }

                // Store in cache with explicit TTL
                await _cache.SetAsync(cacheKey, result, CacheTtl);

                return Ok(result);
        }

        // GET /api/health/services/{serviceName}/history
        [HttpGet("{serviceName}/history")]
        public async Task<ActionResult<List<ServiceHealthHistoryDto>>> GetServiceHistory(string serviceName)
        {
            if (string.IsNullOrWhiteSpace(serviceName))
                return BadRequest("serviceName is required");

            var key = serviceName.ToLowerInvariant();

            var exists = await _context.ServiceHealthStatuses
                .AsNoTracking()
                .AnyAsync(s => s.ServiceName == key);

            if (!exists)
                return NotFound();

            var history = await _context.ServiceHealthHistories
                .AsNoTracking()
                .Where(h => h.ServiceName == key)
                .OrderByDescending(h => h.CheckedAt)
                .Take(1000)
                .Select(h => new ServiceHealthHistoryDto
                {
                    IsHealthy = h.IsHealthy,
                    StatusMessage = h.StatusMessage,
                    ResponseTimeMs = h.ResponseTimeMs,
                    CheckedAt = h.CheckedAt
                })
                .ToListAsync();

            return Ok(history);
        }
    }
}
