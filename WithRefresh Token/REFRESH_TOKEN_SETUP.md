# Refresh Token setup

## Required User Secrets

Configure the real database connection and JWT key outside Git:

```bash
dotnet user-secrets set "ConnectionStrings:ShopDbConnection" "<connection-string>"
dotnet user-secrets set "JwtSettings:Key" "<random-secret-at-least-32-characters>"
```

`JwtSettings:RefreshTokenExpiryDays` defaults to 7 in `appsettings.json` and may also be overridden with User Secrets.

## Database

The supplied `database.sql` already defines:

- `RefreshTokens.TokenHash CHAR(64)`
- `RefreshTokens.ReplacedByTokenHash CHAR(64)`
- `UX_RefreshTokens_TokenHash`
- Hash-based add, revoke, revoke-all, and atomic rotation procedures

For an existing database, run `Database/RefreshTokenHashMigration.sql` only after confirming every existing row has a `TokenHash`. The migration removes the legacy raw-token columns after the API has been switched to Hash mode.

## Token lifecycle

- The raw refresh token is generated once and sent only in an `HttpOnly`, `Secure` cookie.
- SHA-256 of the raw token is stored and queried in SQL Server.
- Refresh rotation sends the old and new hashes to `SP_RotateRefreshToken`.
- The procedure locks the old row and atomically revokes it and inserts the new row.
- Reuse of a revoked token revokes all refresh tokens for that person.
- Logout hashes the cookie value before revocation.

## Test order

1. Run the API over HTTPS.
2. Login and confirm the cookie exists at the configured path `/api/Auth`.
3. Confirm the newest database row has `TokenHash` populated and `Token` is not used by the application.
4. Refresh once: expect `200` and a new cookie.
5. Reuse the old cookie: expect `401`; the token family is revoked.
6. Logout: expect the cookie to be deleted.
7. For concurrency, send two refresh requests using the same cookie: at most one should return `200`.
