using Microsoft.Data.SqlClient;
using ShopAPI.DTOClasses.Auth;
using System.Data;

namespace ShopAPI.Repositories
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly string _connectionString;

        public RefreshTokenRepository(IConfiguration configuration)
        {
            _connectionString = configuration
                .GetConnectionString("ShopDbConnection")
                ?? throw new InvalidOperationException(
                    "ConnectionStrings:ShopDbConnection is missing.");
        }

        public async Task<int> AddAsync(
            string tokenHash,
            int personId,
            DateTime expiresAt)
        {
            await using var connection =
                new SqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new SqlCommand(
                "dbo.SP_AddRefreshToken",
                connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.Add(
                "@TokenHash", SqlDbType.Char, 64).Value = tokenHash;
            command.Parameters.Add(
                "@Person_ID", SqlDbType.Int).Value = personId;
            command.Parameters.Add(
                "@ExpiresAt", SqlDbType.DateTime2).Value = expiresAt;

            var newId = command.Parameters.Add(
                "@NewID", SqlDbType.Int);
            newId.Direction = ParameterDirection.Output;

            await command.ExecuteNonQueryAsync();
            return Convert.ToInt32(newId.Value);
        }

        public async Task<RefreshTokenRecordDTO>
            GetByTokenHashAsync(string tokenHash)
        {
            const string sql = """
                SELECT Token_ID, Person_ID, ExpiresAt,
                       CreatedAt, RevokedAt
                FROM dbo.RefreshTokens
                WHERE TokenHash = @TokenHash;
                """;

            await using var connection =
                new SqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new SqlCommand(sql, connection);
            command.Parameters.Add(
                "@TokenHash", SqlDbType.Char, 64).Value = tokenHash;

            await using var reader =
                await command.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
                return null;

            return new RefreshTokenRecordDTO
            {
                Token_ID = Convert.ToInt32(reader["Token_ID"]),
                Person_ID = Convert.ToInt32(reader["Person_ID"]),
                ExpiresAt = Convert.ToDateTime(reader["ExpiresAt"]),
                CreatedAt = Convert.ToDateTime(reader["CreatedAt"]),
                RevokedAt = reader["RevokedAt"] is DBNull
                    ? null
                    : Convert.ToDateTime(reader["RevokedAt"])
            };
        }

        public async Task<int?> RotateAsync(
            string oldTokenHash,
            string newTokenHash,
            DateTime newExpiresAt)
        {
            await using var connection =
                new SqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new SqlCommand(
                "dbo.SP_RotateRefreshToken",
                connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.Add(
                "@OldTokenHash", SqlDbType.Char, 64).Value = oldTokenHash;
            command.Parameters.Add(
                "@NewTokenHash", SqlDbType.Char, 64).Value = newTokenHash;
            command.Parameters.Add(
                "@NewExpiresAt", SqlDbType.DateTime2).Value = newExpiresAt;

            var newTokenId = command.Parameters.Add(
                "@NewToken_ID", SqlDbType.Int);
            newTokenId.Direction = ParameterDirection.Output;

            var personId = command.Parameters.Add(
                "@Person_ID", SqlDbType.Int);
            personId.Direction = ParameterDirection.Output;

            await command.ExecuteNonQueryAsync();

            if (personId.Value is DBNull || personId.Value is null)
                return null;

            return Convert.ToInt32(personId.Value);
        }

        public async Task<bool> RevokeAsync(
            string tokenHash,
            string replacedByTokenHash = null)
        {
            await using var connection =
                new SqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new SqlCommand(
                "dbo.SP_RevokeRefreshToken",
                connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.Add(
                "@TokenHash", SqlDbType.Char, 64).Value = tokenHash;
            command.Parameters.Add(
                "@ReplacedByTokenHash", SqlDbType.Char, 64).Value =
                (object)replacedByTokenHash ?? DBNull.Value;

            var rowsAffected = command.Parameters.Add(
                "@RowsAffected", SqlDbType.Int);
            rowsAffected.Direction = ParameterDirection.Output;

            await command.ExecuteNonQueryAsync();
            return Convert.ToInt32(rowsAffected.Value) > 0;
        }

        public async Task<bool> RevokeAllForPersonAsync(int personId)
        {
            await using var connection =
                new SqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new SqlCommand(
                "dbo.SP_RevokeAllRefreshTokensForPerson",
                connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.Add(
                "@Person_ID", SqlDbType.Int).Value = personId;

            var rowsAffected = command.Parameters.Add(
                "@RowsAffected", SqlDbType.Int);
            rowsAffected.Direction = ParameterDirection.Output;

            await command.ExecuteNonQueryAsync();
            return Convert.ToInt32(rowsAffected.Value) > 0;
        }
    }
}
