using System.Text.Json.Serialization;

using FluentValidation;

using MicroElements.NSwag.FluentValidation.AspNetCore;

using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi;

using SharpGrip.FluentValidation.AutoValidation.Endpoints.Extensions;

using Opengate.Modules.Shared.Adapters.UI.Middlewares;
using Opengate.Modules.Shared.Configuration.API.Configurations;

namespace Opengate.Modules.Shared.Configuration.API;

public static class ApiConfiguration
{
    public static IServiceCollection AddApiConfiguration(this IServiceCollection services)
    {
        services.AddOpenApi(c =>
        {
            c.AddDocumentTransformer((document, context, cancellationToken) =>
            {
                document.Components ??= new OpenApiComponents();
                document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();

                if (!document.Components.SecuritySchemes!.ContainsKey("Bearer"))
                {
                    document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
                    {
                        Type = SecuritySchemeType.Http,
                        Scheme = "bearer",
                        BearerFormat = "JWT",
                        In = ParameterLocation.Header,
                        Name = "Authorization"
                    };
                }

                document.Security ??= [];
                document.Security.Add(new OpenApiSecurityRequirement
                {
                    [new OpenApiSecuritySchemeReference("Bearer", document)] = []
                });

                return Task.CompletedTask;
            });
        });


        services.AddHttpLogging(options =>
        {
            options.LoggingFields = HttpLoggingFields.ResponseBody
          | HttpLoggingFields.RequestBody
          | HttpLoggingFields.ResponseStatusCode
          | HttpLoggingFields.Duration
          | HttpLoggingFields.RequestMethod
          | HttpLoggingFields.RequestQuery
          | HttpLoggingFields.RequestPath;

            options.RequestHeaders.Add("Authorization");
            options.ResponseHeaders.Add("Set-Cookie");

            options.RequestBodyLogLimit = 4096;
            options.ResponseBodyLogLimit = 4096;
        });

        services.ConfigureHttpJsonOptions(options
                     => options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

        services.Configure<ApiBehaviorOptions>(options
                => options.SuppressModelStateInvalidFilter = true);

        services.AddExceptionHandler<ErrorHandlerMiddleware>();

        services.AddProblemDetails(options =>
        {
            options.CustomizeProblemDetails = context =>
            {
                context.ProblemDetails.Instance = $"{context.HttpContext.Request.Method} {context.HttpContext.Request.Path}";

                context.ProblemDetails.Extensions.TryAdd("requestId", context.HttpContext.TraceIdentifier);

                var activity = context.HttpContext.Features.Get<IHttpActivityFeature>()?.Activity;

                context.ProblemDetails.Extensions.TryAdd("traceId", activity?.Id);
            };
        });

        FluentValidationConfiguration(services);

        services.AddEndpointsApiExplorer();

        return services;
    }

    private static void FluentValidationConfiguration(IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<Program>(
            lifetime: ServiceLifetime.Singleton);

        services.AddFluentValidationAutoValidation(o =>
        {
            o.OverrideDefaultResultFactoryWith<ProblemDetailsResultFactory>();
        });

        services.AddFluentValidationRulesToSwagger();
        ;
    }

    public static void UseApiConfiguration(this WebApplication app)
    {
        app.UseHttpLogging();

        app.UseExceptionHandler(new ExceptionHandlerOptions()
        {
            AllowStatusCode404Response = true,
        });
    }
}