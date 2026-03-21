using DotnetEnterpriseApi.Application.Common.Interfaces;

namespace DotnetEnterpriseApi.Infrastructure.Data.Dialects
{
    public class MySqlDialect : ISqlDialect
    {
        public string InsertReturningId(string table, string columns, string values)
            => $"INSERT INTO {table} ({columns}) VALUES ({values}); SELECT LAST_INSERT_ID()";

        public string PaginateQuery(string selectColumns, string fromClause, string? whereClause, string orderByClause)
        {
            var where = whereClause is not null ? $"WHERE {whereClause}" : "";
            return $"SELECT {selectColumns} FROM {fromClause} {where} {orderByClause} LIMIT @PageSize";
        }

        public bool RequiresOutputParameterForInsert => false;

        public string FormatSql(string sql) => sql;
    }
}
