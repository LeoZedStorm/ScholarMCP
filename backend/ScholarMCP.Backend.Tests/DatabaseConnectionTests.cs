using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ScholarMCP.Data.Repositories;
using Xunit;
using Neo4j.Driver;

namespace ScholarMCP.Backend.Tests
{
    /// <summary>
    /// 数据库连接测试 / Database Connection Tests / データベース接続テスト
    /// </summary>
    public class DatabaseConnectionTests
    {
        [Fact(DisplayName = "PostgreSQL 连接测试 / PostgreSQL Connection Test / PostgreSQL接続テスト")]
        public void CanConnectToPostgres()
        {
            var options = new DbContextOptionsBuilder<PostgresDbContext>()
                .UseNpgsql("Host=localhost;Port=5432;Database=scholarmcp;Username=scholarmcp;Password=scholarmcp")
                .Options;
            using var context = new PostgresDbContext(options);
            // 尝试连接 / Try connect / 接続を試みる
            context.Database.OpenConnection();
            Assert.True(context.Database.CanConnect());
            context.Database.CloseConnection();
        }

        [Fact(DisplayName = "Neo4j 连接测试 / Neo4j Connection Test / Neo4j接続テスト")]
        public void CanConnectToNeo4j()
        {
            var provider = new Neo4jDriverProvider("bolt://localhost:7687", "neo4j", "scholarmcp");
            using var session = provider.GetDriver().AsyncSession();
            var result = session.RunAsync("RETURN 1 AS Test").Result;
            Assert.NotNull(result);
        }
    }
} 