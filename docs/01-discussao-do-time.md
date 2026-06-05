# Discussão do Time — Divergências e Convergências

Este documento registra **como** chegamos às decisões. O objetivo é deixar visíveis os pontos de
atrito entre os papéis e a forma como foram resolvidos — não só o consenso final. Tom direto: onde
uma ideia é ruim ou cara demais, está dito.

**Participantes (subagentes):**
🧭 **PO** · 🧪 **PM** · 🏛️ **CTO/Arquiteto** · 🔧 **Tech Lead** · 🎨 **UX/UI** ·
🚗 **Especialista Automotivo** · 📣 **Marketing**

---

## Rodada 1 — Qual é, de fato, o MVP?

🧭 **PO:** O brief lista 8 funcionalidades no "núcleo". Isso **não** é um MVP, é um produto v1.
Se tentarmos as 8 com qualidade, não entregamos em tempo razoável. Precisamos cortar.

🧪 **PM:** Concordo. A proposta de valor que diferencia o ÄLGarage é **"meu carro vai precisar de
manutenção X em ~Y dias"**. Isso depende de: (a) identificar o carro (VIN), (b) ter o cronograma de
manutenção, (c) calcular com km/dia. O resto orbita isso.

🚗 **Especialista Automotivo:** Cuidado: o cálculo só é confiável se tivermos o **plano de
manutenção por modelo/motor**. E essa é exatamente a parte que **não** vem de graça de nenhuma API.
Sem isso, o "motor de estimativa" vira chute.

🏛️ **CTO:** Então o risco real do MVP não é técnico, é de **dados**. Proponho um MVP que funcione
mesmo com dados curados manualmente para **um punhado de modelos Volvo populares no Brasil**
(ex.: XC60, XC40, S60, V40). Profundidade > amplitude.

🔧 **Tech Lead:** De acordo, e isso encaixa com "dados são configuração". Se o cronograma mora no
banco, começar com 4 modelos e crescer é só inserir linhas, não reescrever código.

> **🟢 Convergência:** MVP = **Garagem + VIN + Estimativa de manutenção + Histórico + Buscador de
> peças (links)**, com **versões de fábrica e upgrades** entrando como "núcleo leve" (telas de
> leitura sobre dados curados). 3D fica fora. Profundidade inicial: poucos modelos Volvo BR.

**Divergência residual:** PO queria tirar "versões de fábrica" e "upgrades" do MVP; UX argumentou
que são telas de **leitura** baratas (só exibem dados) e dão sensação de produto completo. Resolvido
a favor de manter, **desde que sejam apenas leitura de dados curados** — sem lógica nova.

---

## Rodada 2 — Render mode do Blazor: Server, WASM ou Auto?

🎨 **UX/UI:** Para o 3D e para interações ricas, WASM roda no cliente e não sofre com latência de
circuito SignalR. Quero WASM.

🔧 **Tech Lead:** WASM puro traz dor no MVP: payload de download inicial, duplicação de DTOs,
necessidade de uma API HTTP separada para o cliente consumir, e auth mais chata. Triplicamos o custo
de cada feature numa fase em que queremos velocidade.

🏛️ **CTO:** O 3D é **stretch** e, quando vier, roda em **JavaScript (Three.js/Babylon) via JS
interop** — isso independe do render mode. Ou seja, o argumento "preciso de WASM por causa do 3D"
não se sustenta: o 3D é JS no cliente de qualquer jeito.

🎨 **UX/UI:** Justo. Mas latência de digitação/validação em formulários no Server incomoda.

🏛️ **CTO:** Com hospedagem **no Brasil** (Brazil South / sa-east-1), o RTT do SignalR fica baixo
para o público-alvo. Para o MVP isso é aceitável.

🔧 **Tech Lead:** Proposta: **Blazor Web App** com render mode global **`InteractiveServer`** no MVP.
Estruturamos o host já pensando em adicionar um projeto **`.Client`** e migrar para
**`InteractiveAuto`** quando (e se) precisarmos de offline/escala de conexões.

> **🟢 Convergência:** `InteractiveServer` no MVP, arquitetura pronta para `InteractiveAuto`. O 3D
> não entra nessa conta porque é JS interop.

**Trade-off aceito conscientemente:** Server mantém estado por conexão (memória/escala) e exige
conexão viva. Para o volume de MVP, tudo bem. Reavaliar antes de campanhas de marketing que tragam
picos.

---

## Rodada 3 — Banco: PostgreSQL ou SQL Server?

🔧 **Tech Lead:** SQL Server tem a melhor integração com a stack .NET e tooling redondo. Mas custa
licença em produção (ou nos amarra ao Azure SQL).

🏛️ **CTO:** Nossos dados de **catálogo, specs e opcionais de fábrica são heterogêneos por marca**.
Isso é JSON semiestruturado. PostgreSQL com **JSONB** indexável modela isso muito melhor sem virar
um inferno de tabelas esparsas. Some custo zero de licença e portabilidade multicloud.

🔧 **Tech Lead:** EF Core fala com os dois igualmente bem hoje. Sem objeção técnica forte.

🚗 **Especialista Automotivo:** Os opcionais variam absurdamente entre versões e mercados. Forçar
isso num esquema relacional rígido vai doer. JSONB ajuda.

> **🟢 Convergência:** **PostgreSQL 16+** com **EF Core 10** (Npgsql). Specs/opcionais como JSONB;
> entidades de negócio (carro, manutenção, serviço) relacionais.

---

## Rodada 4 — Buscador de peças: afiliados/API ou scraping?

🧪 **PM:** O brief diz "agrega links dos maiores sites de busca de peças". A forma mais rica de
fazer isso é raspar resultados.

🏛️ **CTO:** **Não.** Scraping de marketplaces viola Termos de Uso, quebra a cada mudança de HTML,
e nos expõe juridicamente — ainda mais com a marca de terceiros envolvida. É dívida desde o dia um.

📣 **Marketing:** E há um ângulo de receita: **programas de afiliados** (Mercado Livre, etc.) pagam
comissão por clique/venda. Se vamos mandar tráfego para lojas, queremos a comissão **e** a
legalidade. Afiliado resolve os dois.

🔧 **Tech Lead:** Tecnicamente, "mostrar links" não exige catálogo real. Construímos **deep-links de
busca** (URL com a query montada a partir de VIN/peça) e, onde houver **API/afiliado oficial**,
enriquecemos com preço/título. Tudo atrás de um `IPartsSearchLinkProvider`.

🚗 **Especialista Automotivo:** Importante separar dois conceitos que o brief mistura: **"código de
peça (OEM/part number)"** é um *dado de catálogo* (precisa de fonte); **"link de busca"** é só uma
URL. O buscador de peças do MVP é a segunda coisa. Códigos de peça são um problema de fonte de dados
à parte.

> **🟢 Convergência:** Buscador de peças = **deep-links + APIs de afiliados**, **nunca scraping**.
> Códigos OEM são um problema de _fonte de dados_ separado (ver Rodada 6). Decisão registrada em
> [ADR-0007](adr/0007-parts-finder-affiliate-links-no-scraping.md).

---

## Rodada 5 — Multi-marca agora ou depois?

🧭 **PO:** Só temos Volvo. Modelar multi-marca agora é over-engineering clássico.

🏛️ **CTO:** Discordo em parte. Não vou construir _features_ multi-marca, mas **uma decisão de
modelagem barata agora** (ter `Brand` como entidade e fontes de dados atrás de _ports_) evita uma
reescrita cara depois. O caro é hardcodar "Volvo" em 200 lugares.

🔧 **Tech Lead:** É a diferença entre _design_ multi-marca (barato, fazemos) e _implementação_
multi-marca (cara, não fazemos). `IVinDecoder`, `IVehicleCatalogProvider`, etc., com **uma**
implementação Volvo hoje. Adicionar BMW amanhã = nova implementação + linhas no banco.

> **🟢 Convergência:** **Design** multi-marca via padrão de provider/estratégia; **implementação**
> só Volvo. [ADR-0006](adr/0006-multi-brand-provider-pattern.md).

---

## Rodada 6 — Fontes de dados de VIN e peças (a discussão mais espinhosa)

🚗 **Especialista Automotivo:** Vamos ser honestos com o brief. Ele pede: decodificar VIN →
modelo/versão/ano/motor, e daí "peças, códigos de peças, peças compatíveis, upgrades e stages".
A parte de decodificar VIN é viável. A parte de **códigos de peça e compatibilidade** **não existe**
como API pública da Volvo.

🏛️ **CTO:** Inventário do que existe:
- **NHTSA vPIC** (grátis, sem chave): decodifica VIN para specs de fábrica de veículos vendidos nos
  EUA. Cobre Volvo razoavelmente, mas é centrado no mercado americano e **não tem peças**.
- **APIs comerciais** (vehicledatabases.com, 17vin, etc.): VIN + specs + às vezes catálogo de peças
  OEM. Pagas, com limites, e cobertura BR incerta.
- **Mercado BR**: APIs de placa/FIPE (apiplacas, fipeapi) dão dados de emplacamento e valor — úteis,
  mas também **sem peças** e sem opcionais de fábrica.

🔧 **Tech Lead:** Conclusão: **nenhuma fonte única** atende. Precisamos de uma estratégia em camadas
com **plano B que não dependa de ninguém**.

🏛️ **CTO:** Proposta — _chain of responsibility_ no `IVinDecoder`:
1. **vPIC** (grátis) para o decode básico do WMI/modelo/ano.
2. **Provider comercial** (opcional, feature-flag) para enriquecer.
3. **Dataset Volvo curado próprio** (seed no banco) como verdade para os modelos do MVP e como
   _fallback_ garantido.

🧭 **PO:** O dataset curado é trabalho manual. Quem mantém?

📣 **Marketing / 🚗 Especialista:** Começa pequeno (4 modelos), e há espaço para **comunidade**
contribuir versões/specs no futuro (com moderação). Mas isso é _futuro_, não MVP.

> **🟢 Convergência:** Estratégia de fonte em camadas com **dataset curado como plano B garantido**.
> Detalhe em [`07-fontes-de-dados.md`](07-fontes-de-dados.md) e
> [ADR-0008](adr/0008-vin-decoding-strategy.md).

**Divergência residual não resolvida pelo time → vira pergunta para o stakeholder:** quanto há de
**orçamento** para APIs pagas? Isso decide se o MVP nasce rico ou curado-manual. Registrado em
[`06`](06-riscos-e-questoes-abertas.md).

---

## Rodada 7 — A ideia do 3D

🎨 **UX/UI:** A visão é linda: o usuário gira o carro em 3D, peças perto da troca ficam vermelhas,
clica e o carro "explode" para o esqueleto com a peça destacada. É o "uau" do produto.

🏛️ **CTO:** É lindo e é **caro**. Para fazer de verdade preciso de:
- um **modelo 3D riggado por veículo** (ou ao menos por plataforma), com **meshes nomeados por
  peça** para poder destacar individualmente;
- licenciamento/modelagem desses assets — caro e lento, e a linha Volvo é grande;
- pipeline de carregamento (glTF) e performance no cliente.

🔧 **Tech Lead:** O "explodir para o esqueleto com a peça X destacada" exige que cada peça
monitorada tenha correspondência com um mesh no modelo. Isso é um **mapeamento manual gigante** por
veículo. Não escala no MVP.

🎨 **UX/UI:** Contraproposta para extrair 80% do valor por 20% do custo: começar com
**diagramas 2D em SVG** (ou um modelo 3D **genérico** por categoria, não por VIN), com **hotspots**
clicáveis nas peças monitoradas. O "vermelho = perto da troca" funciona igual e é barato.

🏛️ **CTO:** Tecnicamente, quando formos para 3D real, é **Three.js ou Babylon.js via JS interop**
no Blazor — não há lib .NET madura que substitua. Fica como stretch.

> **🟢 Convergência:** 3D é **stretch**. Prototipar primeiro com **SVG/diagrama 2D + hotspots**.
> 3D real (glTF + Three/Babylon via interop) só depois, e provavelmente com modelo **genérico por
> categoria**, não por veículo. [ADR-0010](adr/0010-3d-visualization-stretch.md). **Dito sem rodeios:
> 3D por-VIN com esqueleto explodido é a parte mais propensa a estourar prazo/orçamento do brief.**

---

## Rodada 8 — Assinatura e posicionamento (futuro)

📣 **Marketing:** O modelo de assinatura é o futuro do negócio. Não construímos agora, mas a
arquitetura tem que **prever entitlements/planos** para não exigir reescrita. E o posicionamento é
"o app de quem **ama** o carro" — entusiasta Volvo primeiro, não oficina.

🧪 **PM:** De acordo. Previsão na arquitetura: um conceito de **Plan/Entitlement** desde já (mesmo
que todo mundo seja "Free" no MVP), e gates de feature lendo entitlement, não `if (isPremium)`
espalhado.

🏛️ **CTO:** Barato e previne dor. Entra como **entidade prevista** no modelo de dados, sem
billing real. Integração de pagamento (Stripe/Mercado Pago) é _futuro_.

> **🟢 Convergência:** Prever `Plan`/`Subscription`/`Entitlement` no modelo; **zero** billing no
> MVP. Tudo "Free". Marketing alinha posicionamento "entusiasta Volvo".

---

## Rodada 9 — LGPD

🏛️ **CTO:** VIN identifica um veículo e, ligado a uma conta, é **dado pessoal**. Combinado com
placa/histórico, fica sensível. Precisamos de base legal, minimização, residência de dados no
Brasil, e direitos do titular (exportar/excluir).

🔧 **Tech Lead:** Implicações concretas no esqueleto: hospedagem em região BR, criptografia em
repouso, _soft delete_ + rotina de exclusão real, exportação de dados do usuário, e log de
consentimento. Nada disso é feature de produto, é fundação.

> **🟢 Convergência:** LGPD é requisito de fundação, não _backlog_. [ADR-0011](adr/0011-lgpd-data-protection.md).

---

## Placar final das divergências

| # | Tema | Defendido por | Decisão | Resolução |
|---|---|---|---|---|
| 1 | Tamanho do MVP | PO (cortar) vs UX (manter telas de leitura) | MVP enxuto + telas de leitura | Compromisso: leitura barata fica |
| 2 | Render mode | UX (WASM) vs Tech Lead (Server) | `InteractiveServer` | 3D é JS interop, não justifica WASM |
| 3 | Banco | Tech Lead (SQL Server) vs CTO (Postgres) | PostgreSQL | JSONB + custo + multicloud |
| 4 | Buscador de peças | PM (scraping) vs CTO/Marketing (afiliados) | Afiliados/deep-links | Legal + receita |
| 5 | Multi-marca | PO (depois) vs CTO (design agora) | Design agora, impl. Volvo | Barato evitar reescrita |
| 6 | Fontes de dados | — (consenso no método) | Camadas + dataset curado | **Orçamento → pergunta ao stakeholder** |
| 7 | 3D | UX (3D real) vs CTO (caro) | Stretch, começa 2D | Custo/escala |
| 8 | Assinatura | Marketing (prever) vs PO (ignorar) | Prever entidade, sem billing | Barato, previne dor |

**Pontos que o time NÃO consegue decidir sozinho** (vão para você): orçamento de dados, mercados-alvo
do MVP, nuvem preferida, e confirmação da política anti-scraping. Ver
[`06-riscos-e-questoes-abertas.md`](06-riscos-e-questoes-abertas.md).

---

## 📌 Atualização pós-respostas do stakeholder (2026-06-05)

Algumas premissas das rodadas acima mudaram com as respostas recebidas. Este documento é um **registro
histórico do debate**; as decisões vigentes são:

- **Hospedagem:** não é mais nuvem BR. É **local na equipe** — Raspberry Pi/ARM64, Docker, Postgres
  com backup em SSD; **portável** para Azure/AWS depois, sem lock-in. (As conclusões sobre latência
  do render mode Server **continuam válidas e até reforçadas** — agora é LAN.) → [ADR-0012](adr/0012-hosting-deployment.md).
- **Idiomas:** **Inglês + Português** desde o MVP (i18n). → [ADR-0013](adr/0013-i18n-en-pt.md).
- **Scraping:** **confirmado sem scraping**. → [ADR-0007](adr/0007-parts-finder-affiliate-links-no-scraping.md).
- **UI:** **dark mode obrigatório**. → [ADR-0014](adr/0014-dark-mode.md).
- **Produto:** o escopo atual é **ferramenta interna da equipe** (não consumidor público). Assinatura/
  marketing permanecem como _futuro_, previstos na arquitetura.

Ainda em aberto: **orçamento de dados (Q1)** — a maior bifurcação. Ver [`06`](06-riscos-e-questoes-abertas.md).
