#!/usr/bin/env bash
# Backup do PostgreSQL do ÄLGarage (rodando no Pi via docker compose).
# Gera um dump comprimido com timestamp em ./backups e mantém os N mais recentes.
# Agende no cron, ex.: 0 3 * * *  /caminho/scripts/backup-postgres.sh
set -euo pipefail

cd "$(dirname "$0")/.."
[ -f .env ] && set -a && . ./.env && set +a

DB="${POSTGRES_DB:-algarage}"
USER="${POSTGRES_USER:-algarage}"
KEEP="${BACKUP_KEEP:-14}"          # quantos backups manter
OUT_DIR="./backups"
STAMP="$(date +%Y%m%d-%H%M%S)"
OUT="${OUT_DIR}/algarage-${STAMP}.sql.gz"

mkdir -p "$OUT_DIR"
echo "==> Dump de '${DB}' para ${OUT}"
docker compose exec -T db pg_dump -U "$USER" -d "$DB" | gzip > "$OUT"

# Rotação: mantém só os $KEEP mais recentes.
ls -1t "${OUT_DIR}"/algarage-*.sql.gz 2>/dev/null | tail -n "+$((KEEP+1))" | xargs -r rm -f

echo "==> OK. Backups atuais:"
ls -lh "${OUT_DIR}"/algarage-*.sql.gz
