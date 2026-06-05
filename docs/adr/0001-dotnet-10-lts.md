# ADR-0001 — .NET 10 (LTS) + C# 14

- **Status:** Aceito
- **Data:** 2026-06-05
- **Decisores:** CTO, Tech Lead

## Contexto
Produto novo, com expectativa de vida longa. Precisamos de uma base estável e suportada por anos,
que seja a fundação natural do Blazor atual.

## Opções consideradas
1. **.NET 10 (LTS, nov/2025)** — suporte de 3 anos, maturidade, base do Blazor Web App atual.
2. **.NET 8 (LTS anterior)** — sólido, mas mais perto do fim do suporte mainstream; sem ganho ao
   iniciar um projeto novo agora.
3. **.NET 9 (STS)** — só 18 meses de suporte; ruim para produto de longo prazo.

## Decisão
**.NET 10 (LTS)** com **C# 14** (versão que acompanha o SDK). Fixado em [`global.json`](../../global.json).

## Consequências
- ✅ Janela de suporte longa; menos pressão de upgrade no MVP.
- ✅ Recursos recentes de C#/runtime e do Blazor disponíveis.
- ⚠️ Equipe/CI precisam do SDK 10 instalado (este esqueleto foi criado sem o SDK; primeiro build é
  responsabilidade da CI/dev).
