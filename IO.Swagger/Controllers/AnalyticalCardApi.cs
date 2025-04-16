/*
 * Webshop Service API
 *
 * Webshop services connect Webshop applications to ERP system. The entry point of Webshop API is `/customers/{companyName}/{customerNr}`, which is called by Webshop application whenever a user logs in. If the customer requestedOrderPosition by its number could be found, the response contains a `_links` section, which contains all possible navigations and actions the customer can take.
 *
 */

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using IO.Swagger.Attributes;
using IO.Swagger.Models;
using System.Data.SqlClient;
using System.Linq;
using IO.Swagger.Helpers;
using static IO.Swagger.Models.Order;
using System.Data;
using System.ServiceModel;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.AspNetCore.Http;
using static IO.Swagger.Models.UsedCreditLimit;
using System.Threading.Tasks;

namespace IO.Swagger.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [ApiController]
    public class AnalyticalCardApiController : ControllerBase
    {
        /// <summary>
        /// 
        /// </summary>
        /// <remarks>Retrieves representation of a used credit limit for a customer.</remarks>
        /// <param name="company">The company to which the webshop belongs, e.g.&#39;Derendinger-Switzerland&#39;.</param>
        /// <param name="customerNr">The number of the customer.</param>
        /// <param name="paymentMethod">The payment method.</param>
        /// <response code="200">Successful retrieval of the used credit limit of the customer results in an HTTP status of 200 (OK). </response>
        /// <response code="400">If the provided company name is invalid, the service responds with a 400 (Bad Request) status.</response>
        /// <response code="404">If the customer has no usedcreditlimit or customer does not exist, endpoint responds with a 404 (Not Found) status.</response>
        [HttpGet]
        [Route("/apps/prod-webshop-service-app/webshop-service/usedcreditlimit/{company}/{customerNr}")]
        [ValidateModelState]
        [SwaggerOperation("GetAnalyticalCard_UsedCreditLimit")]
        [SwaggerResponse(statusCode: 200, type: typeof(UsedCreditLimit), description: "Successful retrieval of the used credit limit of the customer results in an HTTP status of 200 (OK). ")]
        [SwaggerResponse(statusCode: 400, type: typeof(ErrorInfo), description: "If the provided company name is invalid, the service responds with a 400 (Bad Request) status.")]
        [SwaggerResponse(statusCode: 404, type: typeof(ErrorInfo), description: "If the customer has no orders or customer does not exist, endpoint responds with a 404 (Not Found) status.")]
        public virtual async Task<IActionResult> GetAnalyticalCard_UsedCreditLimit([FromRoute][Required] string company, [FromRoute][Required] string customerNr, [FromQuery][Required] PaymentMethodAnalyticalCardEnum paymentMethod)
        {
            if (!Companies.IsCompanyExists(company))
            {
                return StatusCode(400, (new ErrorInfo()
                {
                    ErrorOrigin = ErrorInfo.ErrorOriginEnum.WEBSHOPSERVICEEnum,
                    ErrorMessage = "Company not found"
                }));
            }

            List<SqlParameter> param = new List<SqlParameter>()
            {
                new SqlParameter("@company", company),
                new SqlParameter("@customerNr", customerNr),
                new SqlParameter("@paymentMethod", paymentMethod)

            };

            UsedCreditLimit usedCreitLimit = null;

            try
            {
                DataSet ds = await Dal.GetDataAsync("GetAnalyticalCard_UsedCreditLimit", param);
                if (ds.Tables[0].Rows.Count > 0)
                {
                    usedCreitLimit = new UsedCreditLimit()
                    {
                        PostedBalance = Convert.ToDouble(ds.Tables[0].Rows[0]["postedBalance"]),
                        InProcessAmount = Convert.ToDouble(ds.Tables[0].Rows[0]["inProcessAmount"])
                    };
                    return new ObjectResult(usedCreitLimit);
                }
                else
                    return StatusCode(404, (new ErrorInfo()
                    {
                        ErrorOrigin = ErrorInfo.ErrorOriginEnum.WEBSHOPSERVICEEnum,
                        ErrorMessage = "Customer " + customerNr + " has no used credit limit in filter or customer does not exist"
                    }));
            }
            finally
            {
                param = null;
                usedCreitLimit = null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>Retrieves representation of a list of documents for a customer.</remarks>
        /// <param name="company">The company to which the webshop belongs, e.g.&#39;Derendinger-Switzerland&#39;.</param>
        /// <param name="customerNr">The number of the customer.</param>
        /// <param name="paymentMethod">The payment method.</param>
        /// <param name="sorting">Sorting creteria.</param>
        /// <param name="dateFrom">The start date of range for order selection in ISO 8601 format.</param>
        /// <param name="dateTo">The end date of range for order selection in ISO 8601 format.</param>
        /// <param name="status">The status</param>
        /// <param name="page">The number of the page to be retrieved, defaults to the first page. The page parameter should not be provided explicitly, but the usage of &#x60;next&#x60; and &#x60;prev&#x60; links of the response is recommended.</param>
        /// <response code="200">Successful retrieval of the entries of the customer results in an HTTP status of 200 (OK). </response>
        /// <response code="400">If the provided company name is invalid, the service responds with a 400 (Bad Request) status.</response>
        /// <response code="404">If the customer has no entries or customer does not exist, endpoint responds with a 404 (Not Found) status.</response>
        [HttpGet]
        [Route("/apps/prod-webshop-service-app/webshop-service/entries/{company}/{customerNr}")]
        [ValidateModelState]
        [SwaggerOperation("GetAnalyticalCard_Entries")]
        [SwaggerResponse(statusCode: 200, type: typeof(Documents), description: "Successful retrieval of the entries of the customer results in an HTTP status of 200 (OK). ")]
        [SwaggerResponse(statusCode: 400, type: typeof(ErrorInfo), description: "If the provided company name is invalid, the service responds with a 400 (Bad Request) status.")]
        [SwaggerResponse(statusCode: 404, type: typeof(ErrorInfo), description: "If the customer has no entries or customer does not exist, endpoint responds with a 404 (Not Found) status.")]
        public virtual async Task<IActionResult> GetAnalyticalCard_Entries([FromRoute][Required] string company,  [FromRoute][Required] string customerNr, [FromQuery][Required] PaymentMethodAnalyticalCardEnum paymentMethod, [FromQuery][Required] SortingAnalyticalCardEnum sorting, [FromQuery] DateTime? dateFrom, [FromQuery] DateTime? dateTo, [FromQuery] StatusAnalyticalCardEnum status, [FromQuery] int? page)
        {
            if (!Companies.IsCompanyExists(company))
            {
                return StatusCode(400, (new ErrorInfo()
                {
                    ErrorOrigin = ErrorInfo.ErrorOriginEnum.WEBSHOPSERVICEEnum,
                    ErrorMessage = "Company not found"
                }));
            }

            List<Document> documents = new List<Document>();
            List<SqlParameter> param = new List<SqlParameter>()
            {
                new SqlParameter("@company", company),
                new SqlParameter("@customerNr", customerNr),
                new SqlParameter("@paymentMethod", paymentMethod),
                new SqlParameter("@sorting", sorting),
                new SqlParameter("@dateFrom", (object)dateFrom ?? DBNull.Value),
                new SqlParameter("@dateTo", (object)dateTo ?? DBNull.Value),
                new SqlParameter("@status", ((object)status is null || (StatusAnalyticalCardEnum)status == StatusAnalyticalCardEnum.NONE) ? DBNull.Value : (object)status),
                new SqlParameter("@page", page ?? 1)
            };
            Dictionary<string, LinkEntry> links = new Dictionary<string, LinkEntry>() {
                        { "self", new LinkEntry(Request.Path.ToString() + Request.QueryString.ToString()) } };


            try
            {
                DataSet ds = await Dal.GetDataAsync("GetAnalyticalCard_Entries", param);
                if (ds.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        Document document = new Document()
                        {
                            CustomerNr = dr["customerNr"].ToString(),
                            PaymentMethod = (PaymentMethodAnalyticalCardEnum)Convert.ToInt32(dr["paymentMethod"]),
                            DocumentType = (DocumentTypeAnalyticalCardEnum)Convert.ToInt32(dr["documentType"]),
                            DocumentNr = dr["documentNr"].ToString(),
                            WebOrderNr = dr["webOrderNr"].ToString(),
                            PostingDate = Convert.ToDateTime(dr["postingDate"]),
                            DueDate = Convert.ToDateTime(dr["dueDate"]),
                            PaymentDeadlineNotification = dr["paymentDeadlineNotification"].ToString(),
                            Status = (StatusAnalyticalCardEnum)Convert.ToInt32(dr["status"]),
                            RemainingAmount = Convert.ToDouble(dr["remainingAmount"]),
                            DocumentCount = Convert.ToInt32(dr["documentCount"])
                        };
                        documents.Add(document);
                    }

                    

                    if (page != null && page != 1)
                    {
                        var queryString = QueryHelpers.ParseQuery(Request.QueryString.ToString());
                        if (queryString.ContainsKey("page"))
                        {
                            queryString.Remove("page");
                        }
                        queryString.Add("page", (page - 1).ToString());
                        links.Add("prev", new LinkEntry(Request.Path.ToString() + QueryString.Create(queryString).ToString()));
                    }
                    if (ds.Tables[1].Rows.Count > 0 && Convert.ToInt32(ds.Tables[1].Rows[0]["NextPageNumber"]) != 0)
                    {
                        var queryString = QueryHelpers.ParseQuery(Request.QueryString.ToString());
                        if (queryString.ContainsKey("page"))
                        {
                            queryString.Remove("page");
                        }
                        queryString.Add("page", ds.Tables[1].Rows[0]["NextPageNumber"].ToString());
                        links.Add("next", new LinkEntry(Request.Path.ToString() + QueryString.Create(queryString).ToString()));
                    }

                    var queryString4Last = QueryHelpers.ParseQuery(Request.QueryString.ToString());
                    if (queryString4Last.ContainsKey("page"))
                    {
                        queryString4Last.Remove("page");
                    }
                    queryString4Last.Add("page", ds.Tables[1].Rows.Count > 0
                                                    ? ds.Tables[1].Rows[0]["LastPageNumber"].ToString()
                                                    : "1");
                    links.Add("last", new LinkEntry(Request.Path.ToString() + QueryString.Create(queryString4Last).ToString()));

                    return new ObjectResult(new Documents() { _Documents = documents, Links = links });
                }
                else
                    return StatusCode(404, (new ErrorInfo()
                    {
                        ErrorOrigin = ErrorInfo.ErrorOriginEnum.WEBSHOPSERVICEEnum,
                        ErrorMessage = "Customer " + customerNr + " has no entries in filter or customer does not exist"
                    }));
            }
            finally
            {
                param = null;
                documents = null;
                links = null;
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>Retrieves the representation of a particular Entry, which is given by the number of the document.</remarks>
        /// <param name="company">The company to which the webshop belongs, e.g. &#39;Derendinger-Switzerland&#39;.</param>
        /// <param name="customerNr">The number of the customer.</param>
        /// <param name="paymentMethod">The payment method.</param>
        /// <param name="documentType">Document type.</param>
        /// <param name="documentNr">Document number.</param>
        /// <param name="status">The status</param>
        /// <param name="page">The number of the page to be retrieved, defaults to the first page. The page parameter should not be provided explicitly, but the usage of &#x60;next&#x60; and &#x60;prev&#x60; links of the response is recommended.</param>
        /// <response code="200">Succesful response returns the representation of the entry specified by the parameters.</response>
        /// <response code="400">If the provided company name is invalid, the service responds with a 400 (Bad Request) status.</response>
        /// <response code="404">If the entry referenced by number of document does not exist, the service endpoint responds with a 404 (Not Found) status.</response>
        [HttpGet]
        [Route("/apps/prod-webshop-service-app/webshop-service/entries/{company}/{customerNr}/{documentNr}")]
        [ValidateModelState]
        [SwaggerOperation("GetAnalyticalCard_Document")]
        [SwaggerResponse(statusCode: 200, type: typeof(Entry), description: "Succesful response returns the representation of the entry specified by the parameters.")]
        [SwaggerResponse(statusCode: 400, type: typeof(ErrorInfo), description: "If the provided company name is invalid, the service responds with a 400 (Bad Request) status.")]
        [SwaggerResponse(statusCode: 404, type: typeof(ErrorInfo), description: "If the entry referenced by number of document does not exist, the service endpoint responds with a 404 (Not Found) status.")]
        public virtual async Task<IActionResult> GetAnalyticalCard_Document([FromRoute][Required] string company, [FromRoute][Required] string customerNr, [FromRoute][Required] string documentNr, [FromQuery][Required] PaymentMethodAnalyticalCardEnum paymentMethod, [FromQuery][Required] DocumentTypeAnalyticalCardEnum documentType,   [FromQuery][Required] StatusAnalyticalCardEnum status,  [FromQuery] int? page)
        {
            if (!Companies.IsCompanyExists(company))
            {
                return StatusCode(400, (new ErrorInfo()
                {
                    ErrorOrigin = ErrorInfo.ErrorOriginEnum.WEBSHOPSERVICEEnum,
                    ErrorMessage = "Company not found"
                }));
            }

            List<SqlParameter> param = new List<SqlParameter>()
            {
                new SqlParameter("@company", company),
                new SqlParameter("@customerNr", customerNr),
                new SqlParameter("@documentNr", documentNr),
                new SqlParameter("@paymentMethod", paymentMethod),
                new SqlParameter("@documentType", documentType),              
                new SqlParameter("@status", status),
                new SqlParameter("@page", page ?? 1)
            };
            Entry entry = null;
            List<EntryLine> entryLines;
            Dictionary<string, LinkEntry> links = new Dictionary<string, LinkEntry>() {
                     { "self", new LinkEntry(Request.Path.ToString() + Request.QueryString.ToString()) } };
            try
            {
                DataSet ds = await Dal.GetDataAsync("GetAnalyticalCard_Document", param);
                if (ds.Tables[0].Rows.Count > 0)
                {
                    entry = new Entry()
                    {
                        DocumentNr = ds.Tables[0].Rows[0]["documentNr"].ToString(),
                        PostingDate = Convert.ToDateTime(ds.Tables[0].Rows[0]["postingDate"]),
                        DueDate = Convert.ToDateTime(ds.Tables[0].Rows[0]["dueDate"]),
                        Description = ds.Tables[0].Rows[0]["description"].ToString(),
                        ExternalDocumentNr = ds.Tables[0].Rows[0]["externalDocumentNr"].ToString(),
                        Salesperson = ds.Tables[0].Rows[0]["salesperson"].ToString(),
                        RemainingAmount = Convert.ToDouble(ds.Tables[0].Rows[0]["remainingAmount"]),
                        EntryLinesNo = Convert.ToInt32(ds.Tables[0].Rows[0]["entryLinesNo"])
                    };

                    entryLines = new List<EntryLine>();

                    foreach (DataRow dr in ds.Tables[1].Rows)
                    {
                        EntryLine entryLine = new EntryLine()
                        {
                            Sequence = Convert.ToInt32(dr["sequence"]),
                            Type = dr["type"].ToString(),
                            Nr = dr["nr"].ToString(),
                            Description = dr["description"].ToString(),
                            UoM = dr["uoM"].ToString(),
                            Quantity = Convert.ToDouble(dr["quantity"]),
                            UnitPrice = Convert.ToDouble(dr["unitPrice"]),
                            AmountInclVAT = Convert.ToDouble(dr["amountInclVAT"])
                        };
                        entryLines.Add(entryLine);
                    }

                    entry.EntryLines = entryLines;

                    

                    if (page != null && page != 1)
                    {
                        var queryString = QueryHelpers.ParseQuery(Request.QueryString.ToString());
                        if (queryString.ContainsKey("page"))
                        {
                            queryString.Remove("page");
                        }
                        queryString.Add("page", (page - 1).ToString());
                        links.Add("prev", new LinkEntry(Request.Path.ToString() + QueryString.Create(queryString).ToString()));
                    }
                    if (ds.Tables[2].Rows.Count > 0 && Convert.ToInt32(ds.Tables[2].Rows[0]["NextPageNumber"]) != 0)
                    {
                        var queryString = QueryHelpers.ParseQuery(Request.QueryString.ToString());
                        if (queryString.ContainsKey("page"))
                        {
                            queryString.Remove("page");
                        }
                        queryString.Add("page", ds.Tables[2].Rows[0]["NextPageNumber"].ToString());
                        links.Add("next", new LinkEntry(Request.Path.ToString() + QueryString.Create(queryString).ToString()));
                    }


                    var queryString4Last = QueryHelpers.ParseQuery(Request.QueryString.ToString());
                    if (queryString4Last.ContainsKey("page"))
                    {
                        queryString4Last.Remove("page");
                    }
                    queryString4Last.Add("page", ds.Tables[2].Rows.Count > 0
                                                    ? ds.Tables[2].Rows[0]["LastPageNumber"].ToString()
                                                    : "1");
                    links.Add("last", new LinkEntry(Request.Path.ToString() + QueryString.Create(queryString4Last).ToString()));


                    entry.Links = links;

                    return new ObjectResult(entry);
                }
                else
                    return StatusCode(404, (new ErrorInfo()
                    {
                        ErrorOrigin = ErrorInfo.ErrorOriginEnum.WEBSHOPSERVICEEnum,
                        ErrorMessage = "Customer " + customerNr + " has no entries in filter or customer does not exist"
                    }));
            }
            finally
            {
                param = null;
                entry = null;
                entryLines = null;
                links = null;
            }

        }

    }
}
