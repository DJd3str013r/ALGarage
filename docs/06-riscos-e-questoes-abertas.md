# Riscos e Questões em Aberto

## Parte A — Riscos (com mitigação)

| # | Risco | Impacto | Prob. | Mitigação |
|---|---|---|---|---|
| R1 | **Sem fonte oficial de peças/specs/opcionais Volvo** (e mercado BR mal coberto) | 🔴 Alto | 🔴 Alta | Arquitetura de _providers_ + **dataset curado próprio** como plano B garantido ([07](07-fontes-de-dados.md)) |
| R2 | **Buscador de peças** depender de afiliados/scraping | 🔴 Alto | 🟡 Média | Só **afiliados/deep-links**; scraping proibido ([ADR-0007](adr/0007-parts-finder-affiliate-links-no-scraping.md)) |
| R3 | **3D por-VIN com esqueleto** estoura prazo/custo | 🟡 Médio | 🔴 Alta | É **stretch**; começar por **2D/SVG**; 3D genérico por categoria ([ADR-0010](adr/0010-3d-visualization-stretch.md)) |
| R4 | **Estimativa de manutenção imprecisa** (km/dia é média) | 🟡 Médio | 🟡 Média | "O que vier primeiro" (km **ou** tempo) conservador; refinar com histórico de hodômetro; OBD no futuro |
| R5 | **LGPD** — VIN/dados do veículo são dados pessoais | 🔴 Alto | 🟡 Média | Residência BR, consentimento, exportação/exclusão na fundação ([ADR-0011](adr/0011-lgpd-data-protection.md)) |
| R6 | **Custo de APIs pagas** corrói viabilidade | 🟡 Médio | 🟡 Média | Feature-flag por provider; cache agressivo; dataset curado reduz dependência |
| R7 | **Render mode Server** sob pico de tráfego (marketing) | 🟢 Baixo | 🟡 Média | Hospedagem BR (latência); caminho para `InteractiveAuto`; escalar horizontal |
| R8 | **Uso de marca "Volvo"** (trademark) sem afiliação oficial | 🟡 Médio | 🟡 Média | Disclaimers de "não afiliado"; revisão jurídica antes de marketing |
| R9 | **Manutenção do dataset curado** vira gargalo manual | 🟡 Médio | 🟡 Média | Começar pequeno (4 modelos); ferramenta de import; comunidade no futuro |
| R10 | **Libs que viraram pagas** (AutoMapper/MediatR/FluentAssertions) | 🟢 Baixo | 🟢 Baixa | Já evitadas na escolha de stack ([02](02-decisoes-de-stack.md)) |

## Parte B — Questões que precisam de DECISÃO SUA

Estas travam ou direcionam o trabalho. Sem elas, seguimos com os _defaults_ marcados, mas a escolha
é sua.

### ❓ Q1 — Orçamento para dados/APIs pagas
A maior bifurcação do projeto. Define se o MVP nasce com **dados ricos (API paga)** ou **curado
manual (poucos modelos)**.
- **Default se você não responder:** **curado manual** + vPIC grátis. Funciona, mas cobre poucos
  modelos no lançamento.

### ❓ Q2 — Mercado e idioma do MVP
- Só **Brasil / PT-BR**? Ou já mirar internacional?
- Afeta fontes de dados (vPIC US vs BR), hospedagem e i18n.
- **Default:** **só Brasil, PT-BR.**

### ❓ Q3 — Política anti-scraping confirmada?
Recomendamos **somente afiliados/APIs oficiais** no buscador de peças. Você confirma?
- **Default:** **sim, sem scraping.**

### ❓ Q4 — Nuvem preferida
Azure (Container Apps, Brazil South), AWS (sa-east-1) ou provedor BR?
- Afeta custo, tooling e contratos.
- **Default:** **Azure Container Apps, Brazil South** (cloud-agnóstico via container, fácil trocar).

### ❓ Q5 — Escopo do "núcleo leve" no MVP
"Versões de fábrica" e "upgrades/stages" entram no **MVP** (telas de leitura) ou ficam para a Fase 2?
- **Default:** **entram como leitura**, se não atrasarem o coração (garagem+estimativa).

### ❓ Q6 — Modelos Volvo iniciais para curar
Sugerimos **XC60, XC40, S60, V40** (populares no BR). Concorda? Trocaria algum?
- **Default:** esses quatro.

### ❓ Q7 — Profundidade do 3D que você realmente quer
Você topa que o "3D explodindo para esqueleto **por veículo**" pode **não** ser viável e que
entregaremos primeiro **2D/SVG** + (talvez) 3D **genérico**? Precisamos de alinhamento de
expectativa aqui, porque é a parte mais propensa a frustrar.
- **Default:** 2D primeiro; 3D real condicional a validação + orçamento de assets.

### ❓ Q8 — Telemetria/OBD-II no horizonte?
Está no brief como "opcional/futuro". Quer que a arquitetura **reserve a porta** `ITelemetrySource`
desde já (barato) ou ignoramos por enquanto?
- **Default:** **reservar a porta**, sem implementar.

---

> **Como responder:** pode responder por número (Q1…Q8). Onde concordar com o _default_, basta dizer
> "default". As respostas de Q1 e Q2 são as que mais mudam o plano da Fase 1.
