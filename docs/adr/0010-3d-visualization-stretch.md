# ADR-0010 — Visualização 3D como stretch (2D primeiro)

- **Status:** Aceito
- **Data:** 2026-06-05
- **Decisores:** CTO, UX/UI, Tech Lead

## Contexto
A visão: girar o carro em 3D, peças perto da troca em **vermelho**, clicar e "explodir" para o
esqueleto com a peça destacada. É o maior "uau" do brief — e a parte mais cara e arriscada.

## Por que é caro/arriscado
- Exige **modelo 3D riggado por veículo** (ou por plataforma) com **meshes nomeados por peça** para
  destacar individualmente.
- Licenciamento/modelagem desses assets é caro e lento; a linha Volvo é grande.
- Mapear cada peça monitorada a um mesh é trabalho **manual gigante** por veículo → não escala no MVP.

## Opções consideradas
1. **3D por-VIN com esqueleto** — fiel à visão, mas inviável no MVP (custo/escala de assets).
2. **2D/SVG com hotspots** — diagrama por categoria, peças `DueSoon` em vermelho, clique abre
   detalhe/links. ~80% do valor por ~20% do custo.
3. **3D genérico por categoria** (não por VIN) — meio-termo: um modelo glTF por tipo de carroceria,
   via **Three.js/Babylon** em **JS interop**.

## Decisão
**3D é stretch.** Começamos por **2D/SVG (opção 2)**. **3D real (opção 3)** só depois, condicionado a
validação de demanda e orçamento de assets, com modelo **genérico por categoria**. **Não prometemos
3D por-VIN.** O render mode do Blazor é irrelevante aqui — 3D é JS no cliente de qualquer forma.

## Consequências
- ✅ Entregamos a experiência "peça vermelha → detalhe" cedo e barato.
- ✅ Sem comprometer prazo do MVP com pipeline 3D.
- ⚠️ Expectativa precisa ser alinhada com o stakeholder (ver Q7 em [`06`](../06-riscos-e-questoes-abertas.md)).
