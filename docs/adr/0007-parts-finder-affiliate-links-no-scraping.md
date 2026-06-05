# ADR-0007 — Buscador de peças via afiliados/deep-links, sem scraping

- **Status:** Aceito
- **Data:** 2026-06-05
- **Decisores:** CTO, PM, Marketing, Tech Lead

## Contexto
O MVP deve "agregar e exibir **links** dos maiores sites de busca de peças" — o app **só mostra
links**, a compra é do usuário. A forma de obter esses links tem implicações legais e de manutenção.

## Opções consideradas
1. **Deep-links de busca** — montar a URL de busca da loja com a query (VIN/peça). Legal, estável
   (aponta para resultados, não para SKU), sem receita direta.
2. **API de afiliados** (ex.: Mercado Livre Developers) — dados ricos (título/preço) + **comissão**;
   dentro dos Termos. Exige cadastro/certificação.
3. **Scraping** de resultados — rico, mas **viola ToS**, **quebra** a cada mudança de HTML e gera
   **risco jurídico**. Rejeitado.

## Decisão
**Deep-links de busca como base do MVP**, **enriquecidos por APIs de afiliados** na Fase 2.
**Scraping é proibido.** Cada loja é um `IPartsSearchLinkProvider`; adicionar loja não toca no resto.

## Consequências
- ✅ Conformidade legal e links estáveis.
- ✅ Caminho de **receita** via afiliados (alinha com o modelo de assinatura/comissão).
- ⚠️ Sem scraping, dados ricos dependem de cada loja ter API/afiliado — começamos só com links.
- ➡️ Validar disponibilidade em tempo real onde houver API.
