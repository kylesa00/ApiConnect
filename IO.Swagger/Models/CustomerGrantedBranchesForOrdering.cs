
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
    public class CustomerGrantedBranchesForOrdering //: IEquatable<CustomerGrantedBranchesForOrdering>
    { 
        /// <summary>
        /// .
        /// </summary>
        /// <value>.</value>
        [DataMember(Name= "branchId")]
        public string BranchId { get; set; }

        /// <summary>
        /// .
        /// </summary>
        /// <value>.</value>
        [DataMember(Name= "orderingPriority")]
        public int OrderingPriority { get; set; }

        /// <summary>
        /// .
        /// </summary>
        /// <value>.</value>
        [DataMember(Name = "paymentMethodAllowed")]
        public PaymentMethodAllowedEnum? PaymentMethodAllowed { get; set; }

        /// <summary>
        /// .
        /// </summary>
        /// <value>.</value>
        [JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
        public enum PaymentMethodAllowedEnum
        {
            /// <summary>
            /// Enum NONEEnum for NONE
            /// </summary>
            [EnumMember(Value = "NONE")]
            NONEEnum = 0,

            /// <summary>
            /// Enum WholesaleOnlyEnum for Wholesale
            /// </summary>
            [EnumMember(Value = "Wholesale")]
            WholesaleOnlyEnum = 1,

            /// <summary>
            /// Enum CashPaymentOnlyEnum for Cash Payment
            /// </summary>
            [EnumMember(Value = "Cash Payment")]
            CashPaymentOnlyEnum = 2,

            /// <summary>
            /// Enum WholesaleAndCashPaymentEnum for Wholesale and Cash Payment
            /// </summary>
            [EnumMember(Value = "Wholesale and Cash Payment")]
            WholesaleAndCashPaymentEnum = 3,

        }

        /// <summary>
        /// .
        /// </summary>
        /// <value>.</value>
        [DataMember(Name = "externalOrderingLocationType")]
        public string ExternalOrderingLocationType { get; set; }

    }
}
