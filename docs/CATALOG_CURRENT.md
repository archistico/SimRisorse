> **Nota di lettura 03/11/2026:** il catalogo `civil-growth-13` è ora **validato su Windows tramite v59-RC1** secondo l'utente. Le intestazioni storiche v58-RC2 non sono più il gate attivo. **16 risorse, 20 edifici-obiettivo presenti e Municipio extra; Cantiere e Castello non esistono ancora.** [Stato autorevole](CURRENT_STATE.md) e [progetto G1](design/G1_NAVAL_MISSIONS_DECISION_GATE.md).

> **Aggiornamento 03/11/2026 · v58-RC2 (non validata):** questo documento contiene materiale del precedente checkpoint v57 e alcune parti sono storiche. Per lo stato attuale del ramo unificato leggere [docs/CURRENT_STATE.md](CURRENT_STATE.md), [docs/ROADMAP.md](ROADMAP.md) e [docs/validation/V58_RECONCILIATION.md](validation/V58_RECONCILIATION.md). Nuovo ruleset candidato: `civil-growth-13` (addestramento individuale); flotta non implementata.

# Catalogo attivo — `civil-growth-13` (candidato non validato)

**Catalogo del ramo v58-RC2, 03/11/2026; v13 non compilata/verificata.** Elenco dei **nomi effettivamente presenti nel codice** `src/SimRisorse.Application/Scenarios/CivilGrowthScenario.cs` e `EconomyFoundationScenario.cs`. Serve a evitare errori di nomenclatura e conteggio nel nuovo sviluppo. Non è un nuovo contratto di gioco.

## Risorse: 16

| ID tecnico | Nome nell'interfaccia | Nota |
| --- | --- | --- |
| `logs` | Tronchi | Materia prima del legno |
| `planks` | **Legno lavorato** | Chiamato spesso «Tavole» nei documenti di design; è **la stessa** risorsa |
| `stone` | Pietra | Materia prima |
| `fish` | Pesce | Input del Deposito alimentare |
| `food` | Cibo | Razione ordinaria |
| `grain` | Grano | Input del Mulino |
| `water` | Acqua | Input del Forno |
| `flour` | Farina | Output del Mulino |
| `bread` | Pane | Razione avanzata; stessa logica di capacità delle altre risorse |
| `iron-ore` | Minerale di ferro | Estrazione |
| `coal` | Carbone | Input condiviso fra fonderie/Forgia/Zecca |
| `iron` | Ferro | Output della Fonderia di ferro |
| `gold-ore` | Minerale d'oro | Estrazione |
| `gold` | Oro | Output della Fonderia d'oro |
| `swords` | Spade | Prodotto della Forgia, consumo all'arruolamento |
| `coins` | Monete | Prodotto della Zecca, usi navali soltanto proposti |

## Strutture: 21

| ID tecnico | Nome nell'interfaccia | Stato |
| --- | --- | --- |
| `tree-feller` | Taglialegna | Obiettivo della campagna; iniziale |
| `sawmill` | Segheria | Obiettivo della campagna; iniziale |
| `stonecutter` | **Tagliapietra** | Obiettivo della campagna; iniziale; talvolta indicato genericamente come «Cava» nei documenti |
| `small-house` | Casa piccola | Obiettivo della campagna; iniziale |
| `large-house` | Casa grande | Obiettivo della campagna |
| `warehouse` | Magazzino | Obiettivo della campagna; +500 a tutte le risorse per esemplare |
| `fisher` | Pescatore | Obiettivo della campagna |
| `food-store` | Deposito alimentare | **Produce Cibo** da Pesce; aggiunge +50 capacità Cibo, non è un deposito per ogni risorsa |
| `farm` | Fattoria | Obiettivo della campagna |
| `well` | Pozzo | Obiettivo della campagna |
| `mill` | Mulino | Obiettivo della campagna |
| `bakery` | Forno | **Produce Pane**, aggiunge +50 capacità Pane |
| `iron-mine` | Miniera di ferro | Obiettivo della campagna |
| `coal-mine` | Miniera di carbone | Obiettivo della campagna |
| `iron-smelter` | Fonderia di ferro | Obiettivo della campagna |
| `gold-mine` | Miniera d'oro | Obiettivo della campagna |
| `gold-smelter` | Fonderia d'oro | Obiettivo della campagna |
| `forge` | Forgia | Obiettivo della campagna |
| `mint` | Zecca | Obiettivo della campagna |
| `barracks` | Caserma | Obiettivo della campagna, 1 posto/30 s, 5 Spade per recluta |
| `town-hall` | **Municipio** | **Extra**, fisso, uno iniziale e non costruibile; **non** uno dei 22 obiettivi |

## Non ancora nel catalogo

- **Cantiere navale:** uno dei due edifici-obiettivo mancanti; i costi e le navi appartengono alla [proposta M5.4](design/M5_4_DECISION_GATE_V55.md) non approvata.
- **Castello:** ultimo edificio-obiettivo; richiederà almeno una conquista, anch'essa non implementata.

Di conseguenza: **20 obiettivi implementati + 2 mancanti = 22 obiettivi**; **21 strutture a catalogo oggi = 20 obiettivi presenti + 1 Municipio extra**. Dopo entrambi gli incrementi il catalogo conterrebbe **23 strutture**, ma la vittoria continuerebbe a richiedere **22 tipi della campagna**.

Per capacità numeriche, scorte iniziali e test vedere [CURRENT_STATE.md](CURRENT_STATE.md), [HANDOFF.md](HANDOFF.md) e [V55_STATIC_AUDIT.md](validation/V55_STATIC_AUDIT.md).
