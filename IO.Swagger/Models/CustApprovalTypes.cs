using System.Runtime.Serialization;

namespace IO.Swagger.Models
{
    public class CustApprovalTypes
    {
        #region dataMembers

        /// <summary>
        /// The customer sagGwsApprovalType_Name
        /// </summary>
        /// <value>The customer sagGwsApprovalType_Name</value>
        [DataMember(Name = "approvalTypeName")]
        public string ApprovalTypeName  {get; set; }

        /// <summary>
        /// The customer certificate
        /// </summary>
        /// <value>The customer certificate</value>
        [DataMember(Name = "certificate")]
        public string Certificate { get; set; }





        #endregion
    }
}
