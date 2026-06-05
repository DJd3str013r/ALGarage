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
| Hospedagem | **Containers** (Docker) em região **Brasil** (Azure Container Apps `Brazil South` ou AWS `sa-east-1`) | Latência + residência de dados (LGPD). Cloud-agnóstico via container. |
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

## O que precisamos de VOCÊ antes de avançar

As perguntas que travam decisões estão em [`06-riscos-e-questoes-abertas.md`](06-riscos-e-questoes-abertas.md).
As mais urgentes:

1. **Orçamento para dados/APIs pagas** (VIN/peças/specs) — define se o MVP nasce com dados ricos ou com dataset curado manual.
2. **Mercados/idiomas alvo do MVP** — só Brasil/PT-BR? Isso muda fontes de dados e hospedagem.
3. **Apetite por scraping** — confirmamos a política "somente afiliados/APIs oficiais"?
4. **Nuvem preferida** (Azure vs AWS vs provedor BR) — afeta custo e tooling.

## O que NÃO foi feito (de propósito)

Nenhuma feature foi implementada. Não há telas funcionais, nem integração real com APIs externas,
nem modelos 3D. Entregamos: decisões (ADRs), esqueleto da solution, esboço do modelo de dados,
roadmap e este resumo.
