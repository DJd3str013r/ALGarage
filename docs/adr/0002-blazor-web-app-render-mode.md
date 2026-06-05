# ADR-0002 — Blazor Web App, render mode `InteractiveServer` no MVP

- **Status:** Aceito
- **Data:** 2026-06-05
- **Decisores:** CTO, Tech Lead, UX/UI

## Contexto
Precisamos escolher o modelo de renderização do Blazor. A ideia do 3D levou parte do time a defender
WebAssembly (WASM) por desempenho no cliente. O público é majoritariamente brasileiro.

## Opções consideradas
1. **`InteractiveServer`** — UI no servidor via SignalR. Simples, sem API separada, auth fácil,
   rápido de entregar. Custo: estado por conexão, exige conexão viva, sensível a latência.
2. **`InteractiveWebAssembly`** — roda no cliente. Bom p/ offline/escala de conexões. Custo: payload
   inicial, DTOs duplicados, exige API HTTP separada, auth mais complexa.
3. **`InteractiveAuto`** — Server no 1º acesso, WASM depois. Melhor UX a longo prazo, mas exige
   projeto `.Client` e a complexidade do WASM desde já.

## Decisão
**Blazor Web App** com render mode global **`InteractiveServer`** no MVP. Estrutura preparada para
adicionar `ALGarage.Web.Client` e migrar para **`InteractiveAuto`** quando houver necessidade real.

**O 3D não pesa nessa decisão:** roda em JS (Three.js/Babylon) via JS interop, independente do render
mode (ver [ADR-0010](0010-3d-visualization-stretch.md)).

## Consequências
- ✅ Menor complexidade e maior velocidade no MVP.
- ✅ Latência mínima: o app roda na **LAN** da equipe (Raspberry Pi local — [ADR-0012](0012-hosting-deployment.md)).
- ⚠️ Estado por conexão consome memória; escala de muitas conexões simultâneas exige atenção.
- ➡️ Migração para `Auto` é evolução planejada, não retrabalho — o `Shared` já isola contratos.
