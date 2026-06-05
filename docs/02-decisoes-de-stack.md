# Escolhas de Stack (Justificadas)

Resumo das tecnologias e o porquê de cada uma. Decisões individuais têm ADR próprio em
[`adr/`](adr/).

## Runtime e linguagem — .NET 10 (LTS) + C# 14

- **.NET 10** é a versão **LTS** atual (lançada nov/2025, suporte de 3 anos). Para um produto que vai
  durar, LTS > STS. ([ADR-0001](adr/0001-dotnet-10-lts.md))
- **C# 14** acompanha o SDK; sem motivo para fixar versão antiga.
- **Por que não .NET 8?** Ainda suportado, mas estaríamos começando um produto novo numa versão
  prestes a sair de suporte mainstream. Sem ganho.

## UI — Blazor Web App, render mode `InteractiveServer`

- **Blazor Web App** (o modelo unificado pós-.NET 8) com **render mode global `InteractiveServer`**
  no MVP. Menos partes móveis, sem API HTTP separada, auth simples, time-to-market.
- Estrutura preparada para adicionar projeto `.Client` e migrar para **`InteractiveAuto`** quando
  houver necessidade (offline, escala de conexões SignalR). ([ADR-0002](adr/0002-blazor-web-app-render-mode.md))
- **O 3D não influencia essa escolha**: roda em JS (Three/Babylon) via JS interop, independente do
  render mode.

## Banco de dados — PostgreSQL 16+

- **JSONB** para specs/opcionais de fábrica heterogêneos (variam muito por versão/mercado).
- **Custo de licença zero** e **portabilidade multicloud** (não amarra a um fornecedor).
- Ótimo suporte no EF Core via **Npgsql**. ([ADR-0003](adr/0003-postgresql-efcore.md))

## ORM — EF Core 10 (+ Dapper pontual no futuro)

- Migrations, LINQ, produtividade. Para o domínio transacional (garagem, manutenção, serviços) é
  ideal.
- **Dapper** fica reservado para _read models_ pesados de catálogo/relatório **se e quando** o perfil
  de performance pedir. Não no MVP.

## Autenticação — ASP.NET Core Identity (OIDC-ready)

- **Donos do dado** do usuário (importante para LGPD e para não pagar por MAU em IdP externo cedo).
- Atrás de uma abstração que permite plugar **OIDC/IdP externo** (Entra ID, Auth0, Keycloak, login
  social) depois sem reescrever o domínio. ([ADR-0004](adr/0004-aspnet-identity-auth.md))
- **Trade-off:** Identity exige cuidar de reset de senha, e-mail, MFA nós mesmos. Aceitável no MVP;
  reavaliar se a base crescer.

## Hospedagem — Local-first (Raspberry Pi / ARM64), portável para nuvem

- **Ferramenta interna da equipe**, hospedada **localmente** num **Raspberry Pi** (ou similar):
  **Docker, imagem `linux/arm64`**, site + **PostgreSQL** no mesmo host via `docker-compose`.
- **SSD/USB** para o banco (não cartão SD) + **backup** de Postgres agendado (`pg_dump`).
- **Sem nuvem agora.** Como é container + Postgres padrão, migrar para **Azure** (Container Apps) ou
  **AWS** (ECS/App Runner + RDS) no futuro é re-deploy, **sem lock-in**. ([ADR-0012](adr/0012-hosting-deployment.md))
- **Dev:** opcionalmente **.NET Aspire** para orquestrar app + Postgres localmente.

## Internacionalização — Inglês + Português (desde o MVP)

- **`Microsoft.Extensions.Localization`** (no shared framework, sem pacote extra) com
  **`IStringLocalizer`** + **`.resx`** (`SharedResource.resx` em inglês neutro,
  `SharedResource.pt-BR.resx`). Padrão `pt-BR`. ([ADR-0013](adr/0013-i18n-en-pt.md))
- **`InvariantGlobalization` desligado** (precisa de ICU; imagem de container Debian inclui).

## UI / Tema — Dark mode obrigatório

- **Dark é o padrão**, via **CSS design tokens** (custom properties), `data-theme` + `localStorage`,
  com script anti-flash. Light é opcional. ([ADR-0014](adr/0014-dark-mode.md))
- Sem framework de UI pesado no MVP; CSS próprio enxuto. Um design system (ex.: MudBlazor) pode
  entrar depois **se** a complexidade das telas justificar — fica como decisão futura.

## Bibliotecas principais (e as que evitamos de propósito)

| Necessidade | Escolha | Observação |
|---|---|---|
| Validação | **FluentValidation** | Continua OSS/livre. |
| Mapeamento objeto-objeto | **Mapperly** (source generator) | Livre. **Evitamos AutoMapper** (virou pago em 2025). |
| Mediação/CQRS | **Sem MediatR** | MediatR virou comercial em 2025. Usamos _application services_ simples ou um dispatcher interno minúsculo. |
| Asserções de teste | **Shouldly** | **Evitamos FluentAssertions v8** (virou pago). Alternativa livre: AwesomeAssertions. |
| Testes | **xUnit** | Padrão de mercado. |
| Logging | **Serilog** | Structured logging. |
| Observabilidade | **OpenTelemetry** | Traces/metrics; export para o backend da nuvem escolhida. |
| HTTP resiliente | **`Microsoft.Extensions.Http.Resilience`** (Polly) | Para chamar APIs externas (VIN, peças) com retry/circuit breaker. |

> **Por que esses "evitamos" importam:** AutoMapper, MediatR e FluentAssertions — três pilares
> históricos de projetos .NET — mudaram para licenças comerciais em 2024–2025. Escolher alternativas
> livres agora evita uma dor de licenciamento (ou reescrita) no meio do caminho. É uma decisão
> consciente, não esquecimento.

## Resumo em uma frase

**.NET 10 + Blazor Web App (Server, dark-first, en/pt) + PostgreSQL + EF Core, em containers ARM64
rodando localmente num Raspberry Pi (portável para nuvem), com bibliotecas comprovadamente livres**
— uma stack conservadora de propósito, otimizada para um time pequeno entregar rápido sem armadilhas
de licença, lock-in ou reescrita.
