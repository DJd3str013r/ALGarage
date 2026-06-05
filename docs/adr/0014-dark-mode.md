# ADR-0014 — Dark mode obrigatório (e padrão), via design tokens

- **Status:** Aceito
- **Data:** 2026-06-05
- **Decisores:** UX/UI, Tech Lead

## Contexto
Requisito do stakeholder: **dark mode é obrigatório**. Para não retrabalhar o CSS depois, o tema
precisa ser uma decisão de fundação, não um "tema aplicado por cima".

## Decisão
- **Dark é o tema padrão** e é garantido. Light é opcional.
- Implementar com **CSS custom properties (design tokens)** — `--bg`, `--surface`, `--text`,
  `--accent`, e os estados de manutenção (`--state-ok/--state-due-soon/--state-overdue`, o "vermelho"
  do alerta). Trocar de tema = trocar o valor das variáveis, sem reescrever componentes.
  Ver [`wwwroot/app.css`](../../src/ALGarage.Web/wwwroot/app.css).
- Alternância via `data-theme` no `<html>`, persistida em `localStorage`, com **script anti-flash**
  aplicado antes do `<body>` (evita piscar branco no load). Ver
  [`wwwroot/js/theme.js`](../../src/ALGarage.Web/wwwroot/js/theme.js) e o componente
  [`ThemeToggle.razor`](../../src/ALGarage.Web/Components/Shared/ThemeToggle.razor).

## Consequências
- ✅ Tema é trocável e consistente; novos componentes herdam os tokens.
- ✅ Os estados de manutenção têm cores semânticas centralizadas (úteis no 2D/3D futuro).
- ⚠️ Componentes devem **sempre** usar os tokens (nunca cores fixas) — ponto de revisão de PR.
- ➡️ Light mode fica disponível "de graça"; respeitar `prefers-color-scheme` na 1ª visita é melhoria
  fácil futura.
