# Architecture Decision Records (ADRs)

Cada arquivo registra **uma** decisão: contexto, opções, decisão e consequências. Formato leve
inspirado em Michael Nygard. Status possíveis: `Proposto` · `Aceito` · `Substituído` · `Obsoleto`.

| ADR | Decisão | Status |
|---|---|---|
| [0001](0001-dotnet-10-lts.md) | .NET 10 (LTS) + C# 14 | Aceito |
| [0002](0002-blazor-web-app-render-mode.md) | Blazor Web App, render mode `InteractiveServer` no MVP | Aceito |
| [0003](0003-postgresql-efcore.md) | PostgreSQL + EF Core 10 | Aceito |
| [0004](0004-aspnet-identity-auth.md) | ASP.NET Core Identity (OIDC-ready) | Aceito |
| [0005](0005-modular-monolith-clean-architecture.md) | Monólito modular + Clean Architecture | Aceito |
| [0006](0006-multi-brand-provider-pattern.md) | Multi-marca por padrão de provider/estratégia | Aceito |
| [0007](0007-parts-finder-affiliate-links-no-scraping.md) | Buscador de peças via afiliados/deep-links, sem scraping | Aceito |
| [0008](0008-vin-decoding-strategy.md) | Estratégia de decode de VIN em camadas + dataset curado | Aceito |
| [0009](0009-maintenance-estimation-engine.md) | Motor de estimativa de manutenção (km e/ou tempo) | Aceito |
| [0010](0010-3d-visualization-stretch.md) | Visualização 3D como stretch (2D primeiro) | Aceito |
| [0011](0011-lgpd-data-protection.md) | LGPD como fundação | Aceito |
| [0012](0012-hosting-deployment.md) | Implantação local-first (Raspberry Pi/ARM64), portável p/ nuvem | Aceito (revisado) |
| [0013](0013-i18n-en-pt.md) | Internacionalização (Inglês + Português) desde o MVP | Aceito |
| [0014](0014-dark-mode.md) | Dark mode obrigatório (e padrão) via design tokens | Aceito |

## Template

```markdown
# ADR-NNNN — Título

- **Status:** Proposto | Aceito | Substituído por ADR-XXXX | Obsoleto
- **Data:** AAAA-MM-DD
- **Decisores:** PO, PM, CTO, Tech Lead, …

## Contexto
O que motiva a decisão? Forças em jogo, restrições.

## Opções consideradas
1. Opção A — prós/contras
2. Opção B — prós/contras

## Decisão
O que foi decidido e por quê.

## Consequências
Positivas, negativas e o que isso obriga/permite no futuro.
```
