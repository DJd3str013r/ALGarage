#!/usr/bin/env bash
# Restaura um backup do PostgreSQL do ÄLGarage.
# Uso: scripts/restore-postgres.sh ./backups/algarage-AAAAMMDD-HHMMSS.sql.gz
set -euo pipefail

cd "$(dirname "$0")/.."
[ -f .env ] && set -a && . ./.env && set +a

FILE="${1:?Informe o arquivo de backup .sql.gz}"
DB="${POSTGRES_DB:-algarage}"
USER="${POSTGRES_USER:-algarage}"

echo "==> ATENÇÃO: isto sobrescreve dados em '${DB}'. Ctrl-C para abortar."
sleep 3
gunzip -c "$FILE" | docker compose exec -T db psql -U "$USER" -d "$DB"
echo "==> Restore concluído."
