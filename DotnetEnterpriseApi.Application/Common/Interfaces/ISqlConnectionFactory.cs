using System.Data;

namespace DotnetEnterpriseApi.Application.Common.Interfaces
{
    public interface ISqlConnectionFactory
    {
        IDbConnection CreateConnection();
    }
}
