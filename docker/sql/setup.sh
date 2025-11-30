#!/bin/bash
set -e

/opt/mssql/bin/sqlservr &

SERVER="localhost"
USER="sa"
INIT_SCRIPT="/usr/config/init.sql"

echo "Ожидание запуска SQL Server..."

until /opt/mssql-tools/bin/sqlcmd -S $SERVER -U $USER -P "$MSSQL_SA_PASSWORD" -Q "SELECT 1" &> /dev/null; do
  echo "SQL Server недоступен. Ожидание..."
  sleep 5
done

echo "SQL Server доступен. Запуск init.sql..."

/opt/mssql-tools/bin/sqlcmd -S $SERVER -U $USER -P "$MSSQL_SA_PASSWORD" -i $INIT_SCRIPT

echo "Инициализация init.sql завершена."

wait