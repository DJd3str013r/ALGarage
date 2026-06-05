# Estrutura da Solution

**Estilo:** Monólito modular com **Clean Architecture** (dependências apontam para dentro:
`Web → Infrastructure → Application → Domain`; `Domain` não depende de ninguém).
([ADR-0005](adr/0005-modular-monolith-clean-architecture.md))

## Visão de projetos

```
ALGarage.sln
│
├─ src/
│  ├─ ALGarage.Domain
│  │     Entidades, value objects, enums e REGRAS de domínio puras.
│  │     Zero dependências de framework (sem EF, sem ASP.NET). É o coração estável.
│  │
│  ├─ ALGarage.Application
│  │     Casos de uso (application services), DTOs e as PORTAS (interfaces) para
│  │     o mundo externo: IVinDecoder, IPartsSearchLinkProvider, IMaintenanceEstimator,
│  │     IUnitOfWork/repos. Depende só de Domain.
│  │
│  ├─ ALGarage.Infrastructure
│  │     Implementações das portas: EF Core (DbContext, migrations, repos),
│  │     providers externos (vPIC, afiliados), e-mail, storage. Depende de Application.
│  │
│  ├─ ALGarage.Shared
│  │     Contratos (DTOs) compartilhados servidor↔cliente. Existe agora para já
│  │     desacoplar a UI; vira ponte para o futuro projeto .Client (render Auto).
│  │
│  └─ ALGarage.Web
│        Blazor Web App: host, Program.cs, componentes/páginas, autenticação.
│        Compõe tudo via DI. É o único projeto "executável".
│
└─ tests/
   └─ ALGarage.Domain.Tests
         Exemplo de teste de unidade do domínio (motor de estimativa de manutenção).
         Futuro: Application.Tests, Architecture.Tests (enforcement de camadas).
```

## Por que cada camada existe

| Projeto | Responsabilidade | Pode depender de | NÃO pode depender de |
|---|---|---|---|
| `Domain` | Regras de negócio, invariantes, cálculos puros | — | EF, ASP.NET, qualquer infra |
| `Application` | Orquestração de casos de uso, define _portas_ | `Domain` | `Infrastructure`, `Web` |
| `Infrastructure` | Implementa portas (DB, HTTP, e-mail) | `Application`, `Domain` | `Web` |
| `Shared` | DTOs de contrato UI↔backend | — (ou `Domain` mínimo) | `Infrastructure` |
| `Web` | UI Blazor + composição (DI, middleware) | todos os `src` | — |
| `*.Tests` | Verificação | o alvo testado | — |

## Organização interna por **módulo** (vertical) dentro de cada camada

Dentro de `Domain`/`Application`/`Infrastructure`, agrupamos por **módulo de negócio**, não por tipo
técnico. Isso facilita extrair um módulo no futuro, se necessário:

```
ALGarage.Domain/
  Common/                → Entity, AggregateRoot, ValueObject, Result, DomainEvent
  Vehicles/              → Vehicle, Vin (VO), Odometer (VO), Acquisition (VO)
  Catalog/               → Brand, VehicleModel, ModelVariant, FactoryOption, EngineSpec
  Maintenance/           → MaintenancePlan, MaintenanceItem, MaintenanceStatus (calc)
  Service/               → ServiceRecord, ServiceItem
  Parts/                 → PartReference, PartCategory
  Upgrades/              → Upgrade, Stage
  Identity/              → (referências mínimas; o usuário concreto vive na Infra/Identity)
  Billing/               → Plan, Subscription, Entitlement   (previsto, sem lógica ainda)
```

> Os mesmos nomes de módulo se repetem em `Application` (casos de uso) e `Infrastructure`
> (persistência/providers). Coesão alta por feature, acoplamento baixo entre features.

## Regras de dependência — como as garantimos

- **Project references** já impedem ciclos óbvios (Domain não referencia ninguém).
- **Futuro:** um projeto `ALGarage.Architecture.Tests` com NetArchTest/ArchUnitNET para falhar o
  build se alguém referenciar EF dentro do `Domain`, etc. Não no esqueleto inicial, mas previsto.

## Por que **não** microserviços (ainda)

- Time pequeno, produto novo, domínio ainda instável. Microserviços agora = custo operacional alto
  (deploy, rede, observabilidade distribuída) sem benefício.
- A **modularização interna** dá 90% do benefício (limites claros) com 10% do custo. Se um módulo
  (ex.: ingestão de catálogo) precisar escalar isolado no futuro, ele já está com fronteira definida
  para ser extraído.

## Por que `.slnx` vs `.sln`

Fornecemos um `.sln` clássico por **máxima compatibilidade** de tooling. O formato `.slnx` (XML, mais
limpo) é suportado no .NET 10 e pode ser adotado depois — é troca trivial.
