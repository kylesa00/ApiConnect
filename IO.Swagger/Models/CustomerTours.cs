using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;

namespace IO.Swagger.Models
{
    /// <summary>
    /// Represents a collection of customer tours.
    /// </summary>
    [DataContract]
    public partial class CustomerTours : IEquatable<CustomerTours>
    {
        /// <summary>
        /// The list of customer tour details.
        /// </summary>
        /// <value>The list of customer tour details.</value>
        [Required]
        [DataMember(Name = "customerTours")]
        public List<CustomerTour> Tours { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class CustomerTours {\n");
            sb.Append("  Tours: ").Append(Tours).Append("\n");
            sb.Append("}\n");
            return sb.ToString();
        }

        /// <summary>
        /// Returns the JSON string presentation of the object
        /// </summary>
        /// <returns>JSON string presentation of the object</returns>
        public string ToJson()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }

        /// <summary>
        /// Returns true if CustomerTours instances are equal
        /// </summary>
        /// <param name="other">Instance of CustomerTours to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(CustomerTours other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return 
                (
                    Tours == other.Tours ||
                    Tours != null && other.Tours != null && Tours.SequenceEqual(other.Tours)
                );
        }

        /// <summary>
        /// Returns true if objects are equal
        /// </summary>
        /// <param name="obj">Object to be compared</param>
        /// <returns>Boolean</returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((CustomerTours)obj);
        }

        /// <summary>
        /// Gets the hash code
        /// </summary>
        /// <returns>Hash code</returns>
        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                var hashCode = 41;
                if (Tours != null)
                    hashCode = hashCode * 59 + Tours.GetHashCode();
                return hashCode;
            }
        }

        #region Operators
        #pragma warning disable 1591

        public static bool operator ==(CustomerTours left, CustomerTours right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(CustomerTours left, CustomerTours right)
        {
            return !Equals(left, right);
        }

        #pragma warning restore 1591
        #endregion Operators
    }
}
