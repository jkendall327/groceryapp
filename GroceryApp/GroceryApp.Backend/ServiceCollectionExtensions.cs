using Azure.Storage.Blobs;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Rest;

namespace GroceryApp.Backend;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCosmosServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<CosmosClient>(sp =>
        {
            var cosmosConfig = configuration.GetSection("CosmosDb");
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
        return services;
    }

    public static IServiceCollection AddCustomServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOpenApi();

        services.AddCosmosServices(configuration);
        services.AddBlobServices(configuration);
        services.AddComputerVisionServices(configuration);
        services.AddLlmServices(configuration);

        return services;
    }
}
