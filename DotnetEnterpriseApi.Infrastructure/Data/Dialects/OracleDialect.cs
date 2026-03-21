using DotnetEnterpriseApi.Application.Common.Interfaces;

namespace DotnetEnterpriseApi.Infrastructure.Data.Dialects
{
    public class OracleDialect : ISqlDialect
    {
        public string InsertReturningId(string table, string columns, string values)
            => $"INSERT INTO {table} ({columns}) VALUES ({values}) RETURNING Id INTO :NewId";

        public string PaginateQuery(string selectColumns, string fromClause, string? whereClause, string orderByClause)
        {
            var where = whereClause is not null ? $"WHERE {whereClause}" : "";
            return $"SELECT {selectColumns} FROM {fromClause} {where} {orderByClause} FETCH FIRST :PageSize ROWS ONLY";
        }

        public bool RequiresOutputParameterForInsert => true;

        public string FormatSql(string sql) => sql.Replace('@', ':');
    }
}
