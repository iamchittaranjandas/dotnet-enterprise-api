using DotnetEnterpriseApi.Application.Common.Interfaces;

namespace DotnetEnterpriseApi.Infrastructure.Data.Dialects
{
    public class SqlServerDialect : ISqlDialect
    {
        public string InsertReturningId(string table, string columns, string values)
            => $"INSERT INTO {table} ({columns}) OUTPUT INSERTED.Id VALUES ({values})";

        public string PaginateQuery(string selectColumns, string fromClause, string? whereClause, string orderByClause)
        {
            var where = whereClause is not null ? $"WHERE {whereClause}" : "";
            return $"SELECT TOP (@PageSize) {selectColumns} FROM {fromClause} {where} {orderByClause}";
        }
    }
}
