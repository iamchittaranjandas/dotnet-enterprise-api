namespace DotnetEnterpriseApi.Application.Common.Interfaces
{
    public interface ISqlDialect
    {
        string InsertReturningId(string table, string columns, string values);

        string PaginateQuery(string selectColumns, string fromClause, string? whereClause, string orderByClause);

        /// <summary>
        /// True for Oracle which uses RETURNING INTO with output parameters.
        /// </summary>
        bool RequiresOutputParameterForInsert { get; }

        /// <summary>
        /// Transforms SQL parameter prefixes. Oracle uses ':' instead of '@'.
        /// </summary>
        string FormatSql(string sql);
    }
}
