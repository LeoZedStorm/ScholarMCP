using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.Json;
using System.Threading.Tasks;
using ModelContextProtocol.Server;

namespace ScholarMCP.Tools.Database
{
    [McpServerToolType]
    public static class Neo4jTools
    {
        /// <summary>
        /// Write a node and optional relation to Neo4j
        /// 向Neo4j写入节点和可选关系
        /// Neo4jにノードと（オプションで）リレーションを追加
        /// </summary>
        /// <param name="neo4jDriver">Neo4j driver (auto-injected)</param>
        /// <param name="nodeLabel">Node label (e.g. Person)</param>
        /// <param name="nodePropertiesJson">Node properties as JSON string</param>
        /// <param name="relatedNodeLabel">Related node label (optional)</param>
        /// <param name="relatedNodePropertiesJson">Related node properties as JSON string (optional)</param>
        /// <param name="relationType">Relation type (e.g. KNOWS, optional)</param>
        /// <returns>Operation result JSON</returns>
        [McpServerTool, Description("Write a node and optional relation to Neo4j. Properties are JSON strings. Returns operation result.")]
        public static async Task<string> WriteNeo4jNodeAndRelation(
            [Description("Neo4j driver (auto-injected)")] Neo4j.Driver.IDriver neo4jDriver,
            [Description("Node label, e.g. 'Person'")] string nodeLabel,
            [Description("Node properties as JSON string, e.g. '{\"name\":\"Alice\"}'")] string nodePropertiesJson,
            [Description("Related node label (optional)")] string? relatedNodeLabel = null,
            [Description("Related node properties as JSON string (optional)")] string? relatedNodePropertiesJson = null,
            [Description("Relation type, e.g. 'KNOWS' (optional)")] string? relationType = null)
        {
            try
            {
                var nodeProps = JsonSerializer.Deserialize<Dictionary<string, object>>(nodePropertiesJson);
                var relatedNodeProps = !string.IsNullOrWhiteSpace(relatedNodePropertiesJson)
                    ? JsonSerializer.Deserialize<Dictionary<string, object>>(relatedNodePropertiesJson)
                    : null;

                using var session = neo4jDriver.AsyncSession();
                var tx = await session.BeginTransactionAsync();
                try
                {
                    // create main node
                    var createNodeCypher = $"CREATE (n:{nodeLabel} $props) RETURN id(n) as nodeId";
                    var nodeResult = await tx.RunAsync(createNodeCypher, new { props = nodeProps });
                    await nodeResult.FetchAsync();
                    var nodeId = Convert.ToInt64(nodeResult.Current["nodeId"]);

                    long? relatedNodeId = null;
                    if (!string.IsNullOrWhiteSpace(relatedNodeLabel) && relatedNodeProps != null)
                    {
                        // create related node
                        var createRelatedNodeCypher = $"CREATE (m:{relatedNodeLabel} $props) RETURN id(m) as relatedNodeId";
                        var relatedNodeResult = await tx.RunAsync(createRelatedNodeCypher, new { props = relatedNodeProps });
                        await relatedNodeResult.FetchAsync();
                        relatedNodeId = Convert.ToInt64(relatedNodeResult.Current["relatedNodeId"]);
                    }

                    if (relatedNodeId.HasValue && !string.IsNullOrWhiteSpace(relationType))
                    {
                        // create relation
                        var createRelCypher = $@"
                            MATCH (a), (b)
                            WHERE id(a) = $nodeId AND id(b) = $relatedNodeId
                            CREATE (a)-[r:{relationType}]->(b)
                            RETURN id(r) as relId";
                        var relResult = await tx.RunAsync(createRelCypher, new { nodeId, relatedNodeId });
                        await relResult.FetchAsync();
                        var relId = Convert.ToInt64(relResult.Current["relId"]);
                        await tx.CommitAsync();
                        return JsonSerializer.Serialize(new
                        {
                            success = true,
                            nodeId,
                            relatedNodeId,
                            relId,
                            timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC")
                        }, new JsonSerializerOptions { WriteIndented = true });
                    }
                    else
                    {
                        await tx.CommitAsync();
                        return JsonSerializer.Serialize(new
                        {
                            success = true,
                            nodeId,
                            relatedNodeId,
                            relId = (long?)null,
                            timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC")
                        }, new JsonSerializerOptions { WriteIndented = true });
                    }
                }
                catch (Exception ex)
                {
                    await tx.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new
                {
                    success = false,
                    error = $"Failed to write to Neo4j: {ex.Message}",
                    timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC")
                }, new JsonSerializerOptions { WriteIndented = true });
            }
        }

        // -------------------- general node operation --------------------

        /// <summary>
        /// Create a node with one or more labels and properties
        /// 创建带标签和属性的节点
        /// ラベルとプロパティ付きノードを作成
        /// </summary>
        /// <param name="neo4jDriver">Neo4j driver (auto-injected)</param>
        /// <param name="labels">Node labels (multiple allowed)</param>
        /// <param name="propertiesJson">Node properties as JSON string (can include description)</param>
        /// <returns>Creation result (node id)</returns>
        [McpServerTool, Description("Create a node with one or more labels and properties. Returns node id.")]
        public static async Task<string> CreateNode(
            [Description("Neo4j driver (auto-injected)")] Neo4j.Driver.IDriver neo4jDriver,
            [Description("Node labels (multiple allowed)")] string[] labels,
            [Description("Node properties as JSON string, e.g. '{\"name\":\"Alice\",\"description\":\"A test user\"}'")] string propertiesJson)
        {
            try
            {
                var props = JsonSerializer.Deserialize<Dictionary<string, object>>(propertiesJson);
                var labelString = string.Join(":", labels);
                using var session = neo4jDriver.AsyncSession();
                var result = await session.RunAsync($"CREATE (n:{labelString} $props) RETURN id(n) as nodeId", new { props });
                await result.FetchAsync();
                var nodeId = Convert.ToInt64(result.Current["nodeId"]);
                return JsonSerializer.Serialize(new { success = true, nodeId }, new JsonSerializerOptions { WriteIndented = true });
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new { success = false, error = ex.Message }, new JsonSerializerOptions { WriteIndented = true });
            }
        }

        /// <summary>
        /// Get a node by id
        /// 根据ID查询节点
        /// IDでノードを取得
        /// </summary>
        /// <param name="neo4jDriver">Neo4j driver (auto-injected)</param>
        /// <param name="labels">Node labels (multiple allowed)</param>
        /// <param name="id">Node id</param>
        /// <returns>Node properties</returns>
        [McpServerTool, Description("Get a node by id. Returns node properties.")]
        public static async Task<string> GetNodeById(
            [Description("Neo4j driver (auto-injected)")] Neo4j.Driver.IDriver neo4jDriver,
            [Description("Node labels (multiple allowed)")] string[] labels,
            [Description("Node id")] long id)
        {
            try
            {
                var labelString = string.Join(":", labels);
                using var session = neo4jDriver.AsyncSession();
                var result = await session.RunAsync($"MATCH (n:{labelString}) WHERE id(n) = $id RETURN properties(n) as props", new { id });
                await result.FetchAsync();
                if (!result.Current.Keys.Contains("props"))
                    return JsonSerializer.Serialize(new { success = false, error = "Node not found" }, new JsonSerializerOptions { WriteIndented = true });
                var props = result.Current["props"];
                return JsonSerializer.Serialize(new { success = true, id, properties = props }, new JsonSerializerOptions { WriteIndented = true });
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new { success = false, error = ex.Message }, new JsonSerializerOptions { WriteIndented = true });
            }
        }

        /// <summary>
        /// Update a node by id
        /// 根据ID更新节点属性
        /// IDでノードのプロパティを更新
        /// </summary>
        /// <param name="neo4jDriver">Neo4j driver (auto-injected)</param>
        /// <param name="labels">Node labels (multiple allowed)</param>
        /// <param name="id">Node id</param>
        /// <param name="propertiesJson">New properties as JSON string</param>
        /// <returns>Update result</returns>
        [McpServerTool, Description("Update a node by id. Overwrites all properties.")]
        public static async Task<string> UpdateNodeById(
            [Description("Neo4j driver (auto-injected)")] Neo4j.Driver.IDriver neo4jDriver,
            [Description("Node labels (multiple allowed)")] string[] labels,
            [Description("Node id")] long id,
            [Description("New properties as JSON string")] string propertiesJson)
        {
            try
            {
                var props = JsonSerializer.Deserialize<Dictionary<string, object>>(propertiesJson);
                var labelString = string.Join(":", labels);
                using var session = neo4jDriver.AsyncSession();
                var result = await session.RunAsync($"MATCH (n:{labelString}) WHERE id(n) = $id SET n = $props RETURN id(n) as nodeId", new { id, props });
                await result.FetchAsync();
                var nodeId = Convert.ToInt64(result.Current["nodeId"]);
                return JsonSerializer.Serialize(new { success = true, nodeId }, new JsonSerializerOptions { WriteIndented = true });
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new { success = false, error = ex.Message }, new JsonSerializerOptions { WriteIndented = true });
            }
        }

        /// <summary>
        /// Delete a node by id
        /// 根据ID删除节点
        /// IDでノードを削除
        /// </summary>
        /// <param name="neo4jDriver">Neo4j driver (auto-injected)</param>
        /// <param name="labels">Node labels (multiple allowed)</param>
        /// <param name="id">Node id</param>
        /// <returns>Delete result</returns>
        [McpServerTool, Description("Delete a node by id. Also deletes relationships.")]
        public static async Task<string> DeleteNodeById(
            [Description("Neo4j driver (auto-injected)")] Neo4j.Driver.IDriver neo4jDriver,
            [Description("Node labels (multiple allowed)")] string[] labels,
            [Description("Node id")] long id)
        {
            try
            {
                var labelString = string.Join(":", labels);
                using var session = neo4jDriver.AsyncSession();
                var result = await session.RunAsync($"MATCH (n:{labelString}) WHERE id(n) = $id DETACH DELETE n RETURN $id as deletedId", new { id });
                await result.FetchAsync();
                var deletedId = Convert.ToInt64(result.Current["deletedId"]);
                return JsonSerializer.Serialize(new { success = true, deletedId }, new JsonSerializerOptions { WriteIndented = true });
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new { success = false, error = ex.Message }, new JsonSerializerOptions { WriteIndented = true });
            }
        }

        // -------------------- general relation operation --------------------

        /// <summary>
        /// Create a relationship between two nodes by id
        /// 根据ID在两个节点间创建关系
        /// 2つのノード間にリレーションを作成
        /// </summary>
        /// <param name="neo4jDriver">Neo4j driver (auto-injected)</param>
        /// <param name="fromId">From node id</param>
        /// <param name="toId">To node id</param>
        /// <param name="relationTypes">Relation types (multiple allowed, at least one required)</param>
        /// <param name="propertiesJson">Relation properties as JSON string (optional)</param>
        /// <returns>Relation id</returns>
        [McpServerTool, Description("Create a relationship between two nodes by id. Optionally set properties. Supports multiple relation types.")]
        public static async Task<string> CreateRelation(
            [Description("Neo4j driver (auto-injected)")] Neo4j.Driver.IDriver neo4jDriver,
            [Description("From node id")] long fromId,
            [Description("To node id")] long toId,
            [Description("Relation types (multiple allowed, at least one required)")] string[] relationTypes,
            [Description("Relation properties as JSON string (optional)")] string? propertiesJson = null)
        {
            try
            {
                var props = !string.IsNullOrWhiteSpace(propertiesJson)
                    ? JsonSerializer.Deserialize<Dictionary<string, object>>(propertiesJson)
                    : new Dictionary<string, object>();
                var relTypeString = string.Join(":", relationTypes);
                using var session = neo4jDriver.AsyncSession();
                var result = await session.RunAsync($@"MATCH (a), (b) WHERE id(a) = $fromId AND id(b) = $toId CREATE (a)-[r:{relTypeString} $props]->(b) RETURN id(r) as relId", new { fromId, toId, props });
                await result.FetchAsync();
                var relId = Convert.ToInt64(result.Current["relId"]);
                return JsonSerializer.Serialize(new { success = true, relId }, new JsonSerializerOptions { WriteIndented = true });
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new { success = false, error = ex.Message }, new JsonSerializerOptions { WriteIndented = true });
            }
        }

        /// <summary>
        /// Delete a relationship by id
        /// 根据ID删除关系
        /// IDでリレーションを削除
        /// </summary>
        /// <param name="neo4jDriver">Neo4j driver (auto-injected)</param>
        /// <param name="relId">Relation id</param>
        /// <returns>Delete result</returns>
        [McpServerTool, Description("Delete a relationship by id.")]
        public static async Task<string> DeleteRelationById(
            [Description("Neo4j driver (auto-injected)")] Neo4j.Driver.IDriver neo4jDriver,
            [Description("Relation id")] long relId)
        {
            try
            {
                using var session = neo4jDriver.AsyncSession();
                var result = await session.RunAsync($"MATCH ()-[r]->() WHERE id(r) = $relId DELETE r RETURN $relId as deletedRelId", new { relId });
                await result.FetchAsync();
                var deletedRelId = Convert.ToInt64(result.Current["deletedRelId"]);
                return JsonSerializer.Serialize(new { success = true, deletedRelId }, new JsonSerializerOptions { WriteIndented = true });
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new { success = false, error = ex.Message }, new JsonSerializerOptions { WriteIndented = true });
            }
        }

        /// <summary>
        /// Get all relationships for a node
        /// 查询节点的所有关系
        /// ノードの全リレーションを取得
        /// </summary>
        /// <param name="neo4jDriver">Neo4j driver (auto-injected)</param>
        /// <param name="nodeId">Node id</param>
        /// <returns>Relation list</returns>
        [McpServerTool, Description("Get all relationships for a node. Returns list of relation ids and types.")]
        public static async Task<string> GetRelationsForNode(
            [Description("Neo4j driver (auto-injected)")] Neo4j.Driver.IDriver neo4jDriver,
            [Description("Node id")] long nodeId)
        {
            try
            {
                using var session = neo4jDriver.AsyncSession();
                var result = await session.RunAsync($"MATCH (n)-[r]->() WHERE id(n) = $nodeId RETURN id(r) as relId, type(r) as relType, endNode(r) as toNodeId");
                var rels = new List<object>();
                while (await result.FetchAsync())
                {
                    rels.Add(new
                    {
                        relId = Convert.ToInt64(result.Current["relId"]),
                        relType = result.Current["relType"] as string,
                        toNodeId = Convert.ToInt64(result.Current["toNodeId"])
                    });
                }
                return JsonSerializer.Serialize(new { success = true, relations = rels }, new JsonSerializerOptions { WriteIndented = true });
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new { success = false, error = ex.Message }, new JsonSerializerOptions { WriteIndented = true });
            }
        }

        /// <summary>
        /// Get nodes by labels with optional pagination
        /// 根据标签查询节点，支持分页
        /// ラベルでノードを取得（ページネーション対応）
        /// </summary>
        /// <param name="neo4jDriver">Neo4j driver (auto-injected)</param>
        /// <param name="labels">Node labels (multiple allowed)</param>
        /// <param name="skip">Number of records to skip (for pagination)</param>
        /// <param name="limit">Max number of records to return</param>
        /// <returns>List of nodes (id + properties)</returns>
        [McpServerTool, Description("Get nodes by labels with optional pagination.")]
        public static async Task<string> GetNodesByLabels(
            [Description("Neo4j driver (auto-injected)")] Neo4j.Driver.IDriver neo4jDriver,
            [Description("Node labels (multiple allowed)")] string[] labels,
            [Description("Skip count (pagination)")] int skip = 0,
            [Description("Limit count (pagination)")] int limit = 100)
        {
            try
            {
                var labelString = string.Join(":", labels);
                using var session = neo4jDriver.AsyncSession();
                var result = await session.RunAsync($"MATCH (n:{labelString}) RETURN id(n) as id, properties(n) as props SKIP $skip LIMIT $limit", new { skip, limit });
                var nodes = new List<object>();
                while (await result.FetchAsync())
                {
                    nodes.Add(new { id = Convert.ToInt64(result.Current["id"]), properties = result.Current["props"] });
                }
                return JsonSerializer.Serialize(new { success = true, nodes }, new JsonSerializerOptions { WriteIndented = true });
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new { success = false, error = ex.Message }, new JsonSerializerOptions { WriteIndented = true });
            }
        }

        /// <summary>
        /// Find nodes by property (exact or fuzzy match)
        /// 根据属性查找节点（精确或模糊匹配）
        /// プロパティでノードを検索（完全一致または部分一致）
        /// </summary>
        /// <param name="neo4jDriver">Neo4j driver (auto-injected)</param>
        /// <param name="labels">Node labels (multiple allowed)</param>
        /// <param name="propertyKey">Property key</param>
        /// <param name="propertyValue">Property value</param>
        /// <param name="exactMatch">Exact match if true, fuzzy (CONTAINS) if false</param>
        /// <param name="skip">Number of records to skip (for pagination)</param>
        /// <param name="limit">Max number of records to return</param>
        /// <returns>List of nodes (id + properties)</returns>
        [McpServerTool, Description("Find nodes by property (exact or fuzzy match). Supports pagination.")]
        public static async Task<string> FindNodesByProperty(
            [Description("Neo4j driver (auto-injected)")] Neo4j.Driver.IDriver neo4jDriver,
            [Description("Node labels (multiple allowed)")] string[] labels,
            [Description("Property key")] string propertyKey,
            [Description("Property value")] string propertyValue,
            [Description("Exact match (true) or fuzzy match (false)")] bool exactMatch = true,
            [Description("Skip count (pagination)")] int skip = 0,
            [Description("Limit count (pagination)")] int limit = 100)
        {
            try
            {
                var labelString = string.Join(":", labels);
                var op = exactMatch ? "=" : "CONTAINS";
                var cypher = $"MATCH (n:{labelString}) WHERE n[$propertyKey] {op} $propertyValue RETURN id(n) as id, properties(n) as props SKIP $skip LIMIT $limit";
                using var session = neo4jDriver.AsyncSession();
                var result = await session.RunAsync(cypher, new { propertyKey, propertyValue, skip, limit });
                var nodes = new List<object>();
                while (await result.FetchAsync())
                {
                    nodes.Add(new { id = Convert.ToInt64(result.Current["id"]), properties = result.Current["props"] });
                }
                return JsonSerializer.Serialize(new { success = true, nodes }, new JsonSerializerOptions { WriteIndented = true });
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new { success = false, error = ex.Message }, new JsonSerializerOptions { WriteIndented = true });
            }
        }

        /// <summary>
        /// Get related nodes for a given node (optionally filter by relation types and direction)
        /// 查询节点的相关节点（可按关系类型和方向过滤）
        /// ノードの関連ノードを取得（リレーションタイプ・方向でフィルタ可能）
        /// </summary>
        /// <param name="neo4jDriver">Neo4j driver (auto-injected)</param>
        /// <param name="nodeId">Node id</param>
        /// <param name="relationTypes">Relation types (optional, multiple allowed)</param>
        /// <param name="direction">Direction: 'out', 'in', or 'both' (default)</param>
        /// <returns>List of related nodes (id + properties + relation id/type)</returns>
        [McpServerTool, Description("Get related nodes for a given node. Filter by relation types and direction.")]
        public static async Task<string> GetRelatedNodes(
            [Description("Neo4j driver (auto-injected)")] Neo4j.Driver.IDriver neo4jDriver,
            [Description("Node id")] long nodeId,
            [Description("Relation types (optional, multiple allowed)")] string[]? relationTypes = null,
            [Description("Direction: 'out', 'in', or 'both'")] string direction = "both")
        {
            try
            {
                string relTypeString = relationTypes != null && relationTypes.Length > 0 ? ":" + string.Join(":", relationTypes) : "";
                string pattern = direction switch
                {
                    "out" => $"(n)-[r{relTypeString}]->(m)",
                    "in" => $"(n)<-[r{relTypeString}]-(m)",
                    _ => $"(n)-[r{relTypeString}]-(m)"
                };
                var cypher = $"MATCH {pattern} WHERE id(n) = $nodeId RETURN id(m) as id, properties(m) as props, id(r) as relId, type(r) as relType";
                using var session = neo4jDriver.AsyncSession();
                var result = await session.RunAsync(cypher, new { nodeId });
                var nodes = new List<object>();
                while (await result.FetchAsync())
                {
                    nodes.Add(new {
                        id = Convert.ToInt64(result.Current["id"]),
                        properties = result.Current["props"],
                        relId = Convert.ToInt64(result.Current["relId"]),
                        relType = result.Current["relType"] as string
                    });
                }
                return JsonSerializer.Serialize(new { success = true, relatedNodes = nodes }, new JsonSerializerOptions { WriteIndented = true });
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new { success = false, error = ex.Message }, new JsonSerializerOptions { WriteIndented = true });
            }
        }

        /// <summary>
        /// Get all relations (optionally filter by type or property)
        /// 查询所有关系（可按类型或属性过滤）
        /// すべてのリレーションを取得（タイプ・プロパティでフィルタ可能）
        /// </summary>
        /// <param name="neo4jDriver">Neo4j driver (auto-injected)</param>
        /// <param name="relationTypes">Relation types (optional, multiple allowed)</param>
        /// <param name="propertyKey">Property key (optional)</param>
        /// <param name="propertyValue">Property value (optional)</param>
        /// <param name="exactMatch">Exact match if true, fuzzy (CONTAINS) if false</param>
        /// <param name="skip">Number of records to skip (for pagination)</param>
        /// <param name="limit">Max number of records to return</param>
        /// <returns>List of relations (id, type, from, to, properties)</returns>
        [McpServerTool, Description("Get all relations, optionally filter by type or property. Supports pagination.")]
        public static async Task<string> GetRelations(
            [Description("Neo4j driver (auto-injected)")] Neo4j.Driver.IDriver neo4jDriver,
            [Description("Relation types (optional, multiple allowed)")] string[]? relationTypes = null,
            [Description("Property key (optional)")] string? propertyKey = null,
            [Description("Property value (optional)")] string? propertyValue = null,
            [Description("Exact match (true) or fuzzy match (false)")] bool exactMatch = true,
            [Description("Skip count (pagination)")] int skip = 0,
            [Description("Limit count (pagination)")] int limit = 100)
        {
            try
            {
                string relTypeString = relationTypes != null && relationTypes.Length > 0 ? ":" + string.Join(":", relationTypes) : "";
                string where = "";
                if (!string.IsNullOrWhiteSpace(propertyKey) && propertyValue != null)
                {
                    var op = exactMatch ? "=" : "CONTAINS";
                    where = $"WHERE r[$propertyKey] {op} $propertyValue";
                }
                var cypher = $"MATCH ()-[r{relTypeString}]->() {where} RETURN id(r) as relId, type(r) as relType, id(startNode(r)) as fromId, id(endNode(r)) as toId, properties(r) as props SKIP $skip LIMIT $limit";
                using var session = neo4jDriver.AsyncSession();
                var result = await session.RunAsync(cypher, new { propertyKey, propertyValue, skip, limit });
                var rels = new List<object>();
                while (await result.FetchAsync())
                {
                    rels.Add(new {
                        relId = Convert.ToInt64(result.Current["relId"]),
                        relType = result.Current["relType"] as string,
                        fromId = Convert.ToInt64(result.Current["fromId"]),
                        toId = Convert.ToInt64(result.Current["toId"]),
                        properties = result.Current["props"]
                    });
                }
                return JsonSerializer.Serialize(new { success = true, relations = rels }, new JsonSerializerOptions { WriteIndented = true });
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new { success = false, error = ex.Message }, new JsonSerializerOptions { WriteIndented = true });
            }
        }

        /// <summary>
        /// Run a custom Cypher pattern query (advanced)
        /// 自定义 Cypher 图模式查询（高级用法）
        /// カスタム Cypher パターン検索（上級）
        /// </summary>
        /// <param name="neo4jDriver">Neo4j driver (auto-injected)</param>
        /// <param name="cypher">Cypher query string (must be a read query)</param>
        /// <param name="parametersJson">Parameters as JSON string</param>
        /// <returns>Raw query result</returns>
        [McpServerTool, Description("Run a custom Cypher pattern query (advanced, read-only). Parameters as JSON string.")]
        public static async Task<string> RunCustomPatternQuery(
            [Description("Neo4j driver (auto-injected)")] Neo4j.Driver.IDriver neo4jDriver,
            [Description("Cypher query string (read-only)")] string cypher,
            [Description("Parameters as JSON string")] string? parametersJson = null)
        {
            try
            {
                var parameters = !string.IsNullOrWhiteSpace(parametersJson)
                    ? JsonSerializer.Deserialize<Dictionary<string, object>>(parametersJson)
                    : new Dictionary<string, object>();
                using var session = neo4jDriver.AsyncSession();
                var result = await session.RunAsync(cypher, parameters);
                var records = new List<object>();
                while (await result.FetchAsync())
                {
                    var dict = new Dictionary<string, object>();
                    foreach (var key in result.Current.Keys)
                        dict[key] = result.Current[key];
                    records.Add(dict);
                }
                return JsonSerializer.Serialize(new { success = true, records }, new JsonSerializerOptions { WriteIndented = true });
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new { success = false, error = ex.Message }, new JsonSerializerOptions { WriteIndented = true });
            }
        }
    }
} 