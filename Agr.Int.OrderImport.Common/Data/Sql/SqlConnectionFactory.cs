using System;
using System.Configuration;
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using Common.Logging;

namespace Agr.Int.OrderImport.Common.Data.Sql
{
    /// <summary>
    ///		Creates and returns the specified connection or if omitted, returns one specified in configuration.
    /// </summary>
    /// <remarks>
    ///     <example>
    ///         Configuration:
    ///         <code>
    ///				<connectionStrings>
    ///					<add name="Default" connectionString="Server=myServerAddress;Database=myDataBase;Trusted_Connection=True;"/>
    ///				</connectionStrings>
    ///			</code>
    ///     </example>
    ///     <example>
    ///         Configuration:
    ///         <code>
    ///				<appSettings>
    ///					<add key="ConnectionName" value="MyConnection"/>
    ///				 </appSettings>
    ///				<connectionStrings>
    ///					<add name="MyConnection" connectionString="Server=myServerAddress;Database=myDataBase;Trusted_Connection=True;"/>
    ///				</connectionStrings>
    ///			</code>
    ///     </example>
    /// </remarks>
    [ExcludeFromCodeCoverage]
    public class SqlConnectionFactory : IDbConnectionFactory
    {
        private const string DefaultConnectionName = "Default";
        private readonly ILog logger;

        public SqlConnectionFactory()
        {
            this.logger = LogManager.GetLogger<SqlConnectionFactory>();
        }

        public DbConnection GetConnection(string connectionName = null)
        {
            connectionName = connectionName ?? ConfigurationManager.AppSettings["ConnectionName"] ?? DefaultConnectionName;
            logger.Trace(m => m($"Using connection name {connectionName}"));

            string connectionString = ConfigurationManager.ConnectionStrings[connectionName]?.ConnectionString;

            if (String.IsNullOrWhiteSpace(connectionString))
                throw new Exception($"Could not create a connection string as no connection string named '{connectionName}' was found");

            return new SqlConnection(connectionString);
        }
    }
}
