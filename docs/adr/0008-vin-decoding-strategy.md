# ADR-0008 — Estratégia de decode de VIN em camadas + dataset curado

- **Status:** Aceito
- **Data:** 2026-06-05
- **Decisores:** CTO, Especialista Automotivo, Tech Lead

## Contexto
O VIN é obrigatório e a decodificação alimenta tudo (modelo/versão/ano/motor → manutenção, peças,
upgrades). Não existe fonte única confiável para Volvo **e** mercado BR. Ver [`07-fontes-de-dados.md`](../07-fontes-de-dados.md).

## Opções consideradas
1. **Fonte única (vPIC grátis)** — simples, mas centrada nos EUA e sem peças; trims BR divergem.
2. **Fonte única paga** — cobertura melhor, mas custo, limites e dependência externa total.
3. **Camadas (chain of responsibility) + dataset curado** — vPIC base → provider comercial opcional
   (flag) → **dataset curado próprio** como verdade dos modelos do MVP e _fallback_ garantido.

## Decisão
**Opção 3.** `IVinDecoder` implementado como cadeia: vPIC → comercial (feature-flag) → dataset
curado. Resultados mesclados com **proveniência** por campo (qual fonte trouxe cada dado).

## Consequências
- ✅ Resiliência: o produto funciona mesmo sem nenhuma API paga (curado + vPIC).
- ✅ Custo controlado por feature-flag e cache.
- ⚠️ Dataset curado é trabalho manual → começar com **1 modelo (Volvo V40 2012–2019)**; risco de
  gargalo ao ampliar (ver R9 e [`09-dataset-v40.md`](../09-dataset-v40.md)).
- ➡️ Proveniência permite auditar e preferir a fonte mais confiável em conflitos.
