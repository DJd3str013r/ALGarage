# Implantação Local — Raspberry Pi (ARM64)

Ferramenta interna da equipe rodando em servidor local (Pi ou similar): site + PostgreSQL em Docker.
Sem nuvem agora; portável para Azure/AWS depois. Ver [ADR-0012](adr/0012-hosting-deployment.md).

## Requisitos do Pi

- Raspberry Pi 4/5 (ou equivalente ARM64) com **64-bit OS** (Raspberry Pi OS Lite 64-bit / Ubuntu Server).
- **SSD/USB** como armazenamento principal — **não** rodar Postgres do cartão SD (desgaste + IO).
- Docker + Docker Compose instalados.

## Passo a passo

```bash
# 1) Clonar e configurar segredos
git clone <repo> && cd ALGarage
cp .env.example .env
#   edite .env e defina POSTGRES_PASSWORD

# 2) (recomendado) apontar os dados do Docker para o SSD
#   edite /etc/docker/daemon.json com {"data-root": "/mnt/ssd/docker"} e reinicie o Docker,
#   OU troque o volume 'algarage_pgdata' por um bind-mount em /mnt/ssd no docker-compose.yml

# 3) Subir
docker compose up -d --build      # no Pi, build nativo arm64

# 4) Acessar na LAN
#   http://<ip-do-pi>:8080
```

### Build cross-platform (de um PC, sem buildar no Pi)

```bash
docker buildx build --platform linux/arm64 -t algarage-web:local --load .
```

## Migration inicial do banco (uma vez)

O app aplica migrations na inicialização, mas a **migration inicial precisa ser gerada** com o SDK
(este passo não roda no Pi sem o SDK .NET; faça num dev e versione a migration):

```bash
dotnet tool install --global dotnet-ef        # se ainda não tiver
dotnet ef migrations add InitialCreate \
  -p src/ALGarage.Infrastructure -s src/ALGarage.Web
```

Ao subir, o `AppDbInitializer` roda `MigrateAsync` e **semeia o catálogo do V40** automaticamente
(idempotente). Se não houver migration, o app sobe mas registra um aviso e a garagem fica sem
catálogo até a migration existir.

## Backup e restore do PostgreSQL

```bash
# Backup manual (gera ./backups/algarage-AAAAMMDD-HHMMSS.sql.gz, mantém os 14 mais recentes)
./scripts/backup-postgres.sh

# Agendar diário às 03:00 (crontab -e)
0 3 * * * cd /caminho/ALGarage && ./scripts/backup-postgres.sh >> ./backups/backup.log 2>&1

# Restore
./scripts/restore-postgres.sh ./backups/algarage-20260605-030000.sql.gz
```

> **Teste o restore.** Backup que nunca foi restaurado não é backup. Faça um restore de validação
> num banco descartável periodicamente. Considere copiar `./backups` para outro dispositivo/NAS
> (o Pi é ponto único de falha).

## TLS (opcional)

O app serve HTTP na LAN. Se quiser HTTPS, ponha um reverse proxy (Caddy/nginx/Traefik) à frente,
terminando TLS — o app não precisa mudar.

## Migração futura para nuvem (sem lock-in)

Como tudo é **container + PostgreSQL padrão**:
- **Azure**: Container Apps + Azure Database for PostgreSQL Flexible Server.
- **AWS**: ECS/App Runner + RDS for PostgreSQL.
- Restaurar o último `pg_dump` no Postgres gerenciado e apontar a `ConnectionStrings__Default`.
Nenhuma reescrita de código.
