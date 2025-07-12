using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using ScholarMCP.Backend.Extensions;
using System.Reflection;
using ModelContextProtocol.Server;
using System.ComponentModel;

var builder = WebApplication.CreateBuilder(args);

// Add MCP Server with HTTP transport
builder.Services.AddMcpServer()
    .WithHttpTransport()
    .WithToolsFromAssembly();

// PostgreSQL DbContext
builder.Services.AddPostgresDb(builder.Configuration);

// Neo4j Driver Provider  
builder.Services.AddNeo4jDriver(builder.Configuration);

// Add Controllers support
builder.Services.AddControllers();

// Add API documentation support
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure Swagger in development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Configure MCP routes using extension method (符合传统路由模式)
// 使用扩展方法配置MCP路由（符合传统路由模式）
// 拡張メソッドを使用してMCPルートを設定（従来のルーティングパターンに準拠）
app.ConfigureMcpRoutes("/mcp");

// General health check endpoint
app.MapGet("/health", () => Results.Ok(new 
{ 
    status = "healthy",
    server = "ScholarMCP Server",
    version = "1.0.0",
    timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC"),
    services = new
    {
        mcp = "/mcp/health",
        api = "/api",
        docs = "/swagger"
    }
}))
.WithName("GetGeneralHealth")
.WithTags("Health")
.WithOpenApi();

// Map Controllers for RESTful APIs and Agent endpoints
app.MapControllers();

app.Run();
