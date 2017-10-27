using System.Data.Common;
namespace Agr.Int.OrderImport.Common.Data
{
    public interface IDbConnectionFactory
    {
        DbConnection GetConnection(string connectionName = null);
    }
}
