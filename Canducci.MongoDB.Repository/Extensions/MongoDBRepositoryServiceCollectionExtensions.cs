using System;
using Microsoft.Extensions.DependencyInjection;

namespace Canducci.MongoDB.Repository
{
    public static class MongoDBRepositoryServiceCollectionExtensions
    {
        public static IServiceCollection AddMongoDBRepository(this IServiceCollection services)
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddSingleton<IConnect, Connect>();
            return services;
        }
    }
}
