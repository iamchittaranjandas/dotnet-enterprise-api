using DotnetEnterpriseApi.Application.Common.Interfaces;

namespace DotnetEnterpriseApi.Infrastructure.Data.Dialects
{
    public class PostgreSqlDialect : ISqlDialect
    {
        public string InsertReturningId(string table, string columns, string values)
            => $"INSERT INTO {table} ({columns}) VALUES ({values}) RETURNING \"Id\"";

        public string PaginateQuery(string selectColumns, string fromClause, string? whereClause, string orderByClause)
        {
            var where = whereClause is not null ? $"WHERE {whereClause}" : "";
            return $"SELECT {selectColumns} FROM {fromClause} {where} {orderByClause} LIMIT @PageSize";
        }
    }
}
