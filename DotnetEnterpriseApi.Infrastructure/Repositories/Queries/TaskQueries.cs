using DotnetEnterpriseApi.Application.Common.Interfaces;

namespace DotnetEnterpriseApi.Infrastructure.Repositories.Queries
{
    public static class TaskQueries
    {
        public const string SelectColumns = "Id, Title, Description, IsCompleted, CreatedDate";
        public const string Table = "Tasks";
        public const string InsertColumns = "Title, Description, IsCompleted, CreatedDate";
        public const string InsertValues = "@Title, @Description, @IsCompleted, @CreatedDate";

        public static string GetById(ISqlDialect dialect)
            => dialect.FormatSql($"SELECT {SelectColumns} FROM {Table} WHERE Id = @Id");

        public static string GetAllPaginated(ISqlDialect dialect, bool hasCursor)
            => dialect.FormatSql(
                hasCursor
                    ? dialect.PaginateQuery(SelectColumns, Table, "Id < @Cursor", "ORDER BY Id DESC")
                    : dialect.PaginateQuery(SelectColumns, Table, null, "ORDER BY Id DESC"));

        public static string Insert(ISqlDialect dialect)
            => dialect.FormatSql(dialect.InsertReturningId(Table, InsertColumns, InsertValues));

        public static string Update(ISqlDialect dialect)
            => dialect.FormatSql($"UPDATE {Table} SET Title = @Title, Description = @Description, IsCompleted = @IsCompleted WHERE Id = @Id");

        public static string Delete(ISqlDialect dialect)
            => dialect.FormatSql($"DELETE FROM {Table} WHERE Id = @Id");
    }
}
