# ADR-0005 — Monólito modular + Clean Architecture

- **Status:** Aceito
- **Data:** 2026-06-05
- **Decisores:** CTO, Tech Lead

## Contexto
Time pequeno, produto novo, domínio ainda instável. Queremos fronteiras claras sem o custo
operacional de sistemas distribuídos.

## Opções consideradas
1. **Monólito modular + Clean Architecture** — camadas (`Domain`/`Application`/`Infrastructure`/`Web`)
   com módulos verticais por feature. Fronteiras claras, deploy único.
2. **Microserviços** — escala/isolamento independentes, mas custo alto de deploy, rede e
   observabilidade distribuída sem benefício nesta fase.
3. **Monólito "transaction script" sem camadas** — rápido de começar, vira espaguete e dívida.

## Decisão
**Monólito modular com Clean Architecture.** Dependências apontam para dentro; `Domain` não depende
de framework. Módulos verticais (Vehicles, Catalog, Maintenance, …) coesos por feature.

## Consequências
- ✅ Fronteiras claras (90% do benefício de microserviços) a baixo custo.
- ✅ Um módulo pode ser extraído no futuro porque já tem fronteira definida.
- ⚠️ Disciplina necessária para não vazar dependências entre camadas → futuro
  `Architecture.Tests` para impor via build.
- ➡️ Sem microserviços até que um gargalo real justifique.
