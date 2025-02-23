using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Logging;
using System.Text;

namespace GroceryApp.Backend;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCosmosServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<CosmosClient>(sp =>
        {
            var cosmosConfig = configuration.GetSection("CosmosDb");
            var logger = sp.GetRequiredService<ILogger<CosmosClient>>();
            logger.LogInformation("Configuring CosmosClient.");
            return new CosmosClient(cosmosConfig["Account"], cosmosConfig["Key"]);
        });

        services.AddSingleton<ICosmosService, CosmosService>();
        return services;
    }

    public static IServiceCollection AddBlobServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<BlobServiceClient>(sp =>
        {
            var blobConfig = configuration.GetSection("AzureBlobStorage");
            var logger = sp.GetRequiredService<ILogger<BlobServiceClient>>();
            logger.LogInformation("Configuring BlobServiceClient.");
            return new BlobServiceClient(blobConfig["ConnectionString"]);
        });

        services.AddSingleton<IBlobService, BlobService>();
        return services;
    }

    public static IServiceCollection AddComputerVisionServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<ComputerVisionClient>(sp =>
        {
            var visionConfig = configuration.GetSection("ComputerVision");
            var logger = sp.GetRequiredService<ILogger<ComputerVisionClient>>();
            logger.LogInformation("Configuring ComputerVisionClient.");
            return new ComputerVisionClient(new ApiKeyServiceClientCredentials(visionConfig["SubscriptionKey"]))
            {
                Endpoint = visionConfig["Endpoint"]
            };
        });

        services.AddSingleton<IComputerVisionService, ComputerVisionService>();
        return services;
    }

    public static IServiceCollection AddLlmServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpClient<ILlmService, LlmService>();
        services.AddSingleton<ILogger<LlmService>, Logger<LlmService>>();
        return services;
    }

    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSettings = configuration.GetSection("Jwt");
        var secretKey = jwtSettings["SecretKey"];

        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = true;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidateAudience = true,
                    ValidAudience = jwtSettings["Audience"],
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                    ValidateLifetime = true
                };
            });

        services.AddSingleton<ILogger<JwtBearerEvents>, Logger<JwtBearerEvents>>();
        return services;
    }

    public static IServiceCollection AddCustomServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOpenApi();

        services.AddCosmosServices(configuration);
        services.AddBlobServices(configuration);
        services.AddComputerVisionServices(configuration);
        services.AddLlmServices(configuration);
        services.AddJwtAuthentication(configuration);

        return services;
    }
}
