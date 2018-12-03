using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SharpRepository.Repository;
using SharpRepository.Repository.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace WeyhdBot.MongoDb.Extensions
{
    public static class RepositoryMongoDbExtenston
    {
        public static IServiceCollection AddSharRepositoryFactory(this IServiceCollection services, IConfiguration Configuration)
        {
            var section = Configuration.GetSection("sharpRepository");
            ISharpRepositoryConfiguration sharpConfig = RepositoryFactory.BuildSharpRepositoryConfiguation(section);
            return services.AddSingleton(sp => new RepositoryFactory(sharpConfig));
        }
    }
}
