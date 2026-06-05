# ADR-0012 — Implantação local-first (Raspberry Pi / ARM64), portável para nuvem

- **Status:** Aceito (revisado em 2026-06-05 após decisão do stakeholder)
- **Data:** 2026-06-05
- **Decisores:** CTO, Tech Lead, PO

## Contexto
Decisão do stakeholder: o ÄLGarage, **nesta fase, é uma ferramenta interna da equipe**, hospedada
em **servidor local** (Raspberry Pi ou similar) rodando site + banco. Sem nuvem agora. No futuro,
possivelmente Azure ou AWS — mantendo tudo **portável, sem lock-in proprietário**.

Isso muda o perfil: poucos usuários, rede confiável (LAN), custo ~zero, mas operação é nossa
(energia, cartão SD frágil, backup manual).

## Decisão
- **Empacotar em Docker, imagem `linux/arm64`** para o Pi (também roda em amd64). Ver [`Dockerfile`](../../Dockerfile).
- **PostgreSQL** mantido (paridade com a nuvem futura), em container ARM64, via
  [`docker-compose.yml`](../../docker-compose.yml).
- **Rodar de SSD/USB, não do cartão SD** (durabilidade/IO). Dados do Postgres num volume apontado
  para o SSD.
- **Backup do Postgres** agendado (`pg_dump` comprimido + rotação) — ver [`scripts/`](../../scripts).
- **Sem HTTPS forçado** no app: roda por HTTP na LAN; TLS, se desejado, via reverse proxy à frente.
- **Cloud-portável**: como é container + Postgres, migrar para Azure (Container Apps) ou AWS (ECS/
  App Runner) no futuro é re-deploy, **não reescrita**. Nada de serviço proprietário no caminho.

Detalhe operacional em [`08-implantacao-local.md`](../08-implantacao-local.md).

## Consequências
- ✅ Custo ~zero, controle total, dados ficam fisicamente com a equipe (bom p/ LGPD/privacidade).
- ✅ Render mode `InteractiveServer` fica ainda mais adequado (latência de LAN é mínima).
- ✅ Portabilidade futura preservada (sem lock-in).
- ⚠️ Disponibilidade/durabilidade são responsabilidade nossa: **SSD obrigatório**, **backup
  testado**, e energia/uptime do Pi são pontos únicos de falha (aceitável p/ ferramenta interna).
- ⚠️ Build ARM64: compilar no próprio Pi ou via `docker buildx` cross-platform.
