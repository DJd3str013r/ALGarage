# ADR-0012 — Hospedagem em containers, região Brasil

- **Status:** Aceito
- **Data:** 2026-06-05
- **Decisores:** CTO, Tech Lead

## Contexto
O público-alvo é brasileiro e o render mode `InteractiveServer` é sensível a latência (SignalR). A
LGPD favorece residência de dados no Brasil. Não queremos travar numa única nuvem.

## Opções consideradas
1. **Containers (Docker) em região BR**, cloud-agnóstico — **Azure Container Apps (`Brazil South`)**
   recomendado, **AWS (`sa-east-1`)** equivalente; **PostgreSQL gerenciado**.
2. **PaaS amarrado** (ex.: App Service .NET sem container) — simples, mas menos portável.
3. **VM/VPS própria** — barato, mas mais operação (patch, escala, backup) no nosso colo.

## Decisão
**Opção 1.** App empacotado em **container**; deploy em região do Brasil; banco **PostgreSQL
gerenciado** (Flexible Server / RDS). Nuvem concreta é Q4 em [`06`](../06-riscos-e-questoes-abertas.md)
(default: Azure Container Apps, Brazil South). **Dev** opcionalmente com **.NET Aspire**.

## Consequências
- ✅ Baixa latência p/ o público BR (importante no Server) e residência de dados (LGPD).
- ✅ Cloud-agnóstico: trocar de nuvem é re-deploy do container, não reescrita.
- ⚠️ Operação de container/observabilidade exige setup de CI/CD (Fase 1).
- ➡️ Escala horizontal disponível quando marketing trouxer picos (ver R7).
