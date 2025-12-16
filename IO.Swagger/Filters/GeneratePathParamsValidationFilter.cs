using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Controllers;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
    using Microsoft.OpenApi;
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
                        var param = swaggerParam as OpenApiParameter;
                        if (param != null)
                        {
                            var newParam = new OpenApiParameter
                            {
                                Name = param.Name,
                                In = param.In,
                                Description = param.Description,
                                Required = true, // Set required to true
                                Deprecated = param.Deprecated,
                                AllowEmptyValue = param.AllowEmptyValue,
                                Style = param.Style,
                                Explode = param.Explode,
                                AllowReserved = param.AllowReserved,
                                Schema = param.Schema,
                                Examples = param.Examples,
                                Example = param.Example,
                                Content = param.Content
                            };
                            
                            var index = operation.Parameters.IndexOf(swaggerParam);
                            operation.Parameters[index] = newParam;
                            swaggerParam = newParam;
                        }
                    }

                    // Regex Pattern [RegularExpression]
                    var regexAttr = attributes.FirstOrDefault(p => p.AttributeType == typeof(RegularExpressionAttribute));
                    if (regexAttr != null)
                    {
                        string regex = (string)regexAttr.ConstructorArguments[0].Value;
                        var param = swaggerParam as OpenApiParameter;
                        if (param != null && param.Schema != null)
                        {
                            var newSchema = new OpenApiSchema
                            {
                                Type = param.Schema.Type,
                                Format = param.Schema.Format,
                                Pattern = regex, // Set pattern
                                MinLength = param.Schema.MinLength,
                                MaxLength = param.Schema.MaxLength,
                                Minimum = param.Schema.Minimum,
                                Maximum = param.Schema.Maximum,
                                Default = param.Schema.Default,
                                Description = param.Schema.Description
                            };
                            
                            var newParam = new OpenApiParameter
                            {
                                Name = param.Name,
                                In = param.In,
                                Description = param.Description,
                                Required = param.Required,
                                Deprecated = param.Deprecated,
                                AllowEmptyValue = param.AllowEmptyValue,
                                Style = param.Style,
                                Explode = param.Explode,
                                AllowReserved = param.AllowReserved,
                                Schema = newSchema,
                                Examples = param.Examples,
                                Example = param.Example,
                                Content = param.Content
                            };
                            
                            var index = operation.Parameters.IndexOf(swaggerParam);
                            operation.Parameters[index] = newParam;
                            swaggerParam = newParam;
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

                    if (minLenght.HasValue || maxLength.HasValue)
                    {
                        var param = swaggerParam as OpenApiParameter;
                        if (param != null && param.Schema != null)
                        {
                            var newSchema = new OpenApiSchema
                            {
                                Type = param.Schema.Type,
                                Format = param.Schema.Format,
                                Pattern = param.Schema.Pattern,
                                MinLength = minLenght ?? param.Schema.MinLength, // Set min length
                                MaxLength = maxLength ?? param.Schema.MaxLength, // Set max length
                                Minimum = param.Schema.Minimum,
                                Maximum = param.Schema.Maximum,
                                Default = param.Schema.Default,
                                Description = param.Schema.Description
                            };
                            
                            var newParam = new OpenApiParameter
                            {
                                Name = param.Name,
                                In = param.In,
                                Description = param.Description,
                                Required = param.Required,
                                Deprecated = param.Deprecated,
                                AllowEmptyValue = param.AllowEmptyValue,
                                Style = param.Style,
                                Explode = param.Explode,
                                AllowReserved = param.AllowReserved,
                                Schema = newSchema,
                                Examples = param.Examples,
                                Example = param.Example,
                                Content = param.Content
                            };
                            
                            var index = operation.Parameters.IndexOf(swaggerParam);
                            operation.Parameters[index] = newParam;
                            swaggerParam = newParam;
                        }
                    }

                    // Range [Range]
                    var rangeAttr = attributes.FirstOrDefault(p => p.AttributeType == typeof(RangeAttribute));
                    if (rangeAttr != null)
                    {
                        int rangeMin = Convert.ToInt32(rangeAttr.ConstructorArguments[0].Value);
                        int rangeMax = int.MaxValue;

                        var param = swaggerParam as OpenApiParameter;
                        if (param != null && param.Schema != null)
                        {
                            var newSchema = new OpenApiSchema
                            {
                                Type = param.Schema.Type,
                                Format = param.Schema.Format,
                                Pattern = param.Schema.Pattern,
                                MinLength = param.Schema.MinLength,
                                MaxLength = param.Schema.MaxLength,
                                Minimum = rangeMin.ToString(), // Set minimum
                                Maximum = rangeMax.ToString(), // Set maximum
                                Default = param.Schema.Default,
                                Description = param.Schema.Description
                            };
                            
                            var newParam = new OpenApiParameter
                            {
                                Name = param.Name,
                                In = param.In,
                                Description = param.Description,
                                Required = param.Required,
                                Deprecated = param.Deprecated,
                                AllowEmptyValue = param.AllowEmptyValue,
                                Style = param.Style,
                                Explode = param.Explode,
                                AllowReserved = param.AllowReserved,
                                Schema = newSchema,
                                Examples = param.Examples,
                                Example = param.Example,
                                Content = param.Content
                            };
                            
                            var index = operation.Parameters.IndexOf(swaggerParam);
                            operation.Parameters[index] = newParam;
                        }
                    }
                }
            }
        }
    }
}

