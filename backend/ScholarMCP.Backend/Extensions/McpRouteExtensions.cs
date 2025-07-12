using Microsoft.AspNetCore.Mvc;

namespace ScholarMCP.Backend.Extensions
{
    /// <summary>
    /// MCP route extensions for traditional ASP.NET Core routing
    /// MCP路由扩展，符合传统ASP.NET Core路由模式
    /// 従来のASP.NET Coreルーティングに準拠したMCPルート拡張
    /// </summary>
    public static class McpRouteExtensions
    {
        /// <summary>
        /// Configure MCP endpoints with traditional routing patterns
        /// 使用传统路由模式配置MCP端点
        /// 従来のルーティングパターンでMCPエンドポイントを設定
        /// </summary>
        /// <param name="app">WebApplication instance</param>
        /// <param name="basePath">Base path for MCP endpoints (default: "/mcp")</param>
        /// <returns>WebApplication for method chaining</returns>
        public static WebApplication ConfigureMcpRoutes(this WebApplication app, string basePath = "/mcp")
        {
            // Map MCP endpoints with custom base path
            // 使用自定义基础路径映射MCP端点
            // カスタムベースパスでMCPエンドポイントをマップ
            app.MapMcp(basePath);

            // Add MCP information endpoint (符合RESTful API模式)
            // 添加MCP信息端点（符合RESTful API模式）
            // MCP情報エンドポイントを追加（RESTful APIパターンに準拠）
            app.MapGet($"{basePath}/info", GetMcpInfo)
                .WithName("GetMcpInfo")
                .WithTags("MCP")
                .WithOpenApi();

            // Add MCP health check endpoint
            // 添加MCP健康检查端点
            // MCPヘルスチェックエンドポイントを追加
            app.MapGet($"{basePath}/health", GetMcpHealth)
                .WithName("GetMcpHealth")
                .WithTags("MCP")
                .WithOpenApi();

            return app;
        }

        /// <summary>
        /// Get MCP server information
        /// 获取MCP服务器信息
        /// MCPサーバー情報を取得
        /// </summary>
        /// <returns>MCP server information</returns>
        private static IResult GetMcpInfo()
        {
            var mcpInfo = new
            {
                server = new
                {
                    name = "ScholarMCP Server",
                    version = "1.0.0",
                    description = "Model Context Protocol server for ScholarMCP platform",
                    protocol = "Model Context Protocol (MCP)",
                    transport = "Streamable HTTP"
                },
                endpoints = new
                {
                    sse = "/mcp/sse",
                    message = "/mcp/message",
                    info = "/mcp/info",
                    health = "/mcp/health"
                },
                capabilities = new
                {
                    tools = new[] { "Weather", "System", "Database" },
                    features = new[] { "Real-time data", "Multi-location support", "Error handling", "Logging" }
                },
                documentation = new
                {
                    protocol = "https://modelcontextprotocol.io/",
                    tools = "/mcp/info",
                    usage = "Connect MCP clients to /mcp/sse endpoint"
                },
                timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC")
            };

            return Results.Ok(mcpInfo);
        }

        /// <summary>
        /// Get MCP server health status
        /// 获取MCP服务器健康状态
        /// MCPサーバーのヘルスステータスを取得
        /// </summary>
        /// <returns>MCP server health status</returns>
        private static IResult GetMcpHealth()
        {
            var startTime = global::System.Diagnostics.Process.GetCurrentProcess().StartTime;
            var uptime = DateTime.Now - startTime;

            var healthStatus = new
            {
                status = "healthy",
                server = "ScholarMCP Server",
                mcp = new
                {
                    protocol = "Model Context Protocol (MCP)",
                    transport = "Streamable HTTP",
                    endpoints = new
                    {
                        sse = "/mcp/sse",
                        message = "/mcp/message"
                    }
                },
                system = new
                {
                    uptime = new
                    {
                        days = uptime.Days,
                        hours = uptime.Hours,
                        minutes = uptime.Minutes,
                        totalMilliseconds = uptime.TotalMilliseconds
                    },
                    processId = Environment.ProcessId,
                    memoryUsage = GC.GetTotalMemory(false)
                },
                timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC")
            };

            return Results.Ok(healthStatus);
        }
    }
} 