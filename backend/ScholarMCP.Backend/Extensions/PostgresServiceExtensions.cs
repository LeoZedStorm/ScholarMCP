using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ScholarMCP.Data.Repositories;

namespace ScholarMCP.Backend.Extensions
{
    /// <summary>
    /// PostgreSQL 服务注册扩展
    /// PostgreSQL Service Registration Extension
    /// PostgreSQLサービス登録拡張
    /// </summary>
    public static class PostgresServiceExtensions
    {
        /// <summary>
        /// 注册 PostgreSQL DbContext
        /// Register PostgreSQL DbContext
        /// PostgreSQL DbContext を登録
        /// </summary>
        /// <param name="services">服务集合 / Service collection / サービスコレクション</param>
        /// <param name="config">配置 / Configuration / 設定</param>
        /// <returns>服务集合 / Service collection / サービスコレクション</returns>
        public static IServiceCollection AddPostgresDb(this IServiceCollection services, IConfiguration config)
        {
            services.AddDbContext<PostgresDbContext>(options =>
                options.UseNpgsql(config.GetConnectionString("PostgresConnection")));
            return services;
        }
    }
} 