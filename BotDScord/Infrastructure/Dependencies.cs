using Microsoft.Extensions.DependencyInjection;

namespace BotDScord.Infrastructure;

public static class Dependencies
{
    public static void AddInfrastructure(this IServiceCollection services)
    {
        services.AddDbContext<BotDScordDbContext>();
    }
}