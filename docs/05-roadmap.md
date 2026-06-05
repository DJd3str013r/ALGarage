# Roadmap em Fases

Princípio: **profundidade antes de amplitude**. Poucos modelos Volvo bem cobertos > linha inteira mal
coberta. Cada fase entrega valor de ponta a ponta.

## Fase 0 — Fundação (esta fase) ✅

- [x] Decisões de arquitetura (ADRs)
- [x] Esqueleto da solution + camadas
- [x] Esboço do modelo de dados
- [x] Estratégia de fontes de dados + plano B
- [ ] *(pendente de você)* respostas às questões abertas (orçamento, mercado, nuvem, scraping)

**Saída:** repositório estruturado, pronto para `dotnet build` e para começar features.

---

## Fase 1 — MVP "Garagem que avisa" 🎯 (em andamento)

O coração do produto: cadastrar o carro e ser avisado da manutenção.

> **Status (incremento 1 entregue):** backbone completo — persistência EF Core (configs +
> seeding do V40), cadeia de decodificação de VIN, serviços de aplicação (cadastro, garagem,
> estimativa, histórico, peças) com testes da lógica pura; auth ASP.NET Identity e UI Blazor (SSR,
> dark/bilíngue) da garagem. Falta: gerar a migration inicial e validar o build local (este ambiente
> não tem o SDK .NET), refinos de UX e mais testes de integração.

1. **Auth + conta** (ASP.NET Identity) com **VIN obrigatório** no fluxo de cadastro do 1º carro.
2. **Decodificação de VIN** via `IVinDecoder` (vPIC + fallback dataset curado) → resolve
   `ModelVariant`. Estado gracioso quando não identifica.
3. **Garagem**: CRUD de `Vehicle` com data de compra, donos anteriores, km na aquisição, km atual,
   km/dia. Leituras de hodômetro.
4. **Motor de estimativa de manutenção**: troca de óleo e itens por **km e/ou tempo**, "o que vier
   primeiro", projeção por km/dia. Estados `Ok`/`DueSoon`/`Overdue`.
5. **Histórico de serviços**: registrar serviço feito → **reseta** o relógio do item.
6. **Buscador de peças (links)**: deep-links de busca a partir de VIN/peça; **só links**.
7. **Dados Volvo curados** para o **Volvo V40 (2012–2019)** — todas as versões/motores/extras
   ([`09-dataset-v40.md`](09-dataset-v40.md)). Demais modelos ficam para depois.

**Critério de pronto:** um dono de V40 cadastra pelo VIN, vê "óleo vence em ~38 dias", registra a
troca e o contador zera. Tudo rodando **localmente no Pi** (Docker + Postgres com backup), com LGPD
básico (consentimento, exclusão).

**Núcleo leve (telas de leitura, se couber sem atrasar):**
- **Versões de fábrica**: exibir `ModelVariant` + `FactoryOption` do veículo.
- **Upgrades/stages**: exibir `Upgrade`/`Stage` curados.

---

## Fase 2 — Enriquecimento de dados e peças

- **Provider comercial de VIN/specs** (feature-flag) para ampliar cobertura além dos modelos curados.
- **API de afiliados** (Mercado Livre) no buscador: títulos/preços + comissão. Posicionamento de
  receita começa aqui.
- **Códigos OEM de peça** (`PartReference.OemPartNumber`) onde houver fonte; compatibilidade.
- **Catálogo Volvo mais amplo** (mais modelos/anos).
- **Notificações** (e-mail) de "manutenção próxima".

---

## Fase 3 — Stretch: visualização do carro

- **Etapa 3a (barata):** diagrama **2D em SVG** por categoria, com **hotspots** nas peças
  monitoradas; peça `DueSoon` fica **vermelha**; clique abre detalhe/links. Entrega 80% do "uau".
- **Etapa 3b (cara, condicional):** **3D real** com Three.js/Babylon via JS interop, modelo **glTF
  genérico por categoria** (não por VIN), com "explosão" para esqueleto. Só avança se 3a validar
  demanda e se houver orçamento de assets. **Sem promessa de 3D por-VIN.**

---

## Fase 4 — Futuro de negócio (prever, não construir agora)

- **Assinatura paga**: ativar `Plan`/`Subscription`/`Entitlement` (já previstos) + gateway
  (**Mercado Pago** para BR / Stripe). Gates de feature por entitlement.
- **Multi-marca real**: segunda marca (ex.: BMW) = nova implementação de `IVinDecoder`/catálogo +
  seed. O design já permite; vira trabalho de dados, não de reescrita.
- **Comunidade**: usuários contribuem specs/planos de manutenção (com moderação).
- **Telemetria / OBD-II** (opcional): dongle/app lê km e DTCs em tempo real, alimentando a
  estimativa com dados reais em vez de média. Atrás de uma porta `ITelemetrySource`.

---

## Linha do tempo (ordem, não datas)

```
Fase 0  ──► Fase 1 (MVP) ──► Fase 2 (dados+peças) ──► Fase 3a (2D) ┬─► Fase 3b (3D, condicional)
                                                                    └─► Fase 4 (assinatura/multi-marca/OBD)
```

Datas dependem das respostas às [questões abertas](06-riscos-e-questoes-abertas.md) — principalmente
orçamento de dados, que define quanto do MVP é curado-manual vs. API paga.
