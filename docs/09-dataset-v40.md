# Dataset Curado — Volvo V40 (2012–2019)

Escopo inicial definido pelo stakeholder (Q6): **apenas o Volvo V40, geração II (plataforma P1),
anos 2012–2019**, com todas as versões, motores e extras necessários para o sistema funcionar.
Profundidade num único modelo antes de ampliar.

- **Arquivo:** [`src/ALGarage.Infrastructure/Seed/Data/volvo-v40-2012-2019.json`](../src/ALGarage.Infrastructure/Seed/Data/volvo-v40-2012-2019.json)
- **Loader (código real):** [`Seed/CuratedDatasetLoader.cs`](../src/ALGarage.Infrastructure/Seed/CuratedDatasetLoader.cs)
- **Origem dos dados:** `curated+volvo-manual` (schema v2) — intervalos **alinhados ao manual/VIDA da
  Volvo**; graus/volumes de óleo por motor. Ver disclaimer abaixo.

## Por que um dataset curado (e não uma API)

Como detalhado em [`07-fontes-de-dados.md`](07-fontes-de-dados.md), **não existe API pública oficial
da Volvo** para versões/opcionais/manutenção, e o V40 saiu de linha. O caminho confiável é **curar**
os dados deste modelo. O dataset é a "fonte de verdade" do MVP e o _fallback_ garantido do
`IVinDecoder`.

## O que tem dentro

| Seção | Conteúdo |
|---|---|
| `brand` | Volvo. |
| `models` | `V40` (hatch) e `V40 Cross Country`. |
| `engines` | Lineup **global** 2012–2019: gasolina (1.6 EcoBoost, 2.0/2.5 cinco cilindros, Drive-E 1.5/2.0) e diesel (1.6, 2.0 cinco cilindros, Drive-E 2.0). Cada um com código (ex.: `B4204T`), família, potências, cilindros, fase (pré-facelift/facelift) e anos. |
| `variants` | Combinações trim × motor × ano × mercado. **`market: "BR"`** marca o que foi vendido no Brasil (gasolina); diesels entram como `EU` para completude ("todas as versões"). |
| `factoryOptions` | Extras/equipamentos (City Safety, airbag de pedestre, BLIS, teto panorâmico, Harman Kardon, Sensus, Xenon/Thor's Hammer, rodas, etc.). |
| `maintenancePlans` | Planos por **família de motor**, com itens (óleo, filtros, velas, fluido de freio, correia dentada, transmissão, Haldex p/ AWD…), cada um com `intervalKm` e/ou `intervalMonths` e `partHint` para o buscador de peças. |

## Como mapeia no modelo de dados

```
brand            → Brand
models[]         → VehicleModel
engines[]        → EngineSpec        (compartilhado; ligado por Family ao plano)
variants[]       → ModelVariant      (referencia EngineSpec; resolvido via VIN)
factoryOptions[] → FactoryOption
maintenancePlans[].items[] → MaintenancePlan + MaintenanceItem  (por EngineFamily)
```

Esse dataset motivou um **refinamento do modelo** (ver [`04-modelo-de-dados.md`](04-modelo-de-dados.md)):
o motor virou entidade compartilhada e o plano de manutenção passou a se ligar à **família de motor**.
O `EngineSpec` ganhou ainda `OilGrade` e `OilCapacityLiters`.

## Intervalos oficiais (resumo, gasolina Drive-E)

Valores alinhados ao manual do proprietário / programa VIDA da Volvo (o que vier primeiro):

| Item | km | Tempo | Observação |
|---|---|---|---|
| Óleo + filtro de óleo | 20.000 | 12 meses | Programa Volvo BR historicamente oferecia revisão a cada 10.000 km |
| Filtro de cabine | 20.000 | 12 meses | Tipicamente a cada serviço anual |
| Filtro de ar | ~60.000 | 48 meses | Volvo inspeciona a cada serviço; troca por condição |
| Velas de ignição | 60.000 | 48 meses | — |
| Fluido de freio | — | 24 meses | VIDA: 2 anos (1 ano em uso úmido/montanha) |
| Líquido de arrefecimento | 240.000 | 120 meses | Long-life; sem troca regular em uso normal |
| **Correia dentada** | 120.000 | 120 meses (10 anos) | Diesel D3/D4 e 5-cil: 180.000; diesel D2: 140.000 |
| Fluido transmissão (Aisin) | ~120.000 | — | Volvo: "fill for life"; troca preventiva recomendada |
| Óleo Haldex (AWD) | 60.000 | 36 meses | Só Cross Country / AWD |

### Óleo por motor (grau / volume aprox.)

| Motor | Grau | Volume |
|---|---|---|
| B4204T (Drive-E 2.0, T4/T5) | 0W-20 (VCC RBS0-2AE) | ~5,2 L |
| B4154T (Drive-E 1.5, T2/T3) | 0W-20 (VCC RBS0-2AE) | ~5,5 L |
| B5254T / B5204T (5-cil 2.5/2.0) | 0W-30 (VCC 95200377) | ~5,8 L |
| B4164T (1.6 EcoBoost) | 5W-30 (ACEA A5/B5) | ~4,1 L |
| D4204T / D5204T (diesel 2.0) | 0W-30 (ACEA C) | ~5,2–5,9 L |
| D4162T (diesel 1.6) | 0W-30 (ACEA C) | ~3,9 L |

## Como usar (quando o banco existir)

```csharp
var dataset = CuratedDatasetLoader.LoadV40();
var summary = CuratedDatasetLoader.Summarize(dataset); // contagens p/ sanity check
// TODO(feature): mapear para entidades e fazer upsert idempotente no seed do banco.
```

O loader é executável agora (lê o JSON embutido). A **persistência** (migrations + upsert) é tarefa
da Fase 1 — o esqueleto não cria o banco ainda.

## ⚠️ Disclaimer de qualidade

- **Schema v2:** os intervalos foram **alinhados às recomendações oficiais da Volvo** (manual do
  proprietário / programa de serviço VIDA), substituindo os palpites conservadores da v1.
- **Acesso direto bloqueado:** `volvocars.com` (e agregadores) retornaram **HTTP 403** ao fetch neste
  ambiente, então os valores foram **cruzados de fontes secundárias confiáveis** que reproduzem o
  manual/VIDA (ver fontes). **Não fazemos scraping** (ADR-0007).
- **Volumes de óleo são aproximados** (variam por ano/versão) — confirmar no manual da versão exata.
- **Códigos de opcionais (`FactoryOption.code`) são sintéticos/curados**, não os códigos internos da
  Volvo.
- Pontos que ainda valem confirmar contra o manual da versão: km exato da **correia dentada** do
  **2.5/2.0 cinco cilindros**; intervalo de **óleo do diesel** (fontes citam 20.000 e 30.000 km);
  lineup **BR pré-2015** (nomes Comfort/Dynamic e o 2.0 cinco cilindros 180cv).

## Fontes consultadas

- Volvo V40 (2012–2019) — Wikipedia: <https://en.wikipedia.org/wiki/Volvo_V40_(2012%E2%80%932019)>
- Óleo (grau e volume) — Volvo Support: <https://www.volvocars.com/uk/support/car/v40/article/4d76e576c58a3c76c0a801e801c396ac/>
- Plano de serviço diesel Drive-E — VolvoHowTo: <https://www.volvohowto.com/>
- Intervalos da correia dentada por motor — Autobaak: <https://autobaak.nl/en/volvo-v40/>
- VIDA (fluido de freio a cada 2 anos) — Volvo V40 Club: <https://www.volvov40club.com/>
- Intervalos de serviço por ano/motor — Auto-ABC: <https://www.auto-abc.eu/volvo-v40/>
- Lineup BR: <https://www.noticiasautomotivas.com.br/volvo-v40/>

> Nota: `volvocars.com` e os agregadores responderam **403** ao acesso automatizado neste ambiente;
> os números acima foram consolidados a partir dos resumos de busca dessas fontes (que reproduzem o
> manual/VIDA), sem raspagem de conteúdo.
