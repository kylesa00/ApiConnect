using IO.Swagger.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace IO.Swagger.Helpers
{
    /// <summary>
    /// Cache keys used across the application.
    /// </summary>
    public static class CacheKeys
    {
        public const string OrderRoutingCalendar = "cache_order_routing_calendar";
        public const string CustomerAddresses = "cache_customer_addresses";
    }

    /// <summary>
    /// Background service that loads reference data from the database into <see cref="IMemoryCache"/>
    /// at startup and then refreshes it on a fixed interval so the cache never goes stale.
    /// </summary>
    public class DatabaseCacheService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IMemoryCache _cache;
        private readonly ILogger<DatabaseCacheService> _logger;

        // How often the cache is reloaded. Entries are set with no expiration so they
        // remain available between reloads and are simply overwritten on each tick.
        private static readonly TimeSpan RefreshInterval = TimeSpan.FromHours(6);

        // Metadata exposed to the health endpoint.
        public DateTimeOffset? LastRefreshedAt { get; private set; }
        public DateTimeOffset? NextRefreshAt { get; private set; }
        public int OrderRoutingCalendarCount { get; private set; }
        public int CustomerAddressesCount { get; private set; }

        // Prevents a manual refresh from running concurrently with the periodic one.
        private readonly SemaphoreSlim _refreshLock = new SemaphoreSlim(1, 1);

        public DatabaseCacheService(
            IServiceScopeFactory scopeFactory,
            IMemoryCache cache,
            ILogger<DatabaseCacheService> logger)
        {
            _scopeFactory = scopeFactory;
            _cache = cache;
            _logger = logger;
        }

        /// <summary>
        /// Triggers an immediate cache refresh. Safe to call concurrently — if a refresh
        /// is already in progress the caller waits for it to complete instead of starting
        /// a second one.
        /// </summary>
        public async Task RefreshAsync(CancellationToken cancellationToken = default)
        {
            await _refreshLock.WaitAsync(cancellationToken);
            try
            {
                await LoadAllAsync(cancellationToken);
            }
            finally
            {
                _refreshLock.Release();
            }
        }

        public override void Dispose()
        {
            _refreshLock.Dispose();
            base.Dispose();
        }

        // StartAsync (via IHostedLifecycleService) blocks the host from marking the app
        // as "ready" until the cache is fully populated. This guarantees no request
        // is served with an empty cache on a cold start.
        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            await RefreshAsync(cancellationToken);
            await base.StartAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Initial load already done in StartAsync; just run the periodic refresh.
            using var timer = new PeriodicTimer(RefreshInterval);
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await RefreshAsync(stoppingToken);
            }
        }

        private async Task LoadAllAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("DatabaseCacheService: refreshing reference data from database...");

            // Dal is scoped, so create a fresh scope for each reload.
            using var scope = _scopeFactory.CreateScope();
            var dal = scope.ServiceProvider.GetRequiredService<Dal>();

            await LoadOrderRoutingCalendarAsync(dal, cancellationToken);
            await LoadCustomerAddressesAsync(dal, cancellationToken);

            LastRefreshedAt = DateTimeOffset.UtcNow;
            NextRefreshAt = LastRefreshedAt + RefreshInterval;
            _logger.LogInformation("DatabaseCacheService: reference data refreshed successfully.");
        }

        // ------------------------------------------------------------------ //

        //private async Task LoadCompaniesAsync(Dal dal, CancellationToken cancellationToken)
        //{
        //    try
        //    {
        //        DataSet ds = await dal.GetDataAsync("GetCompanies");

        //        var companies = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        //        foreach (DataRow row in ds.Tables[0].Rows)
        //            companies.Add(row["companyName"].ToString());

        //        _cache.Set(CacheKeys.Companies, companies,
        //            new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = CacheDuration });

        //        _logger.LogInformation("DatabaseCacheService: cached {Count} companies.", companies.Count);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "DatabaseCacheService: failed to load companies from database.");
        //    }
        //}

        //private async Task LoadCustApprovalTypesAsync(Dal dal, CancellationToken cancellationToken)
        //{
        //    try
        //    {
        //        DataSet ds = await dal.GetDataAsync("GetCustApprovalTypes");

        //        // Key: customerNr, Value: list of approval type names granted to that customer.
        //        var approvalTypes = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        //        foreach (DataRow row in ds.Tables[0].Rows)
        //        {
        //            string customerNr = row["customerNr"].ToString();
        //            string approvalType = row["approvalTypeName"].ToString();

        //            if (!approvalTypes.TryGetValue(customerNr, out var list))
        //            {
        //                list = new List<string>();
        //                approvalTypes[customerNr] = list;
        //            }
        //            list.Add(approvalType);
        //        }

        //        _cache.Set(CacheKeys.CustApprovalTypes, approvalTypes,
        //            new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = CacheDuration });

        //        _logger.LogInformation("DatabaseCacheService: cached approval types for {Count} customers.", approvalTypes.Count);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "DatabaseCacheService: failed to load customer approval types from database.");
        //    }
        //}

        private async Task LoadOrderRoutingCalendarAsync(Dal dal, CancellationToken cancellationToken)
        {
            try
            {
                DataSet ds = await dal.GetDataRawAsync(
                    "SELECT [Shipment Method Code], [Location Code], [From Day], [From Time]," +
                    "       [To Day], [To Time], [Redirect to Shipment Method]," +
                    "       [Transport Route Code], [Direction]" +
                    " FROM [dbo].[OrderRoutingCalendar]");

                var entries = new List<OrderRoutingCalendarEntry>();
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    entries.Add(new OrderRoutingCalendarEntry
                    {
                        ShipmentMethodCode      = row["Shipment Method Code"].ToString(),
                        LocationCode            = row["Location Code"].ToString(),
                        FromDay                 = Convert.ToInt32(row["From Day"]),
                        FromTime                = Convert.ToDateTime(row["From Time"]),
                        ToDay                   = Convert.ToInt32(row["To Day"]),
                        ToTime                  = Convert.ToDateTime(row["To Time"]),
                        RedirectToShipmentMethod = row["Redirect to Shipment Method"].ToString(),
                        TransportRouteCode      = row["Transport Route Code"].ToString(),
                        Direction               = Convert.ToInt32(row["Direction"])
                    });
                }

                _cache.Set(CacheKeys.OrderRoutingCalendar, entries,
                    new MemoryCacheEntryOptions { Priority = CacheItemPriority.NeverRemove });

                OrderRoutingCalendarCount = entries.Count;

                _logger.LogInformation("DatabaseCacheService: cached {Count} OrderRoutingCalendar entries.", entries.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DatabaseCacheService: failed to load OrderRoutingCalendar from database.");
            }
        }

        private async Task LoadCustomerAddressesAsync(Dal dal, CancellationToken cancellationToken)
        {
            try
            {
                DataSet ds = await dal.GetDataRawAsync(
                    "SELECT [Customer No_], [Code], [TransportRouteCode], [Default], [Primary Location]" +
                    " FROM [dbo].[CustomerAddresses]");

                // Key: customerNo, Value: list of addresses belonging to that customer.
                var addressMap = new Dictionary<string, List<CustomerAddressEntry>>(StringComparer.OrdinalIgnoreCase);
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    var entry = new CustomerAddressEntry
                    {
                        CustomerNo         = row["Customer No_"].ToString(),
                        Code               = row["Code"].ToString(),
                        TransportRouteCode = row["TransportRouteCode"].ToString(),
                        Default            = Convert.ToInt32(row["Default"]),
                        PrimaryLocation    = row["Primary Location"].ToString()
                    };

                    if (!addressMap.TryGetValue(entry.CustomerNo, out var list))
                    {
                        list = new List<CustomerAddressEntry>();
                        addressMap[entry.CustomerNo] = list;
                    }
                    list.Add(entry);
                }

                _cache.Set(CacheKeys.CustomerAddresses, addressMap,
                    new MemoryCacheEntryOptions { Priority = CacheItemPriority.NeverRemove });

                CustomerAddressesCount = addressMap.Count;

                _logger.LogInformation("DatabaseCacheService: cached CustomerAddresses for {Count} customers.", addressMap.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DatabaseCacheService: failed to load CustomerAddresses from database.");
            }
        }

        // ------------------------------------------------------------------ //
        // Helper methods that the rest of the application can call via the
        // injected IMemoryCache instance.
        // ------------------------------------------------------------------ //

        /// <summary>
        /// Returns true when <paramref name="company"/> exists in the cached company list.
        /// Falls back to true (permissive) when the cache entry is missing.
        /// </summary>
        //public static bool IsCompanyValid(IMemoryCache cache, string company)
        //{
        //    if (cache.TryGetValue(CacheKeys.Companies, out HashSet<string> companies))
        //        return companies.Contains(company);

        //    // Cache not yet populated – allow the request through.
        //    return true;
        //}

        ///// <summary>
        ///// Returns the cached approval types for a customer, or an empty list when not found.
        ///// </summary>
        //public static List<string> GetApprovalTypes(IMemoryCache cache, string customerNr)
        //{
        //    if (cache.TryGetValue(CacheKeys.CustApprovalTypes, out Dictionary<string, List<string>> approvalTypes)
        //        && approvalTypes.TryGetValue(customerNr, out var list))
        //        return list;

        //    return new List<string>();
        //}

        /// <summary>
        /// Returns all cached <see cref="OrderRoutingCalendarEntry"/> rows.
        /// Returns an empty list when the cache entry is missing.
        /// </summary>
        public static List<OrderRoutingCalendarEntry> GetOrderRoutingCalendar(IMemoryCache cache)
        {
            if (cache.TryGetValue(CacheKeys.OrderRoutingCalendar, out List<OrderRoutingCalendarEntry> entries))
                return entries;

            return new List<OrderRoutingCalendarEntry>();
        }

        /// <summary>
        /// Returns the routing calendar rows that match the given shipment method and location code.
        /// </summary>
        public static List<OrderRoutingCalendarEntry> GetOrderRoutingCalendarFor(
            IMemoryCache cache, string shipmentMethodCode, string locationCode)
        {
            return GetOrderRoutingCalendar(cache)
                .FindAll(e =>
                    string.Equals(e.ShipmentMethodCode, shipmentMethodCode, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(e.LocationCode, locationCode, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Returns all cached <see cref="CustomerAddressEntry"/> rows for the given customer.
        /// Returns an empty list when the customer has no cached addresses.
        /// </summary>
        public static List<CustomerAddressEntry> GetCustomerAddresses(IMemoryCache cache, string customerNo)
        {
            if (cache.TryGetValue(CacheKeys.CustomerAddresses, out Dictionary<string, List<CustomerAddressEntry>> addressMap)
                && addressMap.TryGetValue(customerNo, out var list))
                return list;

            return new List<CustomerAddressEntry>();
        }

        /// <summary>
        /// Returns the default <see cref="CustomerAddressEntry"/> for the given customer,
        /// or <c>null</c> when none is marked as default.
        /// </summary>
        public static CustomerAddressEntry GetDefaultCustomerAddress(IMemoryCache cache, string customerNo)
        {
            return GetCustomerAddresses(cache, customerNo).Find(a => a.Default == 1);
        }

        /// <summary>
        /// Returns the <see cref="CustomerAddressEntry"/> that matches the given address code,
        /// or <c>null</c> when not found.
        /// </summary>
        public static CustomerAddressEntry GetCustomerAddressByCode(
            IMemoryCache cache, string customerNo, string addressCode)
        {
            return GetCustomerAddresses(cache, customerNo)
                .Find(a => string.Equals(a.Code, addressCode, StringComparison.OrdinalIgnoreCase));
        }
    }
}
