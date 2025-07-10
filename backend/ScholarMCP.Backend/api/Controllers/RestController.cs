using System;
using Microsoft.AspNetCore.Mvc;

namespace ScholarMCP.Api.Controllers
{
    /// <summary>
    /// RESTful API Controller
    /// RESTful API 控制器
    /// RESTful API コントローラー
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class RestController : ControllerBase
    {
        /// <summary>
        /// Get placeholder
        /// 获取占位符
        /// プレースホルダーを取得
        /// </summary>
        /// <returns>string 占位符 placeholder プレースホルダー</returns>
        [HttpGet]
        public string Get()
        {
            return "Placeholder";
        }
    }
} 