using Microsoft.Extensions.DependencyInjection;

namespace PostalNET
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddPostal(this IServiceCollection services)
        {
            services.AddSingleton<IBox>(sp => Postal.Factory());

            return services;
        }
    }
}
