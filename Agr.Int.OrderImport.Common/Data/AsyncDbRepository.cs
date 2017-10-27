using System;
using System.Data.Common;
using System.Threading.Tasks;
using Common.Logging;

namespace Agr.Int.OrderImport.Common.Data
{
    public abstract class AsyncDbRepository
    {
        /// <summary>
        /// The logger
        /// </summary>
        protected readonly ILog Logger;

        /// <summary>
        /// The connection factory
        /// </summary>
        private readonly IDbConnectionFactory connectionFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncDbRepository"/> class and gets the default connection.
        /// </summary>
        /// <param name="connectionFactory">The connection factory.</param>
        protected AsyncDbRepository(IDbConnectionFactory connectionFactory)
        {
            this.Logger = LogManager.GetLogger(this.GetType());
            this.Logger.Trace(m => m($"ctor started"));

            if (connectionFactory == null)
            {
                this.Logger.Error(m => m($"{nameof(connectionFactory)} not injected"));
                throw new ArgumentNullException(nameof(connectionFactory));
            }

            this.connectionFactory = connectionFactory;

            this.Logger.Trace(m => m($"ctor finished"));

        }

        protected async Task<T> WithConnection<T>(Func<DbConnection, Task<T>> getData)
        {
            this.Logger.Trace(m => m($"WithConnection<T> started"));

            using (var connection = this.connectionFactory.GetConnection())
            {
                this.Logger.Trace(m => m($"WithConnection<T> retrieved a connection"));
                try
                {
                    await connection.OpenAsync();
                }
                catch (Exception exception)
                {
                    this.Logger.Error(m => m($"WithConnection<T> encountered an error: {exception.Message}"), exception);
                    throw;
                }

                this.Logger.Trace(m => m($"WithConnection<T> opened a connection"));
                return await getData(connection);
            }
        }

        protected async Task WithConnection(Func<DbConnection, Task> getData)
        {
            this.Logger.Trace(m => m($"WithConnection started"));

            using (var connection = this.connectionFactory.GetConnection())
            {
                this.Logger.Trace(m => m($"WithConnection retrieved a connection"));

                try
                {
                    await connection.OpenAsync();
                    this.Logger.Trace(m => m($"WithConnection opened a connection"));
                    await getData(connection);
                }
                catch (Exception exception)
                {
                    this.Logger.Error(m => m($"WithConnection encountered an error: {exception.Message}"), exception);
                    throw;
                }

                this.Logger.Trace(m => m($"WithConnection executed the getData Func"));
            }

            this.Logger.Trace(m => m($"WithConnection finished"));
        }
    }
}
