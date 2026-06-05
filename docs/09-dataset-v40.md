# Dataset Curado — Volvo V40 (2012–2019)

Escopo inicial definido pelo stakeholder (Q6): **apenas o Volvo V40, geração II (plataforma P1),
anos 2012–2019**, com todas as versões, motores e extras necessários para o sistema funcionar.
Profundidade num único modelo antes de ampliar.

- **Arquivo:** [`src/ALGarage.Infrastructure/Seed/Data/volvo-v40-2012-2019.json`](../src/ALGarage.Infrastructure/Seed/Data/volvo-v40-2012-2019.json)
- **Loader (código real):** [`Seed/CuratedDatasetLoader.cs`](../src/ALGarage.Infrastructure/Seed/CuratedDatasetLoader.cs)
- **Origem dos dados:** `curated` (curado manualmente) — ver disclaimer abaixo.

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

## Como usar (quando o banco existir)

```csharp
var dataset = CuratedDatasetLoader.LoadV40();
var summary = CuratedDatasetLoader.Summarize(dataset); // contagens p/ sanity check
// TODO(feature): mapear para entidades e fazer upsert idempotente no seed do banco.
```

O loader é executável agora (lê o JSON embutido). A **persistência** (migrations + upsert) é tarefa
da Fase 1 — o esqueleto não cria o banco ainda.

## ⚠️ Disclaimer de qualidade

- Dados **curados**, bons para alimentar a estimativa, mas que **devem ser validados contra o manual
  oficial Volvo** de cada versão antes de uso definitivo.
- **Intervalos de manutenção são conservadores** para uso no Brasil (condição severa). Ex.: óleo a
  cada **10.000 km / 12 meses** (a Volvo BR oferece revisão de 10 mil km), o que vier primeiro —
  mesmo que o manual europeu cite intervalos maiores.
- **Códigos de opcionais (`FactoryOption.code`) são sintéticos/curados**, não os códigos internos da
  Volvo.
- Pontos a confirmar com a equipe / manual: intervalo exato da **correia dentada** por motor; tipo
  de **fluido da transmissão** (Aisin TG-81SC); grau de óleo do **2.5 cinco cilindros**; lineup
  **BR pré-2015** (nomes Comfort/Dynamic e o 2.0 cinco cilindros 180cv).

## Fontes consultadas

- Volvo V40 (2012–2019) — Wikipedia: <https://en.wikipedia.org/wiki/Volvo_V40_(2012%E2%80%932019)>
- Volvo Support BR (V40): <https://www.volvocars.com/br/support/car/v40/>
- Notícias Automotivas (lineup BR): <https://www.noticiasautomotivas.com.br/volvo-v40/>
- Óleo/capacidade: <https://carrorac.com/oleo-motor/volvo/v40.html>
- Intervalos de manutenção: <https://www.auto-abc.eu/volvo-v40/>
