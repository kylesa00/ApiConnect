using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Controllers;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.OpenApi.Models;
using System;

namespace IO.Swagger.Filters
{
    /// <summary>
    /// Path Parameter Validation Rules Filter
    /// </summary>
    public class GeneratePathParamsValidationFilter : IOperationFilter
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="operation">Operation</param>
        /// <param name="context">OperationFilterContext</param>
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var pars = context.ApiDescription.ParameterDescriptions;

            foreach (var par in pars)
            {
                var swaggerParam = operation.Parameters.SingleOrDefault(p => p.Name == par.Name);

                var attributes = ((ControllerParameterDescriptor)par.ParameterDescriptor).ParameterInfo.CustomAttributes;

                if (attributes != null && attributes.Count() > 0 && swaggerParam != null)
                {
                    // Required - [Required]
                    var requiredAttr = attributes.FirstOrDefault(p => p.AttributeType == typeof(RequiredAttribute));
                    if (requiredAttr != null)
                    {
                        swaggerParam.Required = true;
                    }

                    // Regex Pattern [RegularExpression]
                    var regexAttr = attributes.FirstOrDefault(p => p.AttributeType == typeof(RegularExpressionAttribute));
                    if (regexAttr != null)
                    {
                        string regex = (string)regexAttr.ConstructorArguments[0].Value;
                        if (swaggerParam is OpenApiParameter)
                        {
                            ((OpenApiParameter)swaggerParam).Schema.Pattern = regex;
                        }
                    }

                    // String Length [StringLength]
                    int? minLenght = null, maxLength = null;
                    var stringLengthAttr = attributes.FirstOrDefault(p => p.AttributeType == typeof(StringLengthAttribute));
                    if (stringLengthAttr != null)
                    {
                        if (stringLengthAttr.NamedArguments.Count == 1)
                        {
                            minLenght = (int)stringLengthAttr.NamedArguments.Single(p => p.MemberName == "MinimumLength").TypedValue.Value;
                        }
                        maxLength = (int)stringLengthAttr.ConstructorArguments[0].Value;
                    }

                    var minLengthAttr = attributes.FirstOrDefault(p => p.AttributeType == typeof(MinLengthAttribute));
                    if (minLengthAttr != null)
                    {
                        minLenght = (int)minLengthAttr.ConstructorArguments[0].Value;
                    }

                    var maxLengthAttr = attributes.FirstOrDefault(p => p.AttributeType == typeof(MaxLengthAttribute));
                    if (maxLengthAttr != null)
                    {
                        maxLength = (int)maxLengthAttr.ConstructorArguments[0].Value;
                    }

                    if (swaggerParam is OpenApiParameter)
                    {
                        ((OpenApiParameter)swaggerParam).Schema.MinLength = minLenght;
                        ((OpenApiParameter)swaggerParam).Schema.MaxLength = maxLength;
                    }

                    // Range [Range]
                    var rangeAttr = attributes.FirstOrDefault(p => p.AttributeType == typeof(RangeAttribute));
                    if (rangeAttr != null)
                    {
                        int rangeMin = Convert.ToInt32(rangeAttr.ConstructorArguments[0].Value);
                        int rangeMax = int.MaxValue;//Convert.ToInt64(rangeAttr.ConstructorArguments[1].Value);

                        if (swaggerParam is OpenApiParameter)
                        {
                            ((OpenApiParameter)swaggerParam).Schema.Minimum = rangeMin;
                            ((OpenApiParameter)swaggerParam).Schema.Maximum = rangeMax;
                        }
                    }
                }
            }
        }
    }
}

