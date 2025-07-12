using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ScholarMCP.Data.Repositories;
using ScholarMCP.Backend.Extensions;
using Xunit;
using Neo4j.Driver;

namespace ScholarMCP.Backend.Tests
{
    /// <summary>
    /// 数据库连接测试 / Database Connection Tests / データベース接続テスト
    /// </summary>
    public class DatabaseConnectionTests
    {
        private readonly IServiceProvider _serviceProvider;

        public DatabaseConnectionTests()
        {
            // Build configuration
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            // Register services
            var services = new ServiceCollection();
            services.AddPostgresDb(config);
            services.AddNeo4jDriver(config);
            _serviceProvider = services.BuildServiceProvider();
        }

        [Fact(DisplayName = "PostgreSQL 连接测试 / PostgreSQL Connection Test / PostgreSQL接続テスト")]
        public void CanConnectToPostgres()
        {
            var context = _serviceProvider.GetRequiredService<PostgresDbContext>();
            context.Database.OpenConnection();
            Assert.True(context.Database.CanConnect());
            context.Database.CloseConnection();
        }

        [Fact(DisplayName = "Neo4j 连接测试 / Neo4j Connection Test / Neo4j接続テスト")]
        public void CanConnectToNeo4j()
        {
            var provider = _serviceProvider.GetRequiredService<Neo4jDriverProvider>();
            using var session = provider.GetDriver().AsyncSession();
            var result = session.RunAsync("RETURN 1 AS Test").Result;
            Assert.NotNull(result);
        }
    }
} 