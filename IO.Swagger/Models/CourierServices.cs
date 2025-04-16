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
    [DataContract]
    public partial class CourierServices : IEquatable<CourierServices>
    {
        /// <summary>
        /// The list of CourierServices is embedded in this resource representation.
        /// </summary>
        /// <value>The list of CourierServices is embedded in this resource representation.</value>
        [Required]
        [DataMember(Name = "courierServices")]
        public List<CourierService> _CourierServices { get; set; }

        /// <summary>
        /// Returns true if objects are equal
        /// </summary>
        /// <param name="obj">Object to be compared</param>
        /// <returns>Boolean</returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((CourierServices)obj);
        }

        /// <summary>
        /// Returns true if _CourierServices instances are equal
        /// </summary>
        /// <param name="other">Instance of _CourierServices to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(CourierServices other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return
                (
                    _CourierServices == other._CourierServices ||
                    _CourierServices != null &&
                    _CourierServices.SequenceEqual(other._CourierServices)
                );
        }
        public string ToJson()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }

        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                var hashCode = 41;
                // Suitable nullity checks etc, of course :)
                if (_CourierServices != null)
                    hashCode = hashCode * 59 + _CourierServices.GetHashCode();
                return hashCode;
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class CourierServices {\n");
            sb.Append("  _CourierServices: ").Append(_CourierServices).Append("\n");
            sb.Append("}\n");
            return sb.ToString();
        }
    }
}
