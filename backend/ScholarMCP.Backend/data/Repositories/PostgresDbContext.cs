using Microsoft.EntityFrameworkCore;

namespace ScholarMCP.Data.Repositories
{
    /// <summary>
    /// PostgreSQL DbContext
    /// PostgreSQL 数据库上下文
    /// PostgreSQL用DbContext
    /// </summary>
    public class PostgresDbContext : DbContext
    {
        /// <summary>
        /// 构造函数 / Constructor / コンストラクタ
        /// </summary>
        /// <param name="options">DbContext 选项 / DbContext options / DbContextオプション</param>
        public PostgresDbContext(DbContextOptions<PostgresDbContext> options) : base(options) { }

        /// <summary>
        /// 示例实体集合 / Example entity set / サンプルエンティティセット
        /// </summary>
        public DbSet<SampleEntity> Samples { get; set; }
    }

    /// <summary>
    /// 示例实体 / Example entity / サンプルエンティティ
    /// </summary>
    public class SampleEntity
    {
        public int Id { get; set; }
        public string? Name { get; set; }
    }
} 