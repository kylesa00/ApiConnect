using System.Runtime.Serialization;

namespace IO.Swagger.Models
{
    /// <summary>
    /// Represents a single row from the [dbo].[CustomerAddresses] table.
    /// </summary>
    [DataContract]
    public class CustomerAddressEntry
    {
        /// <summary>The customer number this address belongs to.</summary>
        [DataMember(Name = "customerNo")]
        public string CustomerNo { get; set; }

        /// <summary>The address code (short identifier for this address).</summary>
        [DataMember(Name = "code")]
        public string Code { get; set; }

        /// <summary>The transport route code associated with this address.</summary>
        [DataMember(Name = "transportRouteCode")]
        public string TransportRouteCode { get; set; }

        /// <summary>Whether this is the default address for the customer (1 = default, 0 = not default).</summary>
        [DataMember(Name = "isDefault")]
        public int Default { get; set; }

        /// <summary>The primary warehouse / location code linked to this address.</summary>
        [DataMember(Name = "primaryLocation")]
        public string PrimaryLocation { get; set; }
    }
}
