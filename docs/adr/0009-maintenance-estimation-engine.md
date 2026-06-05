# ADR-0009 — Motor de estimativa de manutenção (km e/ou tempo)

- **Status:** Aceito
- **Data:** 2026-06-05
- **Decisores:** CTO, Especialista Automotivo, Tech Lead

## Contexto
Núcleo da proposta de valor: avisar **quando** cada item vence. O brief pede troca por **tempo** ou
por **estimativa de km/dia**. Cada item de manutenção pode ter intervalo por km, por tempo, ou ambos
("o que vier primeiro").

## Modelo
- `MaintenanceItem` define `IntervalKm?`, `IntervalTime?` e `WhicheverComesFirst`.
- **km/dia** estimado: `(CurrentOdometerKm - OdometerAtAcquisitionKm) / dias_desde_aquisição`,
  refinado por `OdometerReading`s ao longo do tempo. O usuário também pode informar uma média.
- Para cada item, a partir do **último serviço** (`ServiceRecord`/`ServiceItem`) que o zerou:
  - **due por km:** `lastServiceKm + IntervalKm` → projeta data via km/dia.
  - **due por tempo:** `lastServiceDate + IntervalTime`.
  - **resultado:** `min(due_km_projetado, due_tempo)` quando `WhicheverComesFirst`.
- **Estado:** `Overdue` (passou) · `DueSoon` (dentro de um limiar configurável) · `Ok`.
  `DueSoon` é o gatilho do **vermelho** no 3D/2D.

## Opções consideradas
1. **Lógica em dados** (intervalos no banco) + cálculo puro no `Domain`. Testável, flexível.
2. **Regras hardcoded por modelo** — rígido, não escala, vira `if` infinito. Rejeitado.

## Decisão
**Opção 1.** Cronogramas são **dados** (`MaintenancePlan`/`MaintenanceItem`); o cálculo é uma função
**pura** no `Domain` (`IMaintenanceEstimator`), coberta por testes de unidade
(`ALGarage.Domain.Tests`).

## Consequências
- ✅ Determinístico e testável; novos modelos = inserir linhas, não código.
- ✅ Conservador (o que vier primeiro) reduz risco de subestimar.
- ⚠️ km/dia é **média** → impreciso para uso irregular. Mitigado por histórico de hodômetro e, no
  futuro, telemetria/OBD-II.
