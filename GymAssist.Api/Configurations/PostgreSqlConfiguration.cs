using Npgsql;

namespace GymAssist.Api.Configurations
{
    public static class PostgreSqlConfiguration
    {
        public static IServiceCollection AddPostgreSql(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("PostgreSql")!;

            var builder = new NpgsqlDataSourceBuilder(connectionString);

            builder.UseVector();

            var dataSource = builder.Build();

            services.AddSingleton(dataSource);

            return services;
        }
    }
}
