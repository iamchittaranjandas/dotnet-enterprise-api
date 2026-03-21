using DotnetEnterpriseApi.Application.Common.Interfaces;

namespace DotnetEnterpriseApi.Infrastructure.Repositories.Queries
{
    public static class RefreshTokenQueries
    {
        public const string SelectColumns = "Id, Token, UserId, ExpiresAt, CreatedAt, IsRevoked";
        public const string Table = "RefreshTokens";
        public const string InsertColumns = "Token, UserId, ExpiresAt, CreatedAt, IsRevoked";
        public const string InsertValues = "@Token, @UserId, @ExpiresAt, @CreatedAt, @IsRevoked";

        public static string GetByToken(ISqlDialect dialect)
            => dialect.FormatSql($"SELECT {SelectColumns} FROM {Table} WHERE Token = @Token");

        public static string Insert(ISqlDialect dialect)
            => dialect.FormatSql(dialect.InsertReturningId(Table, InsertColumns, InsertValues));

        public static string Revoke(ISqlDialect dialect)
            => dialect.FormatSql($"UPDATE {Table} SET IsRevoked = @IsRevoked WHERE Token = @Token");
    }
}
