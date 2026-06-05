# ADR-0006 — Multi-marca por padrão de provider/estratégia

- **Status:** Aceito
- **Data:** 2026-06-05
- **Decisores:** CTO, Tech Lead, PO

## Contexto
Hoje só há Volvo. O brief prevê outras marcas no futuro. Risco dos dois lados: over-engineering (se
construirmos features multi-marca agora) e reescrita cara (se hardcodarmos "Volvo").

## Opções consideradas
1. **Design multi-marca, implementação só Volvo** — `Brand` como entidade; fontes de dados atrás de
   _ports_ (`IVinDecoder`, `IVehicleCatalogProvider`, `IPartsSearchLinkProvider`) com **uma**
   implementação Volvo. Barato agora, sem reescrita depois.
2. **Hardcodar Volvo** — mais rápido hoje, caríssimo de desfazer (lógica espalhada).
3. **Implementar multi-marca completo** — over-engineering: features para marcas que não temos.

## Decisão
**Opção 1.** Separamos **design** (barato, fazemos agora) de **implementação** (cara, só Volvo).
Adicionar uma marca no futuro = nova implementação dos providers + seed de dados, sem tocar no núcleo.

## Consequências
- ✅ Expansão futura vira trabalho de **dados/adapter**, não de reescrita.
- ✅ `Brand` cidadã de 1ª classe no modelo de dados.
- ⚠️ Pequeno overhead de indireção (interfaces) já no MVP — aceito conscientemente.
