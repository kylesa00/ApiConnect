using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace IO.Swagger.Models
{
    public partial class CourierService : IEquatable<CourierService>
    {
        /// <summary>
        /// The courierServiceCode
        /// </summary>
        /// <value>The courierServiceCode</value>
        [Required]
        [DataMember(Name = "courierServiceCode")]
        public string CourierServiceCode { get; set; }

        /// <summary>
        /// The description
        /// </summary>
        /// <value>The description</value>
        [Required]
        [DataMember(Name = "description")]
        public string Description { get; set; }

        /// <summary>
        /// Returns true if objects are equal
        /// </summary>
        /// <param name="obj">Object to be compared</param>
        /// <returns>Boolean</returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((CourierService)obj);
        }

        /// <summary>
        /// Returns true if CourierService instances are equal
        /// </summary>
        /// <param name="other">Instance of CourierService to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(CourierService other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return
                (
                    CourierServiceCode == other.CourierServiceCode ||
                    CourierServiceCode != null &&
                    CourierServiceCode.SequenceEqual(other.CourierServiceCode)
                ) &&
                (
                    Description == other.Description ||
                    Description != null &&
                    Description.Equals(other.Description)
                );
        }
        public string ToJson()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
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
                // Suitable nullity checks etc, of course :)
                if (CourierServiceCode != null)
                    hashCode = hashCode * 59 + CourierServiceCode.GetHashCode();
                if (Description != null)
                    hashCode = hashCode * 59 + Description.GetHashCode();
                return hashCode;
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class CourierService {\n");
            sb.Append("  CourierServiceCode: ").Append(CourierServiceCode).Append("\n");
            sb.Append("  Description: ").Append(Description).Append("\n");
            sb.Append("}\n");
            return sb.ToString();
        }
    }
}
