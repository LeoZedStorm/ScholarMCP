using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol.Server;

namespace ScholarMCP.Tools.SystemTools
{
    /// <summary>
    /// System tools for MCP integration
    /// 系统工具MCP集成
    /// システムツールMCP統合
    /// </summary>
    [McpServerToolType]
    public static class SystemTools
    {
        /// <summary>
        /// Echo message back to test MCP communication
        /// 回显消息以测试MCP通信
        /// MCPコミュニケーションをテストするためのエコーメッセージ
        /// </summary>
        /// <param name="message">Message to echo back</param>
        /// <returns>Echoed message with timestamp</returns>
        [McpServerTool, Description("Echo a message back to test MCP communication. Useful for testing connectivity and basic functionality.")]
        public static string Echo(
            [Description("The message to echo back")] string message)
        {
            var result = new
            {
                success = true,
                originalMessage = message,
                echoedMessage = $"ScholarMCP Echo: {message}",
                timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC"),
                serverInfo = "ScholarMCP Server v1.0.0"
            };

            return JsonSerializer.Serialize(result, new JsonSerializerOptions 
            { 
                WriteIndented = true 
            });
        }

        /// <summary>
        /// Get current server status and information
        /// 获取当前服务器状态和信息
        /// 現在のサーバーステータスと情報を取得
        /// </summary>
        /// <returns>Server status information in JSON format</returns>
        [McpServerTool, Description("Get current server status, uptime, and system information. Useful for monitoring server health.")]
        public static string GetServerStatus()
        {
            var startTime = global::System.Diagnostics.Process.GetCurrentProcess().StartTime;
            var uptime = DateTime.Now - startTime;

            var result = new
            {
                success = true,
                server = new
                {
                    name = "ScholarMCP Server",
                    version = "1.0.0",
                    status = "Running",
                    protocol = "Model Context Protocol (MCP)",
                    transport = "Streamable HTTP"
                },
                system = new
                {
                    currentTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC"),
                    startTime = startTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    uptime = new
                    {
                        days = uptime.Days,
                        hours = uptime.Hours,
                        minutes = uptime.Minutes,
                        seconds = uptime.Seconds,
                        totalMilliseconds = uptime.TotalMilliseconds
                    },
                    processId = Environment.ProcessId,
                    machineName = Environment.MachineName,
                    osVersion = Environment.OSVersion.ToString(),
                    dotnetVersion = Environment.Version.ToString()
                },
                capabilities = new
                {
                    tools = new[] { "Weather", "System", "Database" },
                    features = new[] { "Real-time data", "Multi-location support", "Error handling", "Logging" }
                }
            };

            return JsonSerializer.Serialize(result, new JsonSerializerOptions 
            { 
                WriteIndented = true 
            });
        }

        /// <summary>
        /// Get current UTC time
        /// 获取当前UTC时间
        /// 現在のUTC時間を取得
        /// </summary>
        /// <returns>Current UTC time in various formats</returns>
        [McpServerTool, Description("Get current UTC time in multiple formats. Useful for time synchronization and logging.")]
        public static string GetCurrentTime()
        {
            var now = DateTime.UtcNow;
            
            var result = new
            {
                success = true,
                utcTime = new
                {
                    iso8601 = now.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                    readable = now.ToString("yyyy-MM-dd HH:mm:ss UTC"),
                    unix = ((DateTimeOffset)now).ToUnixTimeSeconds(),
                    unixMilliseconds = ((DateTimeOffset)now).ToUnixTimeMilliseconds(),
                    dayOfWeek = now.DayOfWeek.ToString(),
                    dayOfYear = now.DayOfYear
                },
                timezone = new
                {
                    local = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    timeZone = TimeZoneInfo.Local.DisplayName,
                    offset = TimeZoneInfo.Local.GetUtcOffset(DateTime.Now).ToString()
                }
            };

            return JsonSerializer.Serialize(result, new JsonSerializerOptions 
            { 
                WriteIndented = true 
            });
        }

        /// <summary>
        /// Generate a unique identifier
        /// 生成唯一标识符
        /// 一意識別子を生成
        /// </summary>
        /// <param name="format">Format of the identifier (guid, short, numeric)</param>
        /// <returns>Generated unique identifier</returns>
        [McpServerTool, Description("Generate a unique identifier in various formats. Useful for creating IDs, session tokens, etc.")]
        public static string GenerateUniqueId(
            [Description("Format of the identifier: 'guid' (default), 'short' (8 chars), 'numeric' (timestamp-based)")] string format = "guid")
        {
            try
            {
                string generatedId;
                string description;

                switch (format.ToLower())
                {
                    case "short":
                        generatedId = Guid.NewGuid().ToString("N")[..8];
                        description = "8-character alphanumeric identifier";
                        break;
                    
                    case "numeric":
                        generatedId = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
                        description = "Unix timestamp in milliseconds";
                        break;
                    
                    case "guid":
                    default:
                        generatedId = Guid.NewGuid().ToString();
                        description = "Standard GUID format";
                        break;
                }

                var result = new
                {
                    success = true,
                    identifier = generatedId,
                    format = format.ToLower(),
                    description = description,
                    generatedAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC")
                };

                return JsonSerializer.Serialize(result, new JsonSerializerOptions 
                { 
                    WriteIndented = true 
                });
            }
            catch (Exception ex)
            {
                var errorResult = new
                {
                    success = false,
                    error = $"Failed to generate unique ID: {ex.Message}",
                    timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC")
                };

                return JsonSerializer.Serialize(errorResult, new JsonSerializerOptions 
                { 
                    WriteIndented = true 
                });
            }
        }

        // 已移除SaveUploadedFile和WriteNeo4jNodeAndRelation方法，只保留系统相关工具。
    }
} 