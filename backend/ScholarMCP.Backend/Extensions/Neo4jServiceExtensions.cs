using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ScholarMCP.Data.Repositories;

namespace ScholarMCP.Backend.Extensions
{
    /// <summary>
    /// Neo4j 服务注册扩展
    /// Neo4j Service Registration Extension
    /// Neo4jサービス登録拡張
    /// </summary>
    public static class Neo4jServiceExtensions
    {
        /// <summary>
        /// 注册 Neo4j 驱动
        /// Register Neo4j Driver
        /// Neo4jドライバーを登録
        /// </summary>
        /// <param name="services">服务集合 / Service collection / サービスコレクション</param>
        /// <param name="config">配置 / Configuration / 設定</param>
        /// <returns>服务集合 / Service collection / サービスコレクション</returns>
        public static IServiceCollection AddNeo4jDriver(this IServiceCollection services, IConfiguration config)
        {
            var neo4jConfig = config.GetSection("Neo4j");
            var uri = neo4jConfig["Uri"] ?? throw new InvalidOperationException("Neo4j:Uri is not configured");
            var user = neo4jConfig["User"] ?? throw new InvalidOperationException("Neo4j:User is not configured");
            var password = neo4jConfig["Password"] ?? throw new InvalidOperationException("Neo4j:Password is not configured");
            services.AddSingleton(new Neo4jDriverProvider(uri, user, password));
            return services;
        }
    }
} 