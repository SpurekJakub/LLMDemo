using LLMDemo.Core.Abstractions;
using LLMDemo.Core.Clients;
using LLMDemo.Core.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LLMDemo.Core.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers LLMDemo.Core services: LmStudioOptions binding and IChatCompletionService
    /// backed by LM Studio.
    /// </summary>
    public static IServiceCollection AddLlmDemoCore(
        this IServiceCollection services,
        Action<LmStudioOptions>? configure = null)
    {
        var optionsBuilder = services
            .AddOptions<LmStudioOptions>()
            .BindConfiguration(LmStudioOptions.SectionName);

        if (configure is not null)
            optionsBuilder.Configure(configure);

        optionsBuilder.ValidateOnStart();

        services.AddSingleton<IChatCompletionService, LmStudioChatCompletionService>();

        return services;
    }
}
