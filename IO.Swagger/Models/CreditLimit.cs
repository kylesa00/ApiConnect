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
    public partial class CreditLimit : IEquatable<CreditLimit>
    { 
        /// <summary>
        /// Credit that is already used by customer
        /// </summary>
        /// <value>Credit that is already used by customer</value>
        [DataMember(Name="alreadyUsedCredit")]
        public double? AlreadyUsedCredit { get; set; }

        /// <summary>
        /// Available credit of customer
        /// </summary>
        /// <value>Available credit of customer</value>
        [DataMember(Name="availableCredit")]
        public double? AvailableCredit { get; set; }

        /// <summary>
        /// .
        /// </summary>
        /// <value>.</value>
        [DataMember(Name = "alreadyUsedCreditCashPayment")]
        public double? AlreadyUsedCreditCashPayment { get; set; }

        /// <summary>
        /// .
        /// </summary>
        /// <value>.</value>
        [DataMember(Name = "availableCreditCashPayment")]
        public double? AvailableCreditCashPayment { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class CreditLimit {\n");
            sb.Append("  AlreadyUsedCredit: ").Append(AlreadyUsedCredit).Append("\n");
            sb.Append("  AvailableCredit: ").Append(AvailableCredit).Append("\n");
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
            return obj.GetType() == GetType() && Equals((CreditLimit)obj);
        }

        /// <summary>
        /// Returns true if CreditLimit instances are equal
        /// </summary>
        /// <param name="other">Instance of CreditLimit to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(CreditLimit other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return 
                (
                    AlreadyUsedCredit == other.AlreadyUsedCredit ||
                    AlreadyUsedCredit != null &&
                    AlreadyUsedCredit.Equals(other.AlreadyUsedCredit)
                ) && 
                (
                    AvailableCredit == other.AvailableCredit ||
                    AvailableCredit != null &&
                    AvailableCredit.Equals(other.AvailableCredit)
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
                    if (AlreadyUsedCredit != null)
                    hashCode = hashCode * 59 + AlreadyUsedCredit.GetHashCode();
                    if (AvailableCredit != null)
                    hashCode = hashCode * 59 + AvailableCredit.GetHashCode();
                return hashCode;
            }
        }

        #region Operators
        #pragma warning disable 1591

        public static bool operator ==(CreditLimit left, CreditLimit right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(CreditLimit left, CreditLimit right)
        {
            return !Equals(left, right);
        }

        #pragma warning restore 1591
        #endregion Operators
    }
}
