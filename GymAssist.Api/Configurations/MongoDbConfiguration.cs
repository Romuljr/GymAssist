using MongoDB.Driver;

namespace GymAssist.Api.Configurations
{
    public static class MongoDbConfiguration
    {
        public static IServiceCollection AddMongoDb(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var connectionString = configuration["MongoDB:ConnectionString"]!;

            var databaseName = configuration["MongoDb:Database"]!;

            var client = new MongoClient(connectionString);

            var database = client.GetDatabase(databaseName);

            services.AddSingleton<IMongoDatabase>(database);

            return services;
        }
    }
}
