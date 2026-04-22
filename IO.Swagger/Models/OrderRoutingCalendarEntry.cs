using System;
using System.Runtime.Serialization;

namespace IO.Swagger.Models
{
    /// <summary>
    /// Represents a single row from the [dbo].[OrderRoutingCalendar] table.
    /// </summary>
    [DataContract]
    public class OrderRoutingCalendarEntry
    {
        /// <summary>The shipment method code (e.g. "TOUR", "PICKUP").</summary>
        [DataMember(Name = "shipmentMethodCode")]
        public string ShipmentMethodCode { get; set; }

        /// <summary>The warehouse / location code the rule applies to.</summary>
        [DataMember(Name = "locationCode")]
        public string LocationCode { get; set; }

        /// <summary>Day-of-week the time window starts (1 = Monday … 7 = Sunday, matching NAV convention).</summary>
        [DataMember(Name = "fromDay")]
        public int FromDay { get; set; }

        /// <summary>Exact date-time the window opens (only the time-of-day portion is relevant).</summary>
        [DataMember(Name = "fromTime")]
        public DateTime FromTime { get; set; }

        /// <summary>Day-of-week the time window ends.</summary>
        [DataMember(Name = "toDay")]
        public int ToDay { get; set; }

        /// <summary>Exact date-time the window closes (only the time-of-day portion is relevant).</summary>
        [DataMember(Name = "toTime")]
        public DateTime ToTime { get; set; }

        /// <summary>The shipment method code to redirect to when the window is missed.</summary>
        [DataMember(Name = "redirectToShipmentMethod")]
        public string RedirectToShipmentMethod { get; set; }

        /// <summary>The transport route code this rule belongs to.</summary>
        [DataMember(Name = "transportRouteCode")]
        public string TransportRouteCode { get; set; }

        /// <summary>Routing direction flag as stored in NAV.</summary>
        [DataMember(Name = "direction")]
        public int Direction { get; set; }
    }
}
