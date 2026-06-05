# ÄLGarage

> Aplicação web para monitorar a "saúde" de carros **Volvo** (com expansão futura para outras marcas):
> garagem do usuário, decodificação de VIN, monitoramento de manutenção, busca de peças (por links),
> versões de fábrica e upgrades.

**Status:** 🏗️ Fase de _estruturação_. Ainda **não** há implementação de features — apenas arquitetura,
decisões técnicas, esqueleto da solution, esboço do modelo de dados e roadmap.

---

## 📂 Onde está o quê

A documentação de decisão é o entregável principal desta fase. Comece por aqui:

| Documento | Conteúdo |
|---|---|
| [`docs/00-resumo-executivo.md`](docs/00-resumo-executivo.md) | Resumo executivo da decisão (leia primeiro). |
| [`docs/01-discussao-do-time.md`](docs/01-discussao-do-time.md) | A discussão multidisciplinar: divergências e como foram resolvidas. |
| [`docs/02-decisoes-de-stack.md`](docs/02-decisoes-de-stack.md) | Stack justificada (DB, ORM, auth, hospedagem, libs). |
| [`docs/03-estrutura-da-solution.md`](docs/03-estrutura-da-solution.md) | Camadas, projetos e responsabilidades. |
| [`docs/04-modelo-de-dados.md`](docs/04-modelo-de-dados.md) | Entidades principais e relações. |
| [`docs/05-roadmap.md`](docs/05-roadmap.md) | Roadmap em fases: MVP → stretch → futuro. |
| [`docs/06-riscos-e-questoes-abertas.md`](docs/06-riscos-e-questoes-abertas.md) | Riscos e **perguntas que precisam de decisão sua**. |
| [`docs/07-fontes-de-dados.md`](docs/07-fontes-de-dados.md) | Fontes de VIN, peças e especificações — com plano B. |
| [`docs/08-implantacao-local.md`](docs/08-implantacao-local.md) | Rodar no Raspberry Pi (ARM64): Docker, Postgres, backup. |
| [`docs/09-dataset-v40.md`](docs/09-dataset-v40.md) | Dataset curado do Volvo V40 (2012–2019): escopo, mapeamento e disclaimer. |
| [`docs/adr/`](docs/adr/) | Architecture Decision Records (1 decisão por arquivo). |

## 🧱 Esqueleto da solution

```
ALGarage.sln
├─ src/
│  ├─ ALGarage.Domain          → Entidades, value objects, regras de domínio (sem dependências)
│  ├─ ALGarage.Application     → Casos de uso, DTOs e PORTAS (interfaces) p/ o mundo externo
│  ├─ ALGarage.Infrastructure  → EF Core, providers externos (VIN, peças), implementações
│  ├─ ALGarage.Shared          → Contratos compartilhados servidor/cliente (futuro Auto)
│  └─ ALGarage.Web             → Blazor Web App (host + UI)
└─ tests/
   └─ ALGarage.Domain.Tests    → Exemplo de teste de domínio (motor de estimativa)
```

Detalhes e responsabilidades em [`docs/03-estrutura-da-solution.md`](docs/03-estrutura-da-solution.md).

## ⚙️ Pré-requisitos (quando formos codar)

- [.NET 10 SDK (LTS)](https://dotnet.microsoft.com/) — ver [`global.json`](global.json)
- Docker + Docker Compose (o app + Postgres sobem em container)
- Node não é necessário para o MVP (o 3D é _stretch_)

> ⚠️ **Nota de transparência:** este esqueleto foi gerado em um ambiente **sem o SDK .NET instalado**,
> portanto os `.csproj`/`.sln` **ainda não foram compilados localmente**. São arquivos padrão e
> coerentes, mas o primeiro `dotnet restore && dotnet build` deve ser feito por você (ou pela CI —
> ver [`.github/workflows/ci.yml`](.github/workflows/ci.yml)) e pode exigir pequenos ajustes de
> versão de pacote.

```bash
# Desenvolvimento (após instalar o SDK)
dotnet restore && dotnet build && dotnet test

# Rodar a stack completa (site + Postgres) — inclusive no Raspberry Pi (ARM64)
cp .env.example .env          # defina POSTGRES_PASSWORD
docker compose up -d --build  # acesse http://<ip>:8080
```

➡️ Guia de implantação no Pi (SSD, backup, TLS, migração futura p/ nuvem):
[`docs/08-implantacao-local.md`](docs/08-implantacao-local.md).

## 🗺️ Princípios que guiam o projeto

1. **Monólito modular, Clean Architecture.** Camadas com dependências apontando para dentro. Sem microserviços agora.
2. **Multi-marca por design, Volvo na prática.** `Brand` é cidadã de primeira classe; fontes de dados ficam atrás de _ports_ (`IVinDecoder`, `IPartsSearchLinkProvider`, …). Volvo é só a primeira implementação.
3. **O app só mostra _links_ de peças.** A compra é responsabilidade do usuário. Nada de carrinho/checkout.
4. **Dados são configuração, não código.** Cronogramas de manutenção, intervalos e vida útil de peças moram no banco — não em `if`s.
5. **LGPD desde o dia zero.** VIN é dado pessoal. Minimização, consentimento, exportação e exclusão previstos na arquitetura.
6. **Local-first, dark-first, bilíngue.** Roda na LAN da equipe (Raspberry Pi/ARM64), portável p/ nuvem sem lock-in; **dark mode** é padrão; UI em **en + pt** desde o início.

## 📜 Licença

A definir (ver questões abertas em [`docs/06-riscos-e-questoes-abertas.md`](docs/06-riscos-e-questoes-abertas.md)).
