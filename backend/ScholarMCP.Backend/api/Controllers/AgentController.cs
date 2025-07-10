using System;
using Microsoft.AspNetCore.Mvc;

namespace ScholarMCP.Api.Controllers
{
    /// <summary>
    /// AI Agent API Controller
    /// AI Agent API 控制器
    /// AIエージェントAPIコントローラー
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AgentController : ControllerBase
    {
        /// <summary>
        /// Agent placeholder method
        /// Agent 占位方法
        /// Agent プレースホルダーメソッド
        /// </summary>
        /// <returns>string 占位符 placeholder プレースホルダー</returns>
        [HttpGet]
        public string Get()
        {
            return "Agent Placeholder";
        }
    }
} 