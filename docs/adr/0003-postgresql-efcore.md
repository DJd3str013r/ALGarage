# ADR-0003 — PostgreSQL + EF Core 10

- **Status:** Aceito
- **Data:** 2026-06-05
- **Decisores:** CTO, Tech Lead, Especialista Automotivo

## Contexto
Os dados de catálogo (specs, opcionais de fábrica, compatibilidade) são **heterogêneos** e variam
muito por versão/mercado. O domínio transacional (garagem, manutenção, serviços) é relacional.
Queremos custo de licença baixo e portabilidade entre nuvens.

## Opções consideradas
1. **PostgreSQL** — `JSONB` indexável para specs heterogêneos, custo zero de licença, multicloud,
   ótimo suporte no EF Core via Npgsql.
2. **SQL Server / Azure SQL** — melhor integração histórica com .NET e tooling, mas licença/custo e
   tendência a amarrar ao ecossistema Microsoft.
3. **NoSQL puro (ex.: Mongo)** — flexível para specs, mas perdemos transações relacionais que o
   domínio de garagem/serviços exige.

## Decisão
**PostgreSQL 16+** com **EF Core 10** (Npgsql). Entidades de negócio em tabelas relacionais; specs e
opcionais heterogêneos em **`JSONB`**.

## Consequências
- ✅ Modelagem natural do híbrido relacional + semiestruturado.
- ✅ Sem custo de licença; fácil rodar local (Docker) e em qualquer nuvem.
- ⚠️ Time precisa de familiaridade com particularidades do Postgres/Npgsql (tipos, migrations).
- ➡️ **Dapper** fica reservado para read models pesados no futuro, se o perfil de performance pedir.
