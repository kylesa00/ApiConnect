using IO.Swagger.Controllers;
using IO.Swagger.Helpers;
using IO.Swagger.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;

namespace IO.SwaggerTests.Controllers;

[TestClass]
public class ItemMoqPairRoutingTests
{
    [DataTestMethod]
    [DataRow(2, 0, 2d, true)]
    [DataRow(2, 0, 6d, true)]
    [DataRow(2, 0, 3d, false)]
    [DataRow(2, 1, 2d, true)]
    [DataRow(2, 1, 4d, true)]
    [DataRow(2, 1, 6d, true)]
    [DataRow(2, 1, 3d, false)]
    [DataRow(2, 255, 2d, true)]
    [DataRow(3, 1, 3d, true)]
    [DataRow(3, 1, 6d, true)]
    [DataRow(3, 1, 9d, true)]
    [DataRow(1, 1, 3d, true)]
    [DataRow(0, 0, 3d, true)]
    [DataRow(0, 1, 3d, true)]
    [DataRow(0, 1, 4d, true)]
    [DataRow(-1, 0, 4d, false)]
    [DataRow(2, 0, 2.5d, false)]
    [DataRow(2, 0, 0d, false)]
    [DataRow(2, 0, -2d, false)]
    [DataRow(int.MaxValue, 1, 4294967294d, true)]
    public void QuantityMustBeMultipleOfMoqRegardlessOfPair(int moq, int pair, double quantity, bool expected)
    {
        using var cache = CreateCache(moq, (byte)pair);
        Assert.AreEqual(expected, DatabaseCacheService.AreCentralItemQuantitiesValid(
            cache, new[] { Item(quantity) }, out _));
    }

    [TestMethod]
    public void LookupIgnoresCaseAndSurroundingWhitespace()
    {
        using var cache = CreateCache(2, 1);
        var item = Item(4);
        item.ArticleId = " item-1 ";
        Assert.IsTrue(DatabaseCacheService.AreCentralItemQuantitiesValid(cache, new[] { item }, out _));
    }

    [TestMethod]
    public void MissingRuleIsUnrestrictedButMissingCacheIsNot()
    {
        using var cache = CreateCache(2, 1);
        var item = Item(3);
        item.ArticleId = "OTHER";
        Assert.IsTrue(DatabaseCacheService.AreCentralItemQuantitiesValid(cache, new[] { item }, out _));
        cache.Remove(CacheKeys.ItemMoqPair);
        Assert.IsFalse(DatabaseCacheService.AreCentralItemQuantitiesValid(cache, new[] { item }, out var reason));
        Assert.IsTrue(reason.Contains("unavailable"));
    }

    [TestMethod]
    public void EveryLineMustPassWithoutCombiningDuplicateItems()
    {
        using var cache = CreateCache(2, 1);
        Assert.IsFalse(DatabaseCacheService.AreCentralItemQuantitiesValid(
            cache, new[] { Item(4), Item(3) }, out _));
        Assert.IsFalse(DatabaseCacheService.AreCentralItemQuantitiesValid(
            cache, new[] { Item(1), Item(1) }, out _));
    }

    [TestMethod]
    public void InvalidQuantitiesDoNotPass()
    {
        using var cache = CreateCache(1, 0);
        foreach (double? quantity in new double?[] { null, double.NaN, double.PositiveInfinity, double.NegativeInfinity })
            Assert.IsFalse(DatabaseCacheService.AreCentralItemQuantitiesValid(
                cache, new[] { Item(quantity) }, out _));
    }

    [TestMethod]
    public void ExistingBranchRoutingBypassesQuantityCache()
    {
        using var cache = new MemoryCache(new MemoryCacheOptions());
        cache.Set(CacheKeys.OrderRoutingCalendar, new List<OrderRoutingCalendarEntry>
        {
            new() { LocationCode = "BRANCH" }
        });
        var request = new OrderRequest { BranchId = "BRANCH", SendMethod = "OTHER", Items = new() { Item(3) } };
        var controller = new OrderApiController(null!, NullLogger<OrderApiController>.Instance, cache);
        Assert.IsFalse(RouteToCentral(controller, request));
        // Even malformed quantity-cache data must not be touched on the branch path.
        cache.Set(CacheKeys.ItemMoqPair, "not a quantity-rule dictionary");
        Assert.IsFalse(RouteToCentral(controller, request));
    }

    [TestMethod]
    public void DefaultCentralRoutingAlsoChecksQuantities()
    {
        using var cache = CreateCache(2, 1);
        var controller = new OrderApiController(null!, NullLogger<OrderApiController>.Instance, cache);
        var request = new OrderRequest { BranchId = "NO-CALENDAR-RULE", Items = new() { Item(3) } };
        Assert.IsFalse(RouteToCentral(controller, request));
        request.Items[0].Quantity = 2;
        Assert.IsTrue(RouteToCentral(controller, request));
        cache.Remove(CacheKeys.ItemMoqPair);
        Assert.IsFalse(RouteToCentral(controller, request));
    }

    [TestMethod]
    public void CalendarSelectedCentralRoutingAlsoChecksQuantities()
    {
        using var cache = CreateCache(2, 1);
        cache.Set(CacheKeys.OrderRoutingCalendar, new List<OrderRoutingCalendarEntry>
        {
            new()
            {
                LocationCode = "BRANCH", ShipmentMethodCode = "LIČNO",
                FromDay = 0, ToDay = 6, FromTime = DateTime.Today,
                ToTime = DateTime.Today.AddDays(1).AddTicks(-1)
            }
        });
        var controller = new OrderApiController(null!, NullLogger<OrderApiController>.Instance, cache);
        var request = new OrderRequest
        {
            BranchId = "BRANCH", PickupBranchId = "BRANCH", SendMethod = "ABH", Items = new() { Item(3) }
        };
        Assert.IsFalse(RouteToCentral(controller, request));
        request.Items[0].Quantity = 2;
        Assert.IsTrue(RouteToCentral(controller, request));
    }

    private static MemoryCache CreateCache(int moq, byte pair)
    {
        var cache = new MemoryCache(new MemoryCacheOptions());
        cache.Set(CacheKeys.ItemMoqPair, new Dictionary<string, ItemMoqPairCacheEntry>(StringComparer.OrdinalIgnoreCase)
        {
            ["ITEM-1"] = new() { ItemNo = "ITEM-1", CentralMoq = moq, Pair = pair }
        });
        return cache;
    }

    private static OrderRequestItem Item(double? quantity) => new() { ArticleId = "ITEM-1", Quantity = quantity };

    private static bool RouteToCentral(OrderApiController controller, OrderRequest request) =>
        (bool)typeof(OrderApiController).GetMethod("ShouldRouteToCentral", BindingFlags.Instance | BindingFlags.NonPublic)!
            .Invoke(controller, new object[] { request })!;
}
