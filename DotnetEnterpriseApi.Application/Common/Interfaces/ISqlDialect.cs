namespace DotnetEnterpriseApi.Application.Common.Interfaces
{
    public interface ISqlDialect
    {
        string InsertReturningId(string table, string columns, string values);

        string PaginateQuery(string selectColumns, string fromClause, string? whereClause, string orderByClause);
    }
}
