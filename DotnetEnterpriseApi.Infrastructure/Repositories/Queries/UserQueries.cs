using DotnetEnterpriseApi.Application.Common.Interfaces;

namespace DotnetEnterpriseApi.Infrastructure.Repositories.Queries
{
    public static class UserQueries
    {
        public const string SelectColumns = "Id, UserName, Email, PasswordHash, Role";
        public const string Table = "Users";
        public const string InsertColumns = "UserName, Email, PasswordHash, Role";
        public const string InsertValues = "@UserName, @Email, @PasswordHash, @Role";

        public static string GetById(ISqlDialect dialect)
            => dialect.FormatSql($"SELECT {SelectColumns} FROM {Table} WHERE Id = @Id");

        public static string GetByEmail(ISqlDialect dialect)
            => dialect.FormatSql($"SELECT {SelectColumns} FROM {Table} WHERE Email = @Email");

        public static string ExistsByEmail(ISqlDialect dialect)
            => dialect.FormatSql($"SELECT COUNT(1) FROM {Table} WHERE Email = @Email");

        public static string Insert(ISqlDialect dialect)
            => dialect.FormatSql(dialect.InsertReturningId(Table, InsertColumns, InsertValues));
    }
}
