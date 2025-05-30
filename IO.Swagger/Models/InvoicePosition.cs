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
    public partial class InvoicePosition : IEquatable<InvoicePosition>
    { 
        /// <summary>
        /// The article identification of the invoice position.
        /// </summary>
        /// <value>The article identification of the invoice position.</value>
        [Required]
        [DataMember(Name="articleId")]
        public string ArticleId { get; set; }

        /// <summary>
        /// The amount of articles of the invoice position.
        /// </summary>
        /// <value>The amount of articles of the invoice position.</value>
        [Required]
        [DataMember(Name="quantity")]
        public double? Quantity { get; set; }

        /// <summary>
        /// The corresponding order identification for the invoice position.
        /// </summary>
        /// <value>The corresponding order identification for the invoice position.</value>
        [Required]
        [DataMember(Name="orderNr")]
        public string OrderNr { get; set; }

        /// <summary>
        /// The invoice identification of the invoice position.
        /// </summary>
        /// <value>The invoice identification of the invoice position.</value>
        [DataMember(Name="invoiceNr")]
        public string InvoiceNr { get; set; }

        /// <summary>
        /// The delivery note identification of the invoice position.
        /// </summary>
        /// <value>The delivery note identification of the invoice position.</value>
        [Required]
        [DataMember(Name="deliveryNoteNr")]
        public string DeliveryNoteNr { get; set; }

        /// <summary>
        /// The value of the invoice position.
        /// </summary>
        /// <value>The value of the invoice position.</value>
        [Required]
        [DataMember(Name="amount")]
        public double? Amount { get; set; }

        /// <summary>
        /// The sequence number of the position in the invoice.
        /// </summary>
        /// <value>The sequence number of the position in the invoice.</value>
        [DataMember(Name="sequence")]
        public long? Sequence { get; set; }

        /// <summary>
        /// Map of links which makes URIs to other resources available through symbolic names. The following table lists possible links: +  +  **self (GET)**: The link to invoice representation itself. Following this link returns representation of the very same invoice position representation. +
        /// </summary>
        /// <value>Map of links which makes URIs to other resources available through symbolic names. The following table lists possible links: +  +  **self (GET)**: The link to invoice representation itself. Following this link returns representation of the very same invoice position representation. +</value>
        [DataMember(Name="_links")]
        public Dictionary<string, LinkEntry> Links { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class InvoicePosition {\n");
            sb.Append("  ArticleId: ").Append(ArticleId).Append("\n");
            sb.Append("  Quantity: ").Append(Quantity).Append("\n");
            sb.Append("  OrderNr: ").Append(OrderNr).Append("\n");
            sb.Append("  InvoiceNr: ").Append(InvoiceNr).Append("\n");
            sb.Append("  DeliveryNoteNr: ").Append(DeliveryNoteNr).Append("\n");
            sb.Append("  Amount: ").Append(Amount).Append("\n");
            sb.Append("  Sequence: ").Append(Sequence).Append("\n");
            sb.Append("  Links: ").Append(Links).Append("\n");
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
            return obj.GetType() == GetType() && Equals((InvoicePosition)obj);
        }

        /// <summary>
        /// Returns true if InvoicePosition instances are equal
        /// </summary>
        /// <param name="other">Instance of InvoicePosition to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(InvoicePosition other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return 
                (
                    ArticleId == other.ArticleId ||
                    ArticleId != null &&
                    ArticleId.Equals(other.ArticleId)
                ) && 
                (
                    Quantity == other.Quantity ||
                    Quantity != null &&
                    Quantity.Equals(other.Quantity)
                ) && 
                (
                    OrderNr == other.OrderNr ||
                    OrderNr != null &&
                    OrderNr.Equals(other.OrderNr)
                ) && 
                (
                    InvoiceNr == other.InvoiceNr ||
                    InvoiceNr != null &&
                    InvoiceNr.Equals(other.InvoiceNr)
                ) && 
                (
                    DeliveryNoteNr == other.DeliveryNoteNr ||
                    DeliveryNoteNr != null &&
                    DeliveryNoteNr.Equals(other.DeliveryNoteNr)
                ) && 
                (
                    Amount == other.Amount ||
                    Amount != null &&
                    Amount.Equals(other.Amount)
                ) && 
                (
                    Sequence == other.Sequence ||
                    Sequence != null &&
                    Sequence.Equals(other.Sequence)
                ) && 
                (
                    Links == other.Links ||
                    Links != null &&
                    Links.SequenceEqual(other.Links)
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
                    if (ArticleId != null)
                    hashCode = hashCode * 59 + ArticleId.GetHashCode();
                    if (Quantity != null)
                    hashCode = hashCode * 59 + Quantity.GetHashCode();
                    if (OrderNr != null)
                    hashCode = hashCode * 59 + OrderNr.GetHashCode();
                    if (InvoiceNr != null)
                    hashCode = hashCode * 59 + InvoiceNr.GetHashCode();
                    if (DeliveryNoteNr != null)
                    hashCode = hashCode * 59 + DeliveryNoteNr.GetHashCode();
                    if (Amount != null)
                    hashCode = hashCode * 59 + Amount.GetHashCode();
                    if (Sequence != null)
                    hashCode = hashCode * 59 + Sequence.GetHashCode();
                    if (Links != null)
                    hashCode = hashCode * 59 + Links.GetHashCode();
                return hashCode;
            }
        }

        #region Operators
        #pragma warning disable 1591

        public static bool operator ==(InvoicePosition left, InvoicePosition right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(InvoicePosition left, InvoicePosition right)
        {
            return !Equals(left, right);
        }

        #pragma warning restore 1591
        #endregion Operators
    }
}
