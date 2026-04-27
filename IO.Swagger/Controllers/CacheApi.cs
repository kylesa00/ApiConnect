using IO.Swagger.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace IO.Swagger.Controllers
{
    /// <summary>
    /// Endpoints for manually managing and inspecting the in-memory reference data cache.
    /// </summary>
    [ApiController]
    public class CacheApiController : ControllerBase
    {
        private readonly DatabaseCacheService _cacheService;
        private readonly IMemoryCache _cache;
        private readonly ILogger<CacheApiController> _logger;

        public CacheApiController(
            DatabaseCacheService cacheService,
            IMemoryCache cache,
            ILogger<CacheApiController> logger)
        {
            _cacheService = cacheService;
            _cache = cache;
            _logger = logger;
        }

        /// <summary>
        /// Triggers an immediate reload of all reference data from the database into the cache.
        /// Safe to call at any time — ongoing requests continue to read the current cached data
        /// until the new data is atomically swapped in.
        /// </summary>
        /// <response code="200">Cache refreshed successfully.</response>
        /// <response code="500">Refresh failed — check application logs for details.</response>
        [HttpPost]
        [Route("/apps/prod-webshop-service-app/webshop-service/cache/refresh")]
        [SwaggerOperation("RefreshCache")]
        [SwaggerResponse(statusCode: 200, description: "Cache refreshed successfully.")]
        [SwaggerResponse(statusCode: 500, description: "Refresh failed.")]
        public async Task<IActionResult> RefreshCache(CancellationToken cancellationToken)
        {
            _logger.LogInformation("CacheApiController: manual cache refresh requested.");
            try
            {
                await _cacheService.RefreshAsync(cancellationToken);
                _logger.LogInformation("CacheApiController: manual cache refresh completed.");
                return Ok(new
                {
                    message = "Cache refreshed successfully.",
                    refreshedAt = _cacheService.LastRefreshedAt,
                    nextScheduledRefresh = _cacheService.NextRefreshAt
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CacheApiController: manual cache refresh failed.");
                return StatusCode(500, new { message = "Cache refresh failed. See application logs for details." });
            }
        }

        /// <summary>
        /// Returns the current health and statistics of the in-memory reference data cache.
        /// </summary>
        /// <response code="200">Cache health information returned.</response>
        [HttpGet]
        [Route("/apps/prod-webshop-service-app/webshop-service/cache/health")]
        [SwaggerOperation("GetCacheHealth")]
        [SwaggerResponse(statusCode: 200, description: "Cache health information returned.")]
        public IActionResult GetCacheHealth()
        {
            bool calendarLoaded = _cache.TryGetValue(CacheKeys.OrderRoutingCalendar, out _);
            bool addressesLoaded = _cache.TryGetValue(CacheKeys.CustomerAddresses, out _);

            var status = new
            {
                status = calendarLoaded && addressesLoaded ? "healthy" : "degraded",
                lastRefreshedAt = _cacheService.LastRefreshedAt,
                nextScheduledRefresh = _cacheService.NextRefreshAt,
                entries = new
                {
                    orderRoutingCalendar = new
                    {
                        loaded = calendarLoaded,
                        count = _cacheService.OrderRoutingCalendarCount
                    },
                    customerAddresses = new
                    {
                        loaded = addressesLoaded,
                        count = _cacheService.CustomerAddressesCount
                    }
                }
            };

            return Ok(status);
        }
    }
}
