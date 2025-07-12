using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ModelContextProtocol.Server;
using ScholarMCP.Data.Repositories;

namespace ScholarMCP.Tools.Database
{
    [McpServerToolType]
    public static class SqlDbTools
    {
        /// <summary>
        /// Create a new SampleEntity record
        /// 创建新的 SampleEntity 记录
        /// SampleEntity レコードを新規作成
        /// </summary>
        /// <param name="db">PostgresDbContext (auto-injected)</param>
        /// <param name="dataJson">Record data as JSON string</param>
        /// <returns>Operation result (created id)</returns>
        [McpServerTool, Description("Create a new SampleEntity record. Data as JSON string.")]
        public static async Task<string> CreateSample(
            [Description("PostgresDbContext (auto-injected)")] PostgresDbContext db,
            [Description("Record data as JSON string")] string dataJson)
        {
            try
            {
                var entity = JsonSerializer.Deserialize<SampleEntity>(dataJson);
                db.Samples.Add(entity);
                await db.SaveChangesAsync();
                return JsonSerializer.Serialize(new { success = true, id = entity.Id }, new JsonSerializerOptions { WriteIndented = true });
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new { success = false, error = ex.Message }, new JsonSerializerOptions { WriteIndented = true });
            }
        }

        /// <summary>
        /// Get a SampleEntity record by id
        /// 按ID查询 SampleEntity 记录
        /// IDで SampleEntity レコードを取得
        /// </summary>
        /// <param name="db">PostgresDbContext (auto-injected)</param>
        /// <param name="id">Record id</param>
        /// <returns>Record data as JSON</returns>
        [McpServerTool, Description("Get a SampleEntity record by id.")]
        public static async Task<string> GetSampleById(
            [Description("PostgresDbContext (auto-injected)")] PostgresDbContext db,
            [Description("Record id")] int id)
        {
            try
            {
                var entity = await db.Samples.FindAsync(id);
                if (entity == null)
                    return JsonSerializer.Serialize(new { success = false, error = "Not found" }, new JsonSerializerOptions { WriteIndented = true });
                return JsonSerializer.Serialize(new { success = true, entity }, new JsonSerializerOptions { WriteIndented = true });
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new { success = false, error = ex.Message }, new JsonSerializerOptions { WriteIndented = true });
            }
        }

        /// <summary>
        /// Update a SampleEntity record by id
        /// 按ID更新 SampleEntity 记录
        /// IDで SampleEntity レコードを更新
        /// </summary>
        /// <param name="db">PostgresDbContext (auto-injected)</param>
        /// <param name="id">Record id</param>
        /// <param name="dataJson">New data as JSON string</param>
        /// <returns>Operation result</returns>
        [McpServerTool, Description("Update a SampleEntity record by id. Data as JSON string.")]
        public static async Task<string> UpdateSampleById(
            [Description("PostgresDbContext (auto-injected)")] PostgresDbContext db,
            [Description("Record id")] int id,
            [Description("New data as JSON string")] string dataJson)
        {
            try
            {
                var entity = await db.Samples.FindAsync(id);
                if (entity == null)
                    return JsonSerializer.Serialize(new { success = false, error = "Not found" }, new JsonSerializerOptions { WriteIndented = true });
                var newData = JsonSerializer.Deserialize<SampleEntity>(dataJson);
                entity.Name = newData?.Name;
                await db.SaveChangesAsync();
                return JsonSerializer.Serialize(new { success = true }, new JsonSerializerOptions { WriteIndented = true });
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new { success = false, error = ex.Message }, new JsonSerializerOptions { WriteIndented = true });
            }
        }

        /// <summary>
        /// Delete a SampleEntity record by id
        /// 按ID删除 SampleEntity 记录
        /// IDで SampleEntity レコードを削除
        /// </summary>
        /// <param name="db">PostgresDbContext (auto-injected)</param>
        /// <param name="id">Record id</param>
        /// <returns>Operation result</returns>
        [McpServerTool, Description("Delete a SampleEntity record by id.")]
        public static async Task<string> DeleteSampleById(
            [Description("PostgresDbContext (auto-injected)")] PostgresDbContext db,
            [Description("Record id")] int id)
        {
            try
            {
                var entity = await db.Samples.FindAsync(id);
                if (entity == null)
                    return JsonSerializer.Serialize(new { success = false, error = "Not found" }, new JsonSerializerOptions { WriteIndented = true });
                db.Samples.Remove(entity);
                await db.SaveChangesAsync();
                return JsonSerializer.Serialize(new { success = true }, new JsonSerializerOptions { WriteIndented = true });
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new { success = false, error = ex.Message }, new JsonSerializerOptions { WriteIndented = true });
            }
        }

        /// <summary>
        /// Query SampleEntity records with optional filter and pagination
        /// 查询 SampleEntity 记录，支持条件和分页
        /// SampleEntity レコードを条件・ページネーションで検索
        /// </summary>
        /// <param name="db">PostgresDbContext (auto-injected)</param>
        /// <param name="name">Name filter (optional)</param>
        /// <param name="skip">Skip count (pagination)</param>
        /// <param name="limit">Limit count (pagination)</param>
        /// <returns>List of records</returns>
        [McpServerTool, Description("Query SampleEntity records with optional filter and pagination.")]
        public static async Task<string> QuerySamples(
            [Description("PostgresDbContext (auto-injected)")] PostgresDbContext db,
            [Description("Name filter (optional)")] string? name = null,
            [Description("Skip count (pagination)")] int skip = 0,
            [Description("Limit count (pagination)")] int limit = 100)
        {
            try
            {
                var query = db.Samples.AsQueryable();
                if (!string.IsNullOrWhiteSpace(name))
                    query = query.Where(e => e.Name != null && e.Name.Contains(name));
                var result = await query.Skip(skip).Take(limit).ToListAsync();
                return JsonSerializer.Serialize(new { success = true, result }, new JsonSerializerOptions { WriteIndented = true });
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new { success = false, error = ex.Message }, new JsonSerializerOptions { WriteIndented = true });
            }
        }

        /// <summary>
        /// Query records with custom SQL (read-only, advanced)
        /// 使用自定义SQL查询记录（只读，高级用法）
        /// カスタムSQLでレコードを検索（読み取り専用・上級）
        /// </summary>
        /// <param name="db">PostgresDbContext (auto-injected)</param>
        /// <param name="sql">SQL query string (read-only)</param>
        /// <param name="parametersJson">Parameters as JSON string</param>
        /// <returns>Query result</returns>
        [McpServerTool, Description("Query records with custom SQL (read-only, advanced). Parameters as JSON string.")]
        public static async Task<string> QuerySamplesBySql(
            [Description("PostgresDbContext (auto-injected)")] PostgresDbContext db,
            [Description("SQL query string (read-only)")] string sql,
            [Description("Parameters as JSON string")] string? parametersJson = null)
        {
            try
            {
                var parameters = !string.IsNullOrWhiteSpace(parametersJson)
                    ? JsonSerializer.Deserialize<Dictionary<string, object>>(parametersJson)
                    : new Dictionary<string, object>();
                var result = await db.Samples.FromSqlRaw(sql, parameters.Values.ToArray()).ToListAsync();
                return JsonSerializer.Serialize(new { success = true, result }, new JsonSerializerOptions { WriteIndented = true });
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new { success = false, error = ex.Message }, new JsonSerializerOptions { WriteIndented = true });
            }
        }

        /// <summary>
        /// Get all table names in the current database
        /// 获取当前数据库的所有表名
        /// 現在のデータベースの全テーブル名を取得
        /// </summary>
        /// <param name="db">PostgresDbContext (auto-injected)</param>
        /// <returns>List of table names</returns>
        [McpServerTool, Description("Get all table names in the current database.")]
        public static async Task<string> GetAllTableNames(
            [Description("PostgresDbContext (auto-injected)")] PostgresDbContext db)
        {
            try
            {
                var tableNames = await db.Database.ExecuteSqlRawAsync(
                    "SELECT table_name FROM information_schema.tables WHERE table_schema = 'public'");
                // only return the number of affected rows, need to use NpgsqlConnection or Dapper to query table names
                // use NpgsqlConnection directly to query table names
                var conn = db.Database.GetDbConnection();
                await conn.OpenAsync();
                var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT table_name FROM information_schema.tables WHERE table_schema = 'public'";
                var reader = await cmd.ExecuteReaderAsync();
                var tables = new List<string>();
                while (await reader.ReadAsync())
                {
                    tables.Add(reader.GetString(0));
                }
                await conn.CloseAsync();
                return JsonSerializer.Serialize(new { success = true, tables }, new JsonSerializerOptions { WriteIndented = true });
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new { success = false, error = ex.Message }, new JsonSerializerOptions { WriteIndented = true });
            }
        }

        /// <summary>
        /// Get table schema (columns, types, primary key, nullable)
        /// 获取指定表的结构（字段、类型、主键、可空）
        /// 指定テーブルのスキーマ（カラム・型・主キー・NULL可）を取得
        /// </summary>
        /// <param name="db">PostgresDbContext (auto-injected)</param>
        /// <param name="tableName">Table name</param>
        /// <returns>Table schema info</returns>
        [McpServerTool, Description("Get table schema (columns, types, primary key, nullable).")]
        public static async Task<string> GetTableSchema(
            [Description("PostgresDbContext (auto-injected)")] PostgresDbContext db,
            [Description("Table name")] string tableName)
        {
            try
            {
                var conn = db.Database.GetDbConnection();
                await conn.OpenAsync();
                var cmd = conn.CreateCommand();
                cmd.CommandText = $@"
                    SELECT
                        column_name,
                        data_type,
                        is_nullable,
                        column_default,
                        (
                            SELECT tc.constraint_type
                            FROM information_schema.table_constraints tc
                            JOIN information_schema.constraint_column_usage ccu
                              ON tc.constraint_name = ccu.constraint_name
                            WHERE tc.table_name = '{tableName}' AND ccu.column_name = c.column_name AND tc.constraint_type = 'PRIMARY KEY'
                            LIMIT 1
                        ) as primary_key
                    FROM information_schema.columns c
                    WHERE table_name = '{tableName}'
                ";
                var reader = await cmd.ExecuteReaderAsync();
                var columns = new List<object>();
                while (await reader.ReadAsync())
                {
                    columns.Add(new
                    {
                        name = reader.GetString(0),
                        type = reader.GetString(1),
                        isNullable = reader.GetString(2) == "YES",
                        defaultValue = reader.IsDBNull(3) ? null : reader.GetValue(3),
                        isPrimaryKey = !reader.IsDBNull(4) && reader.GetString(4) == "PRIMARY KEY"
                    });
                }
                await conn.CloseAsync();
                return JsonSerializer.Serialize(new { success = true, columns }, new JsonSerializerOptions { WriteIndented = true });
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new { success = false, error = ex.Message }, new JsonSerializerOptions { WriteIndented = true });
            }
        }

        /// <summary>
        /// Get all registered entity types and their properties
        /// 获取所有已注册实体类型及其属性
        /// 登録済みエンティティ型とそのプロパティを取得
        /// </summary>
        /// <returns>List of entity types and their properties</returns>
        [McpServerTool, Description("Get all registered entity types and their properties.")]
        public static string GetAllEntityTypes()
        {
            try
            {
                var types = new List<object>();
                var dbContextType = typeof(PostgresDbContext);
                foreach (var prop in dbContextType.GetProperties())
                {
                    if (prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>))
                    {
                        var entityType = prop.PropertyType.GetGenericArguments()[0];
                        var props = new List<object>();
                        foreach (var ep in entityType.GetProperties())
                        {
                            props.Add(new
                            {
                                name = ep.Name,
                                type = ep.PropertyType.Name,
                                canWrite = ep.CanWrite,
                                canRead = ep.CanRead
                            });
                        }
                        types.Add(new { entity = entityType.Name, properties = props });
                    }
                }
                return JsonSerializer.Serialize(new { success = true, types }, new JsonSerializerOptions { WriteIndented = true });
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new { success = false, error = ex.Message }, new JsonSerializerOptions { WriteIndented = true });
            }
        }

        /// <summary>
        /// Get schema of a specific entity type
        /// 获取指定实体类型的结构
        /// 指定エンティティ型のスキーマを取得
        /// </summary>
        /// <param name="entityName">Entity type name</param>
        /// <returns>Entity schema info</returns>
        [McpServerTool, Description("Get schema of a specific entity type.")]
        public static string GetEntitySchema(
            [Description("Entity type name")] string entityName)
        {
            try
            {
                var dbContextType = typeof(PostgresDbContext);
                foreach (var prop in dbContextType.GetProperties())
                {
                    if (prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>))
                    {
                        var entityType = prop.PropertyType.GetGenericArguments()[0];
                        if (entityType.Name.Equals(entityName, StringComparison.OrdinalIgnoreCase))
                        {
                            var props = new List<object>();
                            foreach (var ep in entityType.GetProperties())
                            {
                                props.Add(new
                                {
                                    name = ep.Name,
                                    type = ep.PropertyType.Name,
                                    canWrite = ep.CanWrite,
                                    canRead = ep.CanRead
                                });
                            }
                            return JsonSerializer.Serialize(new { success = true, entity = entityType.Name, properties = props }, new JsonSerializerOptions { WriteIndented = true });
                        }
                    }
                }
                return JsonSerializer.Serialize(new { success = false, error = "Entity not found" }, new JsonSerializerOptions { WriteIndented = true });
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new { success = false, error = ex.Message }, new JsonSerializerOptions { WriteIndented = true });
            }
        }
    }
} 