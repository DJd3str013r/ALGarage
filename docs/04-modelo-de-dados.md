# Esboço do Modelo de Dados

Esboço, não esquema final. Foco nas **entidades principais e relações**. Detalhes de tipos/índices
serão definidos nas migrations. As entidades estão refletidas como classes-esqueleto em
`src/ALGarage.Domain/`.

## Diagrama de relações (alto nível)

```
                         ┌──────────────┐
                         │    Brand     │   (Volvo hoje; multi-marca por design)
                         └──────┬───────┘
                                │ 1..*
                         ┌──────▼───────┐
                         │ VehicleModel │  (ex.: XC60)
                         └──────┬───────┘
                                │ 1..*
                         ┌──────▼────────┐        ┌──────────────┐
                         │ ModelVariant  │───────►│  EngineSpec  │
                         │ (versão/ano)  │ 1..*   │ (motorização)│
                         └──┬─────────┬──┘        └──────────────┘
              1..* │        │ 1..*    │ 1..*
        ┌──────────▼──┐ ┌───▼──────────┐ ┌────────▼─────────┐
        │ FactoryOption│ │MaintenancePlan│ │     Upgrade     │
        │ (opcionais)  │ │  (cronograma) │ │ +Stage (perf/   │
        └──────────────┘ └──────┬───────┘ │   estética)     │
                                 │ 1..*    └──────────────────┘
                          ┌──────▼────────┐
                          │ MaintenanceItem│ (intervalo km/tempo, vida útil de peça)
                          └──────┬─────────┘
                                 │ 0..* (referência opcional)
                          ┌──────▼────────┐
                          │ PartReference │ (categoria, part number OEM quando houver)
                          └───────────────┘

   ┌──────────┐ 1..*   ┌──────────┐ 1..1   ┌──────────────┐
   │   User   │───────►│ Vehicle  │───────►│ ModelVariant │ (resolvido via VIN)
   │ (Identity)│       │ (garagem)│        └──────────────┘
   └────┬─────┘        └────┬─────┘
        │ 1..1              │ 1..*
   ┌────▼──────┐      ┌─────▼────────┐
   │Subscription│     │ ServiceRecord│ (histórico do que foi feito)
   │ +Plan      │     │  +ServiceItem│
   │ +Entitlement│    └──────────────┘
   └────────────┘
        (previsto, sem billing no MVP)
```

## Entidades principais

### Catálogo (dados de fábrica / curados)

| Entidade | Campos-chave | Notas |
|---|---|---|
| **Brand** | `Id`, `Name`, `Slug` | Volvo é o primeiro registro. Cidadã de 1ª classe p/ multi-marca. |
| **VehicleModel** | `Id`, `BrandId`, `Name`, `Generation` | Ex.: XC60, gerações I/II. |
| **ModelVariant** | `Id`, `VehicleModelId`, `Trim`, `ModelYear`, `Market`, `SpecsJson (JSONB)` | A "versão" resolvida do VIN. `SpecsJson` guarda specs heterogêneas. |
| **EngineSpec** | `Id`, `ModelVariantId`, `Code`, `FuelType`, `DisplacementCc`, `PowerHp`, `Aspiration` | Motorização. |
| **FactoryOption** | `Id`, `ModelVariantId`, `Code`, `Category`, `Name` | Opcionais de fábrica. Pode também viver em `SpecsJson`. |

### Garagem do usuário

| Entidade | Campos-chave | Notas |
|---|---|---|
| **User** | `Id`, `Email`, … | Gerenciado por ASP.NET Core Identity. |
| **Vehicle** | `Id`, `UserId`, `Vin`, `ModelVariantId?`, `Nickname`, `PurchaseDateUtc`, `PreviousOwners`, `OdometerAtAcquisitionKm`, `CurrentOdometerKm`, `AvgDailyKm`, `CreatedAtUtc` | **VIN obrigatório**. `ModelVariantId` é resolvido pela decodificação (pode ficar nulo se o decode falhar → estado "não identificado"). |
| **OdometerReading** | `Id`, `VehicleId`, `Km`, `RecordedAtUtc`, `Source` | Histórico de hodômetro → melhora a estimativa de km/dia ao longo do tempo. |

> **Value Objects** no domínio: `Vin` (valida formato/checksum, 17 chars), `Odometer`,
> `Acquisition` (data + km + nº donos). Mantêm invariantes fora da entidade.

### Manutenção e estimativa

| Entidade | Campos-chave | Notas |
|---|---|---|
| **MaintenancePlan** | `Id`, `ModelVariantId` (ou `EngineSpecId`), `Name`, `SourceRef` | Cronograma por modelo/motor. |
| **MaintenanceItem** | `Id`, `MaintenancePlanId`, `Name`, `IntervalKm?`, `IntervalTime?` (ISO 8601 duration), `PartReferenceId?`, `WhicheverComesFirst (bool)` | Define **km e/ou tempo**. Óleo: tipicamente `IntervalKm` + `IntervalTime` com "o que vier primeiro". |
| **MaintenanceStatus** *(calculado, não persistido obrigatoriamente)* | `VehicleId`, `MaintenanceItemId`, `DueByDateUtc`, `DueAtKm`, `EstimatedDueDateUtc`, `State` (`Ok`/`DueSoon`/`Overdue`) | Saída do **motor de estimativa** (ver [`adr/0009`](adr/0009-maintenance-estimation-engine.md)). `DueSoon` é o que pinta de vermelho no 3D. |

### Histórico de serviços

| Entidade | Campos-chave | Notas |
|---|---|---|
| **ServiceRecord** | `Id`, `VehicleId`, `PerformedAtUtc`, `OdometerKm`, `Workshop`, `TotalCost?`, `Notes` | Um evento de serviço. |
| **ServiceItem** | `Id`, `ServiceRecordId`, `MaintenanceItemId?`, `Description`, `PartReferenceId?`, `Cost?` | Itens daquele serviço. Liga ao `MaintenanceItem` para **resetar** o relógio da estimativa. |

### Peças e upgrades

| Entidade | Campos-chave | Notas |
|---|---|---|
| **PartReference** | `Id`, `Category`, `OemPartNumber?`, `Name`, `CompatibilityJson?` | "Código de peça" mora aqui **quando existir fonte**. O buscador de links **não** depende disto. |
| **PartCategory** | enum/tabela | Óleo, filtros, freios, correia/corrente, etc. |
| **Upgrade** | `Id`, `ModelVariantId`/`EngineSpecId`, `Type` (`Performance`/`Aesthetic`), `Name`, `Description` | Tela de upgrades. |
| **Stage** | `Id`, `UpgradeId`, `Level` (Stage 1/2/3), `GainsJson`, `Requirements` | Stages de performance. |

### Billing (PREVISTO — sem implementação)

| Entidade | Campos-chave | Notas |
|---|---|---|
| **Plan** | `Id`, `Code` (`Free`/`Pro`…), `Name` | Todos "Free" no MVP. |
| **Subscription** | `Id`, `UserId`, `PlanId`, `Status`, `PeriodEndUtc?` | Sem gateway de pagamento agora. |
| **Entitlement** | `Id`, `PlanId`, `FeatureKey`, `Limit?` | Gates de feature leem isto, não `if (isPremium)`. |

### LGPD / auditoria (fundação)

| Entidade | Campos-chave | Notas |
|---|---|---|
| **ConsentLog** | `Id`, `UserId`, `Purpose`, `GrantedAtUtc`, `Version` | Registro de consentimento. |
| **DataExportRequest** / **DeletionRequest** | `Id`, `UserId`, `Status`, `RequestedAtUtc` | Direitos do titular. |

## Convenções

- **PKs:** `Guid` (v7, sequencial — bom p/ índice) ou `long` identity. Decisão na migration; o
  esqueleto usa `Guid`.
- **Auditoria:** `CreatedAtUtc`/`UpdatedAtUtc` em entidades mutáveis; **soft delete** (`IsDeleted`)
  onde LGPD exige rastro antes da exclusão definitiva.
- **JSONB** para specs/opcionais/compatibilidade heterogêneos (Postgres). O relacional fica para o
  que é consultado/filtrado com frequência.
- **VIN** indexado e tratado como dado pessoal (ver LGPD).

## Pontos em aberto do modelo (decisões adiadas de propósito)

1. **Granularidade do `MaintenancePlan`**: por `ModelVariant` ou por `EngineSpec`? Provavelmente por
   motor, com herança do modelo. Definir ao curar os primeiros dados Volvo.
2. **Compatibilidade de peças**: relacional (`PartCompatibility` N:N) vs `CompatibilityJson`. Começa
   JSON; promove a tabela se virar consulta crítica.
3. **Multi-veículo idêntico**: dois usuários com o "mesmo" modelo compartilham o catálogo, mas cada
   `Vehicle` tem seu próprio estado de manutenção. Já contemplado.
