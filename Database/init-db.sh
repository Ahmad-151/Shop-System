#!/usr/bin/env bash
set -euo pipefail

SQLCMD="/opt/mssql-tools18/bin/sqlcmd"
MARKER_NAME="DockerInitializationCompleted"

echo "Waiting for SQL Server to become ready..."

until "$SQLCMD" \
  -S db,1433 \
  -U sa \
  -P "$MSSQL_SA_PASSWORD" \
  -No \
  -b \
  -Q "SELECT 1" \
  >/dev/null 2>&1
do
  sleep 2
done

echo "SQL Server is ready. Checking database initialization state..."

DATABASE_STATE=$(
  "$SQLCMD" \
    -S db,1433 \
    -U sa \
    -P "$MSSQL_SA_PASSWORD" \
    -No \
    -b \
    -h -1 \
    -W \
    -Q "
      SET NOCOUNT ON;

      IF DB_ID(N'ShopDB') IS NULL
      BEGIN
          SELECT 0;
      END
      ELSE IF EXISTS
      (
          SELECT 1
          FROM ShopDB.sys.extended_properties
          WHERE class = 0
            AND name = N'${MARKER_NAME}'
            AND CONVERT(nvarchar(10), value) = N'1'
      )
      BEGIN
          SELECT 1;
      END
      ELSE IF
      (
          SELECT COUNT(*)
          FROM ShopDB.sys.tables
      ) = 14
      AND
      (
          SELECT COUNT(*)
          FROM ShopDB.sys.procedures
      ) = 20
      BEGIN
          SELECT 2;
      END
      ELSE
      BEGIN
          SELECT 3;
      END
    " | tr -d '[:space:]'
)

case "$DATABASE_STATE" in
  1)
    echo "ShopDB is already initialized. Skipping database initialization."
    exit 0
    ;;
  2)
    echo "ShopDB has the expected schema. Recording its initialization marker..."
    ;;
  3)
    echo "ShopDB exists, but its initialization state is incomplete or unexpected."
    echo "The SQL script will not be rerun automatically."
    exit 1
    ;;
  0)
    echo "ShopDB does not exist. Running the initialization script..."

    "$SQLCMD" \
      -S db,1433 \
      -U sa \
      -P "$MSSQL_SA_PASSWORD" \
      -No \
      -b \
      -i /scripts/ShopDB.Docker.sql
    ;;
  *)
    echo "Could not determine the ShopDB initialization state."
    exit 1
    ;;
esac

echo "Recording the successful initialization marker..."

"$SQLCMD" \
  -S db,1433 \
  -U sa \
  -P "$MSSQL_SA_PASSWORD" \
  -No \
  -b \
  -d ShopDB \
  -Q "
    SET NOCOUNT ON;

    IF NOT EXISTS
    (
        SELECT 1
        FROM sys.extended_properties
        WHERE class = 0
          AND name = N'${MARKER_NAME}'
          AND CONVERT(nvarchar(10), value) = N'1'
    )
    BEGIN
        EXEC sys.sp_addextendedproperty
            @name = N'${MARKER_NAME}',
            @value = N'1';
    END
  "

echo "ShopDB initialization is complete."
