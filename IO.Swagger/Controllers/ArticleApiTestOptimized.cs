/*
 * PERFORMANCE TEST CONTROLLER - WITH SERVER-SIDE TIMING
 */

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using IO.Swagger.Attributes;
using IO.Swagger.Models;
using Microsoft.Data.SqlClient;
using IO.Swagger.Helpers;
using System.Data;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Diagnostics; // ADD THIS

namespace IO.Swagger.Controllers
{
    [ApiController]
    [Route("/apps/prod-webshop-service-app/webshop-service/test-optimized")]
    public class ArticleApiTestOptimizedController : ControllerBase
    {
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = false,
            PropertyNameCaseInsensitive = true
        };

        /// <summary>
        /// OPTIMIZED VERSION with System.Text.Json + SERVER TIMING
        /// </summary>
        [HttpPost]
        [Route("articles/{company}/availabilities")]
        [ValidateModelState]
        [SwaggerOperation("GetAvailabilitiesOptimized")]
        [SwaggerResponse(statusCode: 200, type: typeof(Availabilities), description: "")]
        public virtual async Task<IActionResult> GetAvailabilitiesOptimized(
            [FromRoute][Required] string company, 
            [FromBody] AvailabilityRequest availabilityRequest)
        {
            // START TOTAL TIMER
            var totalTimer = Stopwatch.StartNew();
            var dbTimer = new Stopwatch();
            var mappingTimer = new Stopwatch();
            var serializationTimer = new Stopwatch();

            if (!Companies.IsCompanyExists(company))
            {
                return StatusCode(400, (new ErrorInfo()
                {
                    ErrorOrigin = ErrorInfo.ErrorOriginEnum.WEBSHOPSERVICEEnum,
                    ErrorMessage = "Company not found"
                }));
            }

            DataTable dt = new DataTable();
            dt.Columns.Add(new DataColumn("articleId", typeof(string)) { MaxLength = 50 });
            dt.Columns.Add(new DataColumn("quantity", typeof(double)));
            dt.Columns.Add(new DataColumn("customerNr", typeof(string)) { MaxLength = 50 });
            dt.Columns.Add(new DataColumn("sendMethod", typeof(string)) { MaxLength = 50 });
            dt.Columns.Add(new DataColumn("partialDelivery", typeof(bool)));
            dt.Columns.Add(new DataColumn("deliveryAddressId", typeof(string)) { MaxLength = 50 });
            dt.Columns.Add(new DataColumn("pickupBranchId", typeof(string)) { MaxLength = 50 });
            dt.Columns.Add(new DataColumn("pickingWarehouse", typeof(string)) { MaxLength = 50 });
            dt.Columns.Add(new DataColumn("isTourTimetable", typeof(bool)));

            int itemCount = availabilityRequest.Items.Count;
            dt.MinimumCapacity = itemCount;
            dt.BeginLoadData();

            foreach (AvailabilityRequestItem availabilityRequestItem in availabilityRequest.Items)
            {
                dt.Rows.Add(
                    availabilityRequestItem.ArticleId, 
                    availabilityRequestItem.Quantity ?? 0.0,
                    availabilityRequest.CustomerNr,
                    availabilityRequest.SendMethod, 
                    availabilityRequest.PartialDelivery ?? false,
                    availabilityRequest.DeliveryAddressId, 
                    availabilityRequest.PickupBranchId,
                    availabilityRequest.PickingWarehouse, 
                    availabilityRequest.IsTourTimetable ?? false
                );
            }

            dt.EndLoadData();

            List<SqlParameter> param = new List<SqlParameter>()
            {
                new SqlParameter("@company", company),
                new SqlParameter("@availabilityRequest", dt)
                {
                    TypeName = "dbo.tyAvailabilityRequest"
                }
            };

            List<Availability> availabilities = new List<Availability>(itemCount);

            try
            {
                // TIME DATABASE ACCESS
                dbTimer.Start();
                using (SqlDataReader reader = await DalOptimized.GetDataReaderAsync("GetAvailabilities", param))
                {
                    dbTimer.Stop();

                    if (!reader.HasRows)
                    {
                        return StatusCode(400, (new ErrorInfo()
                        {
                            ErrorOrigin = ErrorInfo.ErrorOriginEnum.WEBSHOPSERVICEEnum,
                            ErrorMessage = "Articles not found"
                        }));
                    }

                    // TIME MAPPING
                    mappingTimer.Start();

                    int articleIdOrd = reader.GetOrdinal("articleId");
                    int quantityOrd = reader.GetOrdinal("quantity");
                    int backOrderOrd = reader.GetOrdinal("backOrder");
                    int cutOffTimeOrd = reader.GetOrdinal("cutOffTime");
                    int deliveryTimeOrd = reader.GetOrdinal("deliveryTime");
                    int immediateDeliveryOrd = reader.GetOrdinal("immediateDelivery");
                    int stockWarehouseOrd = reader.GetOrdinal("stockWarehouse");
                    int deliveryWarehouseOrd = reader.GetOrdinal("deliveryWarehouse");
                    int sendMethodOrd = reader.GetOrdinal("sendMethod");
                    int assignmentPriorityOrd = reader.GetOrdinal("assignmentPriority");
                    int errorMessageOrd = reader.GetOrdinal("errorMessage");
                    int tourNameOrd = reader.GetOrdinal("tourName");
                    int tourTimeTableTourNameOrd = reader.GetOrdinal("tourTimeTableTourName");
                    int tourTimeTableStartTimeOrd = reader.GetOrdinal("tourTimeTableStartTime");

                    while (await reader.ReadAsync())
                    {
                        DateTime? cutOffTimeUtc = null;
                        if (!reader.IsDBNull(cutOffTimeOrd))
                        {
                            var cutOffTemp = reader.GetDateTime(cutOffTimeOrd).ToUniversalTime();
                            cutOffTimeUtc = new DateTime(
                                cutOffTemp.Year, cutOffTemp.Month, cutOffTemp.Day,
                                cutOffTemp.Hour, cutOffTemp.Minute, cutOffTemp.Second,
                                0, DateTimeKind.Utc
                            );
                        }

                        DateTime? deliveryTimeUtc = null;
                        if (!reader.IsDBNull(deliveryTimeOrd))
                        {
                            var deliveryTemp = reader.GetDateTime(deliveryTimeOrd).ToUniversalTime();
                            deliveryTimeUtc = new DateTime(
                                deliveryTemp.Year, deliveryTemp.Month, deliveryTemp.Day,
                                deliveryTemp.Hour, deliveryTemp.Minute, deliveryTemp.Second,
                                0, DateTimeKind.Utc
                            );
                        }

                        DateTime? tourStartTimeUtc = !reader.IsDBNull(tourTimeTableStartTimeOrd)
                            ? reader.GetDateTime(tourTimeTableStartTimeOrd).ToUniversalTime()
                            : (DateTime?)null;

                        Availability availability = new Availability()
                        {
                            ArticleId = DalOptimized.GetStringOrNull(reader, articleIdOrd),
                            Quantity = DalOptimized.GetDoubleOrNull(reader, quantityOrd),
                            BackOrder = DalOptimized.GetBoolOrNull(reader, backOrderOrd),
                            CutOffTime = cutOffTimeUtc,
                            DeliveryTime = deliveryTimeUtc,
                            ImmediateDelivery = DalOptimized.GetBoolOrNull(reader, immediateDeliveryOrd),
                            StockWarehouse = DalOptimized.GetStringOrNull(reader, stockWarehouseOrd),
                            DeliveryWarehouse = DalOptimized.GetStringOrNull(reader, deliveryWarehouseOrd),
                            SendMethod = DalOptimized.GetStringOrNull(reader, sendMethodOrd),
                            AssignmentPriority = DalOptimized.GetInt64OrNull(reader, assignmentPriorityOrd),
                            ErrorMessage = DalOptimized.GetStringOrNull(reader, errorMessageOrd),
                            TourName = DalOptimized.GetStringOrNull(reader, tourNameOrd),
                            TourTimeTable = new List<Tour>()
                            {
                                new Tour()
                                {
                                    TourName = DalOptimized.GetStringOrNull(reader, tourTimeTableTourNameOrd),
                                    StartTime = tourStartTimeUtc
                                }
                            }
                        };
                        availabilities.Add(availability);
                    }

                    mappingTimer.Stop();
                }

                // TIME SERIALIZATION
                serializationTimer.Start();
                var json = System.Text.Json.JsonSerializer.Serialize(
                    new Availabilities() { _Availabilities = availabilities },
                    _jsonOptions
                );
                serializationTimer.Stop();

                totalTimer.Stop();

                // ADD PERFORMANCE HEADERS
                Response.Headers.Append("X-Server-Timing-Total", $"{totalTimer.ElapsedMilliseconds}ms");
                Response.Headers.Append("X-Server-Timing-Database", $"{dbTimer.ElapsedMilliseconds}ms");
                Response.Headers.Append("X-Server-Timing-Mapping", $"{mappingTimer.ElapsedMilliseconds}ms");
                Response.Headers.Append("X-Server-Timing-Serialization", $"{serializationTimer.ElapsedMilliseconds}ms");
                Response.Headers.Append("X-Optimization-Type", "SqlDataReader + System.Text.Json");
                
#if DEBUG
                Response.Headers.Append("X-Build-Configuration", "DEBUG - OPTIMIZATIONS DISABLED!");
#else
                Response.Headers.Append("X-Build-Configuration", "RELEASE");
#endif

                return new ContentResult
                {
                    ContentType = "application/json",
                    StatusCode = 200,
                    Content = json
                };
            }
            catch (Exception ex)
            {
                totalTimer.Stop();
                Response.Headers.Append("X-Server-Timing-Total", $"{totalTimer.ElapsedMilliseconds}ms");
                Response.Headers.Append("X-Error", ex.Message);
                
                return StatusCode(500, new ErrorInfo()
                {
                    ErrorOrigin = ErrorInfo.ErrorOriginEnum.WEBSHOPSERVICEEnum,
                    ErrorMessage = $"Internal server error: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// OPTIMIZED VERSION with Newtonsoft.Json + SERVER TIMING
        /// </summary>
        [HttpPost]
        [Route("articles/{company}/availabilities-newtonsoft")]
        [ValidateModelState]
        [SwaggerOperation("GetAvailabilitiesOptimizedNewtonsoft")]
        [SwaggerResponse(statusCode: 200, type: typeof(Availabilities), description: "")]
        public virtual async Task<IActionResult> GetAvailabilitiesOptimizedNewtonsoft(
            [FromRoute][Required] string company, 
            [FromBody] AvailabilityRequest availabilityRequest)
        {
            var totalTimer = Stopwatch.StartNew();
            var dbTimer = new Stopwatch();
            var mappingTimer = new Stopwatch();

            if (!Companies.IsCompanyExists(company))
            {
                return StatusCode(400, (new ErrorInfo()
                {
                    ErrorOrigin = ErrorInfo.ErrorOriginEnum.WEBSHOPSERVICEEnum,
                    ErrorMessage = "Company not found"
                }));
            }

            DataTable dt = new DataTable();
            dt.Columns.Add(new DataColumn("articleId", typeof(string)) { MaxLength = 50 });
            dt.Columns.Add(new DataColumn("quantity", typeof(double)));
            dt.Columns.Add(new DataColumn("customerNr", typeof(string)) { MaxLength = 50 });
            dt.Columns.Add(new DataColumn("sendMethod", typeof(string)) { MaxLength = 50 });
            dt.Columns.Add(new DataColumn("partialDelivery", typeof(bool)));
            dt.Columns.Add(new DataColumn("deliveryAddressId", typeof(string)) { MaxLength = 50 });
            dt.Columns.Add(new DataColumn("pickupBranchId", typeof(string)) { MaxLength = 50 });
            dt.Columns.Add(new DataColumn("pickingWarehouse", typeof(string)) { MaxLength = 50 });
            dt.Columns.Add(new DataColumn("isTourTimetable", typeof(bool)));

            int itemCount = availabilityRequest.Items.Count;
            dt.MinimumCapacity = itemCount;
            dt.BeginLoadData();

            foreach (AvailabilityRequestItem availabilityRequestItem in availabilityRequest.Items)
            {
                dt.Rows.Add(
                    availabilityRequestItem.ArticleId, 
                    availabilityRequestItem.Quantity ?? 0.0,
                    availabilityRequest.CustomerNr,
                    availabilityRequest.SendMethod, 
                    availabilityRequest.PartialDelivery ?? false,
                    availabilityRequest.DeliveryAddressId, 
                    availabilityRequest.PickupBranchId,
                    availabilityRequest.PickingWarehouse, 
                    availabilityRequest.IsTourTimetable ?? false
                );
            }

            dt.EndLoadData();

            List<SqlParameter> param = new List<SqlParameter>()
            {
                new SqlParameter("@company", company),
                new SqlParameter("@availabilityRequest", dt)
                {
                    TypeName = "dbo.tyAvailabilityRequest"
                }
            };

            List<Availability> availabilities = new List<Availability>(itemCount);

            try
            {
                dbTimer.Start();
                using (SqlDataReader reader = await DalOptimized.GetDataReaderAsync("GetAvailabilities", param))
                {
                    dbTimer.Stop();

                    if (!reader.HasRows)
                    {
                        return StatusCode(400, (new ErrorInfo()
                        {
                            ErrorOrigin = ErrorInfo.ErrorOriginEnum.WEBSHOPSERVICEEnum,
                            ErrorMessage = "Articles not found"
                        }));
                    }

                    mappingTimer.Start();

                    int articleIdOrd = reader.GetOrdinal("articleId");
                    int quantityOrd = reader.GetOrdinal("quantity");
                    int backOrderOrd = reader.GetOrdinal("backOrder");
                    int cutOffTimeOrd = reader.GetOrdinal("cutOffTime");
                    int deliveryTimeOrd = reader.GetOrdinal("deliveryTime");
                    int immediateDeliveryOrd = reader.GetOrdinal("immediateDelivery");
                    int stockWarehouseOrd = reader.GetOrdinal("stockWarehouse");
                    int deliveryWarehouseOrd = reader.GetOrdinal("deliveryWarehouse");
                    int sendMethodOrd = reader.GetOrdinal("sendMethod");
                    int assignmentPriorityOrd = reader.GetOrdinal("assignmentPriority");
                    int errorMessageOrd = reader.GetOrdinal("errorMessage");
                    int tourNameOrd = reader.GetOrdinal("tourName");
                    int tourTimeTableTourNameOrd = reader.GetOrdinal("tourTimeTableTourName");
                    int tourTimeTableStartTimeOrd = reader.GetOrdinal("tourTimeTableStartTime");

                    while (await reader.ReadAsync())
                    {
                        DateTime? cutOffTimeUtc = null;
                        if (!reader.IsDBNull(cutOffTimeOrd))
                        {
                            var cutOffTemp = reader.GetDateTime(cutOffTimeOrd).ToUniversalTime();
                            cutOffTimeUtc = new DateTime(
                                cutOffTemp.Year, cutOffTemp.Month, cutOffTemp.Day,
                                cutOffTemp.Hour, cutOffTemp.Minute, cutOffTemp.Second,
                                0, DateTimeKind.Utc
                            );
                        }

                        DateTime? deliveryTimeUtc = null;
                        if (!reader.IsDBNull(deliveryTimeOrd))
                        {
                            var deliveryTemp = reader.GetDateTime(deliveryTimeOrd).ToUniversalTime();
                            deliveryTimeUtc = new DateTime(
                                deliveryTemp.Year, deliveryTemp.Month, deliveryTemp.Day,
                                deliveryTemp.Hour, deliveryTemp.Minute, deliveryTemp.Second,
                                0, DateTimeKind.Utc
                            );
                        }

                        DateTime? tourStartTimeUtc = !reader.IsDBNull(tourTimeTableStartTimeOrd)
                            ? reader.GetDateTime(tourTimeTableStartTimeOrd).ToUniversalTime()
                            : (DateTime?)null;

                        Availability availability = new Availability()
                        {
                            ArticleId = DalOptimized.GetStringOrNull(reader, articleIdOrd),
                            Quantity = DalOptimized.GetDoubleOrNull(reader, quantityOrd),
                            BackOrder = DalOptimized.GetBoolOrNull(reader, backOrderOrd),
                            CutOffTime = cutOffTimeUtc,
                            DeliveryTime = deliveryTimeUtc,
                            ImmediateDelivery = DalOptimized.GetBoolOrNull(reader, immediateDeliveryOrd),
                            StockWarehouse = DalOptimized.GetStringOrNull(reader, stockWarehouseOrd),
                            DeliveryWarehouse = DalOptimized.GetStringOrNull(reader, deliveryWarehouseOrd),
                            SendMethod = DalOptimized.GetStringOrNull(reader, sendMethodOrd),
                            AssignmentPriority = DalOptimized.GetInt64OrNull(reader, assignmentPriorityOrd),
                            ErrorMessage = DalOptimized.GetStringOrNull(reader, errorMessageOrd),
                            TourName = DalOptimized.GetStringOrNull(reader, tourNameOrd),
                            TourTimeTable = new List<Tour>()
                            {
                                new Tour()
                                {
                                    TourName = DalOptimized.GetStringOrNull(reader, tourTimeTableTourNameOrd),
                                    StartTime = tourStartTimeUtc
                                }
                            }
                        };
                        availabilities.Add(availability);
                    }

                    mappingTimer.Stop();
                }

                totalTimer.Stop();

                Response.Headers.Append("X-Server-Timing-Total", $"{totalTimer.ElapsedMilliseconds}ms");
                Response.Headers.Append("X-Server-Timing-Database", $"{dbTimer.ElapsedMilliseconds}ms");
                Response.Headers.Append("X-Server-Timing-Mapping", $"{mappingTimer.ElapsedMilliseconds}ms");
                Response.Headers.Append("X-Optimization-Type", "SqlDataReader + Newtonsoft.Json");
                
#if DEBUG
                Response.Headers.Append("X-Build-Configuration", "DEBUG");
#else
                Response.Headers.Append("X-Build-Configuration", "RELEASE");
#endif

                return new ObjectResult(new Availabilities() { _Availabilities = availabilities });
            }
            catch (Exception ex)
            {
                totalTimer.Stop();
                Response.Headers.Append("X-Server-Timing-Total", $"{totalTimer.ElapsedMilliseconds}ms");
                
                return StatusCode(500, new ErrorInfo()
                {
                    ErrorOrigin = ErrorInfo.ErrorOriginEnum.WEBSHOPSERVICEEnum,
                    ErrorMessage = $"Internal server error: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// ORIGINAL VERSION + SERVER TIMING
        /// </summary>
        [HttpPost]
        [Route("articles/{company}/availabilities-original")]
        [ValidateModelState]
        [SwaggerOperation("GetAvailabilitiesOriginal")]
        [SwaggerResponse(statusCode: 200, type: typeof(Availabilities), description: "")]
        public virtual async Task<IActionResult> GetAvailabilitiesOriginal(
            [FromRoute][Required] string company, 
            [FromBody] AvailabilityRequest availabilityRequest)
        {
            var totalTimer = Stopwatch.StartNew();
            var dbTimer = new Stopwatch();
            var mappingTimer = new Stopwatch();

            if (!Companies.IsCompanyExists(company))
            {
                return StatusCode(400, (new ErrorInfo()
                {
                    ErrorOrigin = ErrorInfo.ErrorOriginEnum.WEBSHOPSERVICEEnum,
                    ErrorMessage = "Company not found"
                }));
            }

            DataTable dt = new DataTable();
            dt.Columns.Add("articleId", typeof(string));
            dt.Columns.Add("quantity", typeof(double));
            dt.Columns.Add("customerNr", typeof(string));
            dt.Columns.Add("sendMethod", typeof(string));
            dt.Columns.Add("partialDelivery", typeof(bool));
            dt.Columns.Add("deliveryAddressId", typeof(string));
            dt.Columns.Add("pickupBranchId", typeof(string));
            dt.Columns.Add("pickingWarehouse", typeof(string));
            dt.Columns.Add("isTourTimetable", typeof(bool));

            foreach (AvailabilityRequestItem availabilityRequestItem in availabilityRequest.Items)
            {
                dt.Rows.Add(availabilityRequestItem.ArticleId, availabilityRequestItem.Quantity, availabilityRequest.CustomerNr,
                    availabilityRequest.SendMethod, availabilityRequest.PartialDelivery, availabilityRequest.DeliveryAddressId, 
                    availabilityRequest.PickupBranchId, availabilityRequest.PickingWarehouse, availabilityRequest.IsTourTimetable);
            }

            List<SqlParameter> param = new List<SqlParameter>()
            {
                new SqlParameter("@company", company),
                new SqlParameter("@availabilityRequest", dt)
                {
                    TypeName = "dbo.tyAvailabilityRequest"
                }
            };

            List<Availability> availabilities = new List<Availability>();

            try
            {
                dbTimer.Start();
                DataSet ds = await Dal.GetDataAsync("GetAvailabilities", param);
                dbTimer.Stop();

                if (ds.Tables[0].Rows.Count > 0)
                {
                    mappingTimer.Start();
                    
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        Availability availability = new Availability()
                        {
                            ArticleId = dr["articleId"] == DBNull.Value ? null : dr["articleId"].ToString(),
                            Quantity = dr["quantity"] == DBNull.Value ? (double?)null : Convert.ToDouble(dr["quantity"]),
                            BackOrder = dr["backOrder"] == DBNull.Value ? (bool?)null : Convert.ToBoolean(dr["backOrder"]),
                            CutOffTime = dr["cutOffTime"] == DBNull.Value ? (DateTime?)null :
                                new DateTime(
                                    Convert.ToDateTime(dr["cutOffTime"]).ToUniversalTime().Year,
                                    Convert.ToDateTime(dr["cutOffTime"]).ToUniversalTime().Month,
                                    Convert.ToDateTime(dr["cutOffTime"]).ToUniversalTime().Day,
                                    Convert.ToDateTime(dr["cutOffTime"]).ToUniversalTime().Hour,
                                    Convert.ToDateTime(dr["cutOffTime"]).ToUniversalTime().Minute,
                                    Convert.ToDateTime(dr["cutOffTime"]).ToUniversalTime().Second,
                                    0, DateTimeKind.Utc
                                ),
                            DeliveryTime = dr["deliveryTime"] == DBNull.Value ? (DateTime?)null :
                                new DateTime(
                                    Convert.ToDateTime(dr["deliveryTime"]).ToUniversalTime().Year,
                                    Convert.ToDateTime(dr["deliveryTime"]).ToUniversalTime().Month,
                                    Convert.ToDateTime(dr["deliveryTime"]).ToUniversalTime().Day,
                                    Convert.ToDateTime(dr["deliveryTime"]).ToUniversalTime().Hour,
                                    Convert.ToDateTime(dr["deliveryTime"]).ToUniversalTime().Minute,
                                    Convert.ToDateTime(dr["deliveryTime"]).ToUniversalTime().Second,
                                    0, DateTimeKind.Utc
                                ),
                            ImmediateDelivery = dr["immediateDelivery"] == DBNull.Value ? (bool?)null : Convert.ToBoolean(dr["immediateDelivery"]),
                            StockWarehouse = dr["stockWarehouse"] == DBNull.Value ? null : dr["stockWarehouse"].ToString(),
                            DeliveryWarehouse = dr["deliveryWarehouse"] == DBNull.Value ? null : dr["deliveryWarehouse"].ToString(),
                            SendMethod = dr["sendMethod"] == DBNull.Value ? null : dr["sendMethod"].ToString(),
                            AssignmentPriority = dr["assignmentPriority"] == DBNull.Value ? (long?)null : Convert.ToInt64(dr["assignmentPriority"]),
                            ErrorMessage = dr["errorMessage"] == DBNull.Value ? null : dr["errorMessage"].ToString(),
                            TourName = dr["tourName"] == DBNull.Value ? null : dr["tourName"].ToString(),
                            TourTimeTable = new List<Tour>()
                            {
                                new Tour()
                                {
                                    TourName = dr["tourTimeTableTourName"] == DBNull.Value ? null : dr["tourTimeTableTourName"].ToString(),
                                    StartTime = dr["tourTimeTableStartTime"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(dr["tourTimeTableStartTime"]).ToUniversalTime()
                                }
                            }
                        };
                        availabilities.Add(availability);
                    }

                    mappingTimer.Stop();
                    totalTimer.Stop();

                    Response.Headers.Append("X-Server-Timing-Total", $"{totalTimer.ElapsedMilliseconds}ms");
                    Response.Headers.Append("X-Server-Timing-Database", $"{dbTimer.ElapsedMilliseconds}ms");
                    Response.Headers.Append("X-Server-Timing-Mapping", $"{mappingTimer.ElapsedMilliseconds}ms");
                    Response.Headers.Append("X-Optimization-Type", "Original (DataSet + Newtonsoft)");
                    
#if DEBUG
                    Response.Headers.Append("X-Build-Configuration", "DEBUG");
#else
                    Response.Headers.Append("X-Build-Configuration", "RELEASE");
#endif

                    return new ObjectResult(new Availabilities() { _Availabilities = availabilities });
                }
                else
                {
                    totalTimer.Stop();
                    Response.Headers.Append("X-Server-Timing-Total", $"{totalTimer.ElapsedMilliseconds}ms");
                    
                    return StatusCode(400, (new ErrorInfo()
                    {
                        ErrorOrigin = ErrorInfo.ErrorOriginEnum.WEBSHOPSERVICEEnum,
                        ErrorMessage = "Articles not found"
                    }));
                }
            }
            catch (Exception ex)
            {
                totalTimer.Stop();
                Response.Headers.Append("X-Server-Timing-Total", $"{totalTimer.ElapsedMilliseconds}ms");
                
                return StatusCode(500, new ErrorInfo()
                {
                    ErrorOrigin = ErrorInfo.ErrorOriginEnum.WEBSHOPSERVICEEnum,
                    ErrorMessage = $"Internal server error: {ex.Message}"
                });
            }
        }
    }
}
