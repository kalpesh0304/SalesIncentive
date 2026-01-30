using System.Reflection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Dorise.Incentive.Api.Documentation;

/// <summary>
/// Extension methods for configuring Swagger/OpenAPI documentation.
/// "I'm learnding!" - Learning about our API through documentation!
/// </summary>
public static class SwaggerConfiguration
{
    /// <summary>
    /// Adds Swagger documentation services.
    /// </summary>
    public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            // API Information
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Dorise Sales Incentive Framework API",
                Version = "v1",
                Description = @"
## Overview
The **Dorise Sales Incentive Framework (DSIF)** API provides comprehensive endpoints for managing sales incentive calculations, employee data, approval workflows, and system administration.

## Authentication
This API uses **JWT Bearer tokens** for authentication. Include the token in the `Authorization` header:
```
Authorization: Bearer {your-token}
```

## Rate Limiting
- Standard endpoints: 100 requests/minute
- Bulk operations: 10 requests/minute
- Export operations: 5 requests/minute

## Versioning
API versioning is supported via URL path (e.g., `/api/v1/employees`).

## Error Handling
All errors follow the RFC 7807 Problem Details specification.

---
*""Me fail English? That's unpossible!"" - Our API documentation makes understanding unpossibly easy!*
",
                Contact = new OpenApiContact
                {
                    Name = "DSIF Support",
                    Email = "support@dorise.com",
                    Url = new Uri("https://dorise.com/support")
                },
                License = new OpenApiLicense
                {
                    Name = "Proprietary",
                    Url = new Uri("https://dorise.com/license")
                },
                TermsOfService = new Uri("https://dorise.com/terms")
            });

            // Security Definition
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = JwtBearerDefaults.AuthenticationScheme,
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = @"
JWT Authorization header using the Bearer scheme.

Enter your token in the text input below.

Example: `eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...`"
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });

            // XML Comments
            var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
            if (File.Exists(xmlPath))
            {
                options.IncludeXmlComments(xmlPath);
            }

            // Operation Filters
            options.OperationFilter<AuthorizeCheckOperationFilter>();
            options.OperationFilter<ApiResponsesOperationFilter>();
            options.OperationFilter<CorrelationIdOperationFilter>();

            // Schema Filters
            options.SchemaFilter<EnumSchemaFilter>();

            // Document Filters
            options.DocumentFilter<TagDescriptionDocumentFilter>();

            // Custom Schema IDs
            options.CustomSchemaIds(type => type.FullName?.Replace("+", "."));

            // Enable annotations
            options.EnableAnnotations();
        });

        return services;
    }

    /// <summary>
    /// Configures Swagger UI middleware.
    /// </summary>
    public static IApplicationBuilder UseSwaggerDocumentation(this IApplicationBuilder app)
    {
        app.UseSwagger(options =>
        {
            options.RouteTemplate = "api-docs/{documentName}/swagger.json";
        });

        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/api-docs/v1/swagger.json", "DSIF API v1");
            options.RoutePrefix = "api-docs";
            options.DocumentTitle = "DSIF API Documentation";

            // UI Customization
            options.DefaultModelsExpandDepth(2);
            options.DefaultModelExpandDepth(2);
            options.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
            options.EnableFilter();
            options.EnableDeepLinking();
            options.DisplayRequestDuration();
            options.EnableTryItOutByDefault();

            // Custom CSS (optional)
            // options.InjectStylesheet("/swagger-ui/custom.css");
        });

        return app;
    }
}

/// <summary>
/// Operation filter to add authorization information to operations.
/// </summary>
public class AuthorizeCheckOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var hasAuthorize = context.MethodInfo.DeclaringType?.GetCustomAttributes(true)
            .Union(context.MethodInfo.GetCustomAttributes(true))
            .OfType<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>()
            .Any() ?? false;

        var hasAllowAnonymous = context.MethodInfo.GetCustomAttributes(true)
            .OfType<Microsoft.AspNetCore.Authorization.AllowAnonymousAttribute>()
            .Any();

        if (hasAuthorize && !hasAllowAnonymous)
        {
            operation.Security = new List<OpenApiSecurityRequirement>
            {
                new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                }
            };

            // Add 401 and 403 responses
            if (!operation.Responses.ContainsKey("401"))
            {
                operation.Responses.Add("401", new OpenApiResponse
                {
                    Description = "Unauthorized - Authentication required"
                });
            }

            if (!operation.Responses.ContainsKey("403"))
            {
                operation.Responses.Add("403", new OpenApiResponse
                {
                    Description = "Forbidden - Insufficient permissions"
                });
            }
        }
    }
}

/// <summary>
/// Operation filter to add common API responses.
/// </summary>
public class ApiResponsesOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        // Add common error responses if not already present
        if (!operation.Responses.ContainsKey("400"))
        {
            operation.Responses.Add("400", new OpenApiResponse
            {
                Description = "Bad Request - Invalid input or validation error"
            });
        }

        if (!operation.Responses.ContainsKey("500"))
        {
            operation.Responses.Add("500", new OpenApiResponse
            {
                Description = "Internal Server Error - An unexpected error occurred"
            });
        }
    }
}

/// <summary>
/// Operation filter to add correlation ID header parameter.
/// </summary>
public class CorrelationIdOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        operation.Parameters ??= new List<OpenApiParameter>();

        operation.Parameters.Add(new OpenApiParameter
        {
            Name = "X-Correlation-ID",
            In = ParameterLocation.Header,
            Required = false,
            Description = "Optional correlation ID for request tracking",
            Schema = new OpenApiSchema { Type = "string", Format = "uuid" }
        });
    }
}

/// <summary>
/// Schema filter to add enum descriptions.
/// </summary>
public class EnumSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type.IsEnum)
        {
            schema.Enum.Clear();
            var enumValues = Enum.GetNames(context.Type);
            foreach (var name in enumValues)
            {
                schema.Enum.Add(new Microsoft.OpenApi.Any.OpenApiString(name));
            }
            schema.Type = "string";
            schema.Description = $"Possible values: {string.Join(", ", enumValues)}";
        }
    }
}

/// <summary>
/// Document filter to add tag descriptions.
/// </summary>
public class TagDescriptionDocumentFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        swaggerDoc.Tags = new List<OpenApiTag>
        {
            new OpenApiTag
            {
                Name = "Employees",
                Description = "Employee management operations including CRUD, assignments, and status management"
            },
            new OpenApiTag
            {
                Name = "Departments",
                Description = "Department hierarchy and organization structure management"
            },
            new OpenApiTag
            {
                Name = "IncentivePlans",
                Description = "Incentive plan configuration, slabs, and assignment management"
            },
            new OpenApiTag
            {
                Name = "Calculations",
                Description = "Incentive calculation operations including batch processing and recalculations"
            },
            new OpenApiTag
            {
                Name = "Approvals",
                Description = "Approval workflow management for incentive calculations"
            },
            new OpenApiTag
            {
                Name = "Audit",
                Description = "Audit logging and compliance tracking operations"
            },
            new OpenApiTag
            {
                Name = "Reports",
                Description = "Report generation and analytics operations"
            },
            new OpenApiTag
            {
                Name = "Configuration",
                Description = "System configuration, feature flags, and settings management"
            },
            new OpenApiTag
            {
                Name = "Jobs",
                Description = "Background job management and scheduling operations"
            },
            new OpenApiTag
            {
                Name = "DataTransfer",
                Description = "Data import and export operations with template support"
            },
            new OpenApiTag
            {
                Name = "Performance",
                Description = "Performance monitoring, caching, and optimization operations"
            },
            new OpenApiTag
            {
                Name = "Security",
                Description = "Role-based access control and security management"
            }
        };
    }
}
