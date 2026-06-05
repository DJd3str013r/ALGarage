# Fontes de Dados — VIN, Peças, Specs (com Plano B)

Este é o **maior risco do projeto**. O brief assume que existe uma fonte que decodifica VIN e
devolve peças/códigos/opcionais/compatibilidade. **Para a Volvo e para o mercado brasileiro, essa
fonte única não existe.** Abaixo, o que existe de verdade e como nos viramos.

## 1. Decodificação de VIN

| Fonte | Custo | Cobertura | Limitações |
|---|---|---|---|
| **NHTSA vPIC** ([vpic.nhtsa.dot.gov/api](https://vpic.nhtsa.dot.gov/api/)) | **Grátis**, sem chave | Veículos vendidos nos **EUA**; Volvo bem coberta de ~1995+ | Centrado no mercado americano; **não tem peças**; trims/opcionais BR podem divergir |
| **APIs comerciais** (ex.: [vehicledatabases.com](https://vehicledatabases.com/volvo-api), [17vin](https://en.17vin.com/brand/volvo.html), RapidAPI) | **Pago** (planos/limites) | VIN + specs, às vezes catálogo OEM | Cobertura BR **incerta**; custo por chamada; ToS |
| **APIs BR por placa/FIPE** ([apiplacas](https://apiplacas.com.br/), [fipeapi](https://fipeapi.com.br/), [PlacaAPI](https://www.placaapi.com/)) | Freemium → pago | Emplacamento BR, marca/modelo/ano, **valor FIPE** | **Sem peças**, sem opcionais de fábrica; é por **placa**, não por VIN |

**Estratégia (chain of responsibility no `IVinDecoder`):**

```
decode(vin):
  1) vPIC (grátis) ........... decode básico: WMI, modelo, ano, planta
  2) Provider comercial ...... (feature-flag) enriquece specs, se contratado
  3) Dataset Volvo curado .... fonte de verdade p/ modelos do MVP + FALLBACK garantido
  → mescla resultados; marca a origem de cada campo (proveniência)
```

> **Por que o dataset curado é inegociável:** é a única fonte que **controlamos** e que **não cai**,
> não muda preço, nem nos bane. Para o público-alvo (entusiasta Volvo BR), profundidade em poucos
> modelos vale mais que amplitude rasa.

## 2. Catálogo de peças e **códigos OEM**

**Não há API pública oficial da Volvo** para part numbers/compatibilidade. Opções:

- **Comercial** (17vin, 7zap-like): catálogo OEM por VIN. Pago, cobertura/ToS variáveis.
- **Curado**: inserimos os part numbers dos itens de manutenção dos modelos do MVP. Trabalhoso, mas
  é o que sustenta o "código de peça" com confiabilidade.
- **Comunidade** (futuro): contribuição moderada.

**Importante (já levantado na discussão):** o brief mistura **"código de peça"** (dado de catálogo,
precisa de fonte) com **"link de busca de peça"** (só uma URL). O **buscador do MVP é a segunda
coisa** e **não depende** de termos o catálogo OEM.

## 3. Buscador de peças (os "links")

**Decisão: API de afiliados + deep-links. Sem scraping.** ([ADR-0007](adr/0007-parts-finder-affiliate-links-no-scraping.md))

| Abordagem | Legal/ToS | Estabilidade | Receita | Veredito |
|---|---|---|---|---|
| **Deep-link de busca** (montar URL com a query) | ✅ ok | ✅ alta | — | **Base do MVP** |
| **API de afiliados** (ex.: [Mercado Livre Developers](https://developers.mercadolivre.com.br/)) | ✅ ok (programa oficial) | ✅ alta | 💰 comissão | **Enriquecimento (Fase 2)** |
| **Scraping de resultados** | ❌ viola ToS, risco jurídico | ❌ quebra a cada mudança de HTML | — | **Rejeitado** |

**Como manter links válidos:** deep-links de **busca** (não de produto) são estáveis — apontam para
a página de resultados da query, não para um SKU que pode sair de estoque. Onde usarmos API de
afiliado, validamos disponibilidade na hora. Cada loja é um `IPartsSearchLinkProvider`; adicionar
loja = nova implementação, sem tocar no resto.

## 4. Especificações e **opcionais de fábrica**

- **vPIC** dá specs básicas (motor, carroceria, etc.) para o mercado US.
- **Opcionais de fábrica detalhados** (pacotes, acabamentos) raramente vêm de API → majoritariamente
  **curados** e guardados em `ModelVariant.SpecsJson`/`FactoryOption` (JSONB).
- Aqui o **dataset curado** volta a ser a espinha dorsal.

## 5. Plano B consolidado (sem nenhuma fonte oficial)

Se **zero** APIs pagas forem aprovadas e o vPIC for insuficiente para BR:

1. **vPIC** para o decode básico (grátis).
2. **Dataset curado próprio** (seed) para specs/opcionais/planos de manutenção/part numbers dos
   **modelos do MVP**.
3. **Buscador de peças por deep-link** (não precisa de catálogo).
4. Crescimento via **curadoria incremental** e, no futuro, **comunidade**.

Ou seja: **o produto funciona mesmo no pior cenário de dados**, apenas com menos modelos cobertos.
Essa é a razão de toda a arquitetura de _providers_ + dataset curado.

## 6. Proveniência e confiança

Cada campo de spec carrega sua **origem** (vPIC / comercial / curado / usuário). Isso permite:
- mostrar ao usuário a confiança do dado;
- preferir a fonte mais confiável em conflitos;
- auditar/limpar quando uma fonte se provar ruim.

---

### Fontes citadas

- NHTSA vPIC — <https://vpic.nhtsa.dot.gov/api/>
- Vehicle Databases (Volvo API) — <https://vehicledatabases.com/volvo-api>
- 17vin (Volvo VIN + parts) — <https://en.17vin.com/brand/volvo.html>
- API Placas (BR) — <https://apiplacas.com.br/>
- FIPE API (BR) — <https://fipeapi.com.br/>
- Mercado Livre Developers — <https://developers.mercadolivre.com.br/>
