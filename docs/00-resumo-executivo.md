# Resumo Executivo — Estruturação do ÄLGarage

**Data:** 2026-06-05
**Fase:** Estruturação (sem implementação de features)
**Decisão:** aprovada por consenso do time após resolução das divergências registradas em
[`01-discussao-do-time.md`](01-discussao-do-time.md).

## O que estamos construindo

Uma aplicação web para o dono de um Volvo acompanhar a **saúde do seu carro**: cadastro do
veículo por **VIN obrigatório**, decodificação do VIN em modelo/versão/ano/motorização,
monitoramento de manutenção (por tempo e por km/dia), histórico de serviços, versões de fábrica,
upgrades/stages e um **buscador de peças que só exibe links** (a compra é do usuário). Arquitetura
preparada para **multi-marca**, **assinatura paga** e **3D**, mas nada disso é construído agora.

## Decisões-chave (TL;DR)

| Tema | Decisão | Por quê (resumo) |
|---|---|---|
| Runtime | **.NET 10 (LTS)** + C# 14 | Suporte longo (3 anos), maturidade, é a base natural do Blazor atual. |
| UI | **Blazor Web App**, render mode **`InteractiveServer`** no MVP | Menor complexidade, time-to-market, latência ok com hospedagem no Brasil. Caminho para `InteractiveAuto` previsto. |
| Banco | **PostgreSQL 16+** | JSONB para catálogo/specs heterogêneos, custo zero de licença, ótimo p/ multi-marca e multicloud. |
| ORM | **EF Core 10** (+ Dapper pontual no futuro p/ leitura pesada) | Produtividade, migrations, LINQ. |
| Auth | **ASP.NET Core Identity** atrás de abstração OIDC-ready | Donos do dado (LGPD), sem custo, migra p/ IdP externo depois. |
| Arquitetura | **Monólito modular + Clean Architecture** | Velocidade de um time pequeno sem fechar portas p/ extração futura. |
| Multi-marca | **Padrão de _providers_/estratégia** por marca; `Brand` como entidade | Volvo hoje sem engessar a expansão. |
| Peças | **Links via API de afiliados/deep-links** (ex.: Mercado Livre). **Sem scraping.** | Legal, estável e dentro dos Termos de Uso. |
| VIN | **NHTSA vPIC (grátis)** como base + provider comercial opcional + **dataset Volvo curado (plano B)** | Resiliência: nenhuma fonte única é confiável p/ o mercado BR. |
| Hospedagem | **Local-first**: Docker **ARM64** num **Raspberry Pi** (site + Postgres), SSD + backup; portável p/ Azure/AWS depois | Ferramenta interna da equipe; custo ~zero, sem lock-in. ([ADR-0012](adr/0012-hosting-deployment.md)) |
| i18n | **Inglês + Português** desde o MVP (`IStringLocalizer`/`.resx`) | Decisão do stakeholder; barato fazer já, caro depois. ([ADR-0013](adr/0013-i18n-en-pt.md)) |
| UI/Tema | **Dark mode obrigatório e padrão**, via design tokens | Requisito do stakeholder. ([ADR-0014](adr/0014-dark-mode.md)) |
| 3D | **Stretch.** Three.js/Babylon via JS interop, começando por **SVG/diagrama 2D** | Modelos 3D por veículo são caros e arriscados; entregamos valor antes com 2D. |

## Os 3 maiores riscos (detalhe em [`06`](06-riscos-e-questoes-abertas.md))

1. **Dados.** Não existe API pública oficial da Volvo para peças/códigos/opcionais de fábrica,
   e o mercado BR é mal coberto. Mitigação: arquitetura de _providers_ + dataset curado próprio.
2. **Buscador de peças.** Depende de programas de afiliados/APIs de marketplaces; scraping é
   risco jurídico e quebra fácil. Mitigação: começar por Mercado Livre (programa oficial) e
   construção de deep-links, nunca raspagem.
3. **3D "explodindo para o esqueleto".** É a feature mais cara e ambiciosa do brief. Modelos 3D
   riggados por veículo, com peças nomeadas, não escalam para a linha Volvo no MVP. Mitigação:
   tratar como stretch e prototipar com diagramas 2D antes.

## Decisões do stakeholder já incorporadas (rodada Q1–Q5)

- **Hospedagem:** local na equipe (Raspberry Pi/ARM64), Docker, Postgres com backup em SSD;
  portável p/ Azure/AWS no futuro. ✅
- **Idiomas:** Inglês + Português, i18n configurado desde já. ✅
- **Scraping:** confirmado **sem scraping** — só deep-links/afiliados. ✅
- **Nuvem:** nada agora (local); futuro portável, sem lock-in. ✅
- **UI:** dark mode obrigatório. ✅

## O que ainda precisamos de VOCÊ

Continua em aberto (ver [`06`](06-riscos-e-questoes-abertas.md)):

1. **Orçamento para dados/APIs pagas** (VIN/peças/specs) — define se o MVP nasce com dados ricos ou
   com dataset curado manual. **(maior bifurcação, ainda sem resposta)**
2. ~~Modelos Volvo iniciais a curar~~ → **resolvido: somente Volvo V40 (2012–2019)**
   ([`09-dataset-v40.md`](09-dataset-v40.md)).
3. **Alinhamento de expectativa do 3D** (2D primeiro; 3D por-VIN pode não ser viável).

## O que NÃO foi feito (de propósito)

Nenhuma feature foi implementada. Não há telas funcionais, nem integração real com APIs externas,
nem modelos 3D. Entregamos: decisões (ADRs), esqueleto da solution, esboço do modelo de dados,
roadmap e este resumo.
