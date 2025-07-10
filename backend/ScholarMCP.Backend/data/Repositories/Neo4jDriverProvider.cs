using Neo4j.Driver;

namespace ScholarMCP.Data.Repositories
{
    /// <summary>
    /// Neo4j Driver Provider
    /// Neo4j 驱动提供者
    /// Neo4jドライバープロバイダー
    /// </summary>
    public class Neo4jDriverProvider
    {
        private readonly IDriver _driver;

        /// <summary>
        /// 构造函数 / Constructor / コンストラクタ
        /// </summary>
        public Neo4jDriverProvider(string uri, string user, string password)
        {
            _driver = GraphDatabase.Driver(uri, AuthTokens.Basic(user, password));
        }

        /// <summary>
        /// 获取驱动实例 / Get driver instance / ドライバーインスタンス取得
        /// </summary>
        public IDriver GetDriver() => _driver;
    }
} 