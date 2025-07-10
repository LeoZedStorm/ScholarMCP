using System;
using Microsoft.AspNetCore.Mvc;

namespace ScholarMCP.Api.Controllers
{
    /// <summary>
    /// MCP API Controller
    /// MCP API 控制器
    /// MCP API コントローラー
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class McpController : ControllerBase
    {
        /// <summary>
        /// MCP placeholder method
        /// MCP 占位方法
        /// MCP プレースホルダーメソッド
        /// </summary>
        /// <returns>string 占位符 placeholder プレースホルダー</returns>
        [HttpGet]
        public string Get()
        {
            return "MCP Placeholder";
        }
    }
} 