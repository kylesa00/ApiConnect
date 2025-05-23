/*
 * Webshop Service API
 *
 * Webshop services connect Webshop applications to ERP system. The entry point of Webshop API is `/customers/{companyName}/{customerNr}`, which is called by Webshop application whenever a user logs in. If the customer requestedOrderPosition by its number could be found, the response contains a `_links` section, which contains all possible navigations and actions the customer can take.
 *
 * OpenAPI spec version: 2.0-draftAv
 * 
 * Generated by: https://github.com/swagger-api/swagger-codegen.git
 */

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
    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    public partial class NextWorkingDateRequest : IEquatable<NextWorkingDateRequest>
    { 
        /// <summary>
        /// Id of the warehouse for which the next working date will be calculated
        /// </summary>
        /// <value>Id of the warehouse for which the next working date will be calculated</value>
        [Required]
        [DataMember(Name="branchId")]
        public string BranchId { get; set; }

        /// <summary>
        /// The date from which the calculation for the next working date begins. Format should be ISO8601
        /// </summary>
        /// <value>The date from which the calculation for the next working date begins. Format should be ISO8601</value>
        [Required]
        [DataMember(Name="date")]
        public string Date { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class NextWorkingDateRequest {\n");
            sb.Append("  BranchId: ").Append(BranchId).Append("\n");
            sb.Append("  Date: ").Append(Date).Append("\n");
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
        /// Returns true if objects are equal
        /// </summary>
        /// <param name="obj">Object to be compared</param>
        /// <returns>Boolean</returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((NextWorkingDateRequest)obj);
        }

        /// <summary>
        /// Returns true if NextWorkingDateRequest instances are equal
        /// </summary>
        /// <param name="other">Instance of NextWorkingDateRequest to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(NextWorkingDateRequest other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return 
                (
                    BranchId == other.BranchId ||
                    BranchId != null &&
                    BranchId.Equals(other.BranchId)
                ) && 
                (
                    Date == other.Date ||
                    Date != null &&
                    Date.Equals(other.Date)
                );
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
                    if (BranchId != null)
                    hashCode = hashCode * 59 + BranchId.GetHashCode();
                    if (Date != null)
                    hashCode = hashCode * 59 + Date.GetHashCode();
                return hashCode;
            }
        }

        #region Operators
        #pragma warning disable 1591

        public static bool operator ==(NextWorkingDateRequest left, NextWorkingDateRequest right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(NextWorkingDateRequest left, NextWorkingDateRequest right)
        {
            return !Equals(left, right);
        }

        #pragma warning restore 1591
        #endregion Operators
    }
}
