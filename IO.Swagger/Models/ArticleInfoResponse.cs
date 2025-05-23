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
    public partial class ArticleInfoResponse : IEquatable<ArticleInfoResponse>
    { 
        /// <summary>
        /// List of results of articles.
        /// </summary>
        /// <value>List of results of articles.</value>
        [DataMember(Name="articles")]
        public List<ArticleInfo> Articles { get; set; }

        /// <summary>
        /// Map of links, which makes URIs to other resources available through symbolic names. The following table lists possible mappings: +  +  **self (GET)**: The link to the article response representation itself. Following this link returns the very same response representation.  +  **collection/stocks (GET)**: Following this link with a GET request, results in a list of articles stocks per branch. +  +  **collection/stocks/sum (GET)**: Following this link with a GET request, results in a list of articles sum of stocks for the requested country. +            
        /// </summary>
        /// <value>Map of links, which makes URIs to other resources available through symbolic names. The following table lists possible mappings: +  +  **self (GET)**: The link to the article response representation itself. Following this link returns the very same response representation.  +  **collection/stocks (GET)**: Following this link with a GET request, results in a list of articles stocks per branch. +  +  **collection/stocks/sum (GET)**: Following this link with a GET request, results in a list of articles sum of stocks for the requested country. +            </value>
        [Required]
        [DataMember(Name="_links")]
        public Dictionary<string, LinkEntry> Links { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class ArticleInfoResponse {\n");
            sb.Append("  Articles: ").Append(Articles).Append("\n");
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
            return obj.GetType() == GetType() && Equals((ArticleInfoResponse)obj);
        }

        /// <summary>
        /// Returns true if ArticleInfoResponse instances are equal
        /// </summary>
        /// <param name="other">Instance of ArticleInfoResponse to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(ArticleInfoResponse other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return 
                (
                    Articles == other.Articles ||
                    Articles != null &&
                    Articles.SequenceEqual(other.Articles)
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
                    if (Articles != null)
                    hashCode = hashCode * 59 + Articles.GetHashCode();
                    if (Links != null)
                    hashCode = hashCode * 59 + Links.GetHashCode();
                return hashCode;
            }
        }

        #region Operators
        #pragma warning disable 1591

        public static bool operator ==(ArticleInfoResponse left, ArticleInfoResponse right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ArticleInfoResponse left, ArticleInfoResponse right)
        {
            return !Equals(left, right);
        }

        #pragma warning restore 1591
        #endregion Operators
    }
}
