using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace CrudAspireSample.ServiceDefaults
{
    public class FlappingHealthCheck : IHealthCheck
    {
        public Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            // هر 10 ثانیه یکبار وضعیت عوض می‌شود
            var now = DateTime.UtcNow;
            var isHealthy = (now.Second / 10) % 2 == 0; // 0-9 Healthy, 10-19 Unhealthy, ...

            if (isHealthy)
                return Task.FromResult(HealthCheckResult.Healthy("Everything is OK"));
            else
                return Task.FromResult(HealthCheckResult.Unhealthy("Forced unhealthy"));
        }
    }
}
