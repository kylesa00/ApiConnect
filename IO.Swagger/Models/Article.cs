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
    public partial class Article : IEquatable<Article>
    { 
        /// <summary>
        /// Id of the article.
        /// </summary>
        /// <value>Id of the article.</value>
        [Required]
        [DataMember(Name="id")]
        public string Id { get; set; }

        /// <summary>
        /// Description of article.
        /// </summary>
        /// <value>Description of article.</value>
        [DataMember(Name="description")]
        public string Description { get; set; }

        /// <summary>
        /// Number of article.
        /// </summary>
        /// <value>Number of article.</value>
        [DataMember(Name="number")]
        public string Number { get; set; }

        /// <summary>
        /// Search term of article in SAG-sys.
        /// </summary>
        /// <value>Search term of article in SAG-sys.</value>
        [DataMember(Name="keyword")]
        public string Keyword { get; set; }

        /// <summary>
        /// Unit of consumption, i.e. number of units to be sold (SAG speech for &#39;Verbrauchseinheit&#39;).
        /// </summary>
        /// <value>Unit of consumption, i.e. number of units to be sold (SAG speech for &#39;Verbrauchseinheit&#39;).</value>
        [DataMember(Name="salesQuantity")]
        public double? SalesQuantity { get; set; }

        /// <summary>
        /// Leading article id of related depot article, if there is any; null otherwise.
        /// </summary>
        /// <value>Leading article id of related depot article, if there is any; null otherwise.</value>
        [DataMember(Name="depotArticleId")]
        public string DepotArticleId { get; set; }

        /// <summary>
        /// Leading article id of related recycle article,if there is any;null otherwise
        /// </summary>
        /// <value>Leading article id of related recycle article,if there is any;null otherwise</value>
        [DataMember(Name="recycleArticleId")]
        public string RecycleArticleId { get; set; }

        /// <summary>
        /// Leading article id of related voc article, if there is any; null otherwise.
        /// </summary>
        /// <value>Leading article id of related voc article, if there is any; null otherwise.</value>
        [DataMember(Name="vocArticleId")]
        public string VocArticleId { get; set; }

        /// <summary>
        /// Leading article id of related vrg article, if there is any; null otherwise.
        /// </summary>
        /// <value>Leading article id of related vrg article, if there is any; null otherwise.</value>
        [DataMember(Name="vrgArticleId")]
        public string VrgArticleId { get; set; }

        /// <summary>
        /// Indicator for article being locked at an Umsart level or with Absolute ArtikelSperre
        /// </summary>
        /// <value>Indicator for article being locked at an Umsart level or with Absolute ArtikelSperre</value>
        [DataMember(Name="articleLock")]
        public bool? ArticleLock { get; set; }

        /// <summary>
        /// Indicator for article being locked for front-end applications
        /// </summary>
        /// <value>Indicator for article being locked for front-end applications</value>
        [DataMember(Name="fitmentLock")]
        public bool? FitmentLock { get; set; }

        /// <summary>
        /// Map of links, which makes URIs to other resources available through symbolic names. The following table lists possible links: +  +  **self (GET)**: The link to article representation itself. Following this link returns representation of very same article resource.  +  **collection/stock (GET)**: Following this link with a GET request, results in a list of the article stock per branch. +           
        /// </summary>
        /// <value>Map of links, which makes URIs to other resources available through symbolic names. The following table lists possible links: +  +  **self (GET)**: The link to article representation itself. Following this link returns representation of very same article resource.  +  **collection/stock (GET)**: Following this link with a GET request, results in a list of the article stock per branch. +           </value>
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
            sb.Append("class Article {\n");
            sb.Append("  Id: ").Append(Id).Append("\n");
            sb.Append("  Description: ").Append(Description).Append("\n");
            sb.Append("  Number: ").Append(Number).Append("\n");
            sb.Append("  Keyword: ").Append(Keyword).Append("\n");
            sb.Append("  SalesQuantity: ").Append(SalesQuantity).Append("\n");
            sb.Append("  DepotArticleId: ").Append(DepotArticleId).Append("\n");
            sb.Append("  RecycleArticleId: ").Append(RecycleArticleId).Append("\n");
            sb.Append("  VocArticleId: ").Append(VocArticleId).Append("\n");
            sb.Append("  VrgArticleId: ").Append(VrgArticleId).Append("\n");
            sb.Append("  ArticleLock: ").Append(ArticleLock).Append("\n");
            sb.Append("  FitmentLock: ").Append(FitmentLock).Append("\n");
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
            return obj.GetType() == GetType() && Equals((Article)obj);
        }

        /// <summary>
        /// Returns true if Article instances are equal
        /// </summary>
        /// <param name="other">Instance of Article to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(Article other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return 
                (
                    Id == other.Id ||
                    Id != null &&
                    Id.Equals(other.Id)
                ) && 
                (
                    Description == other.Description ||
                    Description != null &&
                    Description.Equals(other.Description)
                ) && 
                (
                    Number == other.Number ||
                    Number != null &&
                    Number.Equals(other.Number)
                ) && 
                (
                    Keyword == other.Keyword ||
                    Keyword != null &&
                    Keyword.Equals(other.Keyword)
                ) && 
                (
                    SalesQuantity == other.SalesQuantity ||
                    SalesQuantity != null &&
                    SalesQuantity.Equals(other.SalesQuantity)
                ) && 
                (
                    DepotArticleId == other.DepotArticleId ||
                    DepotArticleId != null &&
                    DepotArticleId.Equals(other.DepotArticleId)
                ) && 
                (
                    RecycleArticleId == other.RecycleArticleId ||
                    RecycleArticleId != null &&
                    RecycleArticleId.Equals(other.RecycleArticleId)
                ) && 
                (
                    VocArticleId == other.VocArticleId ||
                    VocArticleId != null &&
                    VocArticleId.Equals(other.VocArticleId)
                ) && 
                (
                    VrgArticleId == other.VrgArticleId ||
                    VrgArticleId != null &&
                    VrgArticleId.Equals(other.VrgArticleId)
                ) && 
                (
                    ArticleLock == other.ArticleLock ||
                    ArticleLock != null &&
                    ArticleLock.Equals(other.ArticleLock)
                ) && 
                (
                    FitmentLock == other.FitmentLock ||
                    FitmentLock != null &&
                    FitmentLock.Equals(other.FitmentLock)
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
                    if (Id != null)
                    hashCode = hashCode * 59 + Id.GetHashCode();
                    if (Description != null)
                    hashCode = hashCode * 59 + Description.GetHashCode();
                    if (Number != null)
                    hashCode = hashCode * 59 + Number.GetHashCode();
                    if (Keyword != null)
                    hashCode = hashCode * 59 + Keyword.GetHashCode();
                    if (SalesQuantity != null)
                    hashCode = hashCode * 59 + SalesQuantity.GetHashCode();
                    if (DepotArticleId != null)
                    hashCode = hashCode * 59 + DepotArticleId.GetHashCode();
                    if (RecycleArticleId != null)
                    hashCode = hashCode * 59 + RecycleArticleId.GetHashCode();
                    if (VocArticleId != null)
                    hashCode = hashCode * 59 + VocArticleId.GetHashCode();
                    if (VrgArticleId != null)
                    hashCode = hashCode * 59 + VrgArticleId.GetHashCode();
                    if (ArticleLock != null)
                    hashCode = hashCode * 59 + ArticleLock.GetHashCode();
                    if (FitmentLock != null)
                    hashCode = hashCode * 59 + FitmentLock.GetHashCode();
                    if (Links != null)
                    hashCode = hashCode * 59 + Links.GetHashCode();
                return hashCode;
            }
        }

        #region Operators
        #pragma warning disable 1591

        public static bool operator ==(Article left, Article right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Article left, Article right)
        {
            return !Equals(left, right);
        }

        #pragma warning restore 1591
        #endregion Operators
    }
}
