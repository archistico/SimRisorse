# RC49 / F1-D05d.2 — Checkpoint integrale delle nuove filiere alimentari

**5 novembre 2026 — implementazione candidata, test .NET 10 NON eseguiti.**

## Decisione di sviluppo: nessuna compatibilità obbligatoria con i vecchi database

Durante lo sviluppo del gioco è stata **esplicitamente rimossa la necessità di importare ogni vecchia partita**. Possiamo evolvere, riscrivere o eliminare lo schema di salvataggio in presenza di modifiche strutturali. La documentazione indica il cambiamento incompatibile, e i vecchi file non vengono reinterpretati né sovrascritti per errore. La protezione dei file già esistenti è una buona pratica, **non** un motivo per bloccare l'evoluzione dei modelli.

In RC49 `V15SqliteFoodSaveStore` adotta `PRAGMA user_version=5` e `game_save.snapshot_version=5`; conserva temporaneamente `rules_version=civil-growth-15`, perché il nuovo ruleset non è ancora attivo. **I file sperimentali RC46–RC48 con `user_version=4` non sono caricabili da questo store.** Per i test ricreare il database di prova. Il salvataggio **giocabile v14** rimane su un percorso separato e non viene modificato da RC49.

Il nome C# `V15...` indica ancora il ramo di sviluppo del ruleset, non una promessa di compatibilità binaria fra schemi; converrà rinominare i componenti quando il nuovo runtime sarà unico.

## Nuovo contratto di snapshot

`V15FoodCampaignCheckpoint` contiene tre proprietà obbligatorie:

1. `CityCampaign`: stato storico non alimentare, con risorse, popolazione, industria, case, tecnologia, scuola, esercito, tempo e stato individuale.
2. `Depot`: inventario grezzo esclusivo `fish/bread/meat/fruit/vegetables` e Cibo pronto `food`.
3. `Industry`: `Animals` (`cattle`, `game`), `Transit` (`meat`, `fruit`, `vegetables`) e `Buildings` (conteggi e lavoratori delle sei strutture alimentari).

`FoodIndustrySites.Ids` definisce i sei edifici: `stable`, `hunter`, `butcher`, `garden`, `orchard`, `food-depot`. Il loro **conteggio** e i **lavoratori assegnati** vengono registrati; la capacità di posti è derivata dai conteggi (in questa tranche tecnica **1 posto per edificio base**, valore provvisorio). Tutte le assegnazioni sono verificate rispetto a posti reali e popolazione residua dopo industria preesistente, scuola e militari. Lo stesso tipo di risorsa può avere **quantità distinte** in transito e nel deposito, ma nessuna singola quantità può essere duplicata; la città non possiede gli intermedi.

Una scorta superiore alle capacità future è conservata integralmente: la persistenza non tronca automaticamente beni. Le capacità di produzione/consegna sono parametri di simulazione, **non** valori duplicati nel DB. La produzione è effettuata dal planner, non dal salvataggio.

## SQLite generation 5

Tre nuove tabelle `STRICT` e collegate a `game_save`:

| Tabella | Colonne | Regola |
| --- | --- | --- |
| `food_industry_site` | `slot`, `site_id`, `count`, `workers` | Esattamente le sei righe canoniche, conteggi e addetti non negativi; addetti entro posti derivati |
| `food_animals_stock` | `slot`, `resource_id`, `milliunits` | Solo `cattle`, `game`, valori al millesimo; righe zero omesse |
| `food_transit_stock` | `slot`, `resource_id`, `milliunits` | Solo `meat`, `fruit`, `vegetables`, valori al millesimo; righe zero omesse |

Le 13 tabelle precedenti rimangono, per un totale di **16**. `ReadFile` rifiuta tabelle extra o mancanti, righe mancanti o impreviste, risorse non canoniche, personale impossibile e file con versione incompatibile. `Save` verifica l'intero snapshot, scrive su file temporaneo, lo rilegge e lo promuove; se sovrascrive una precedente generazione 5 valida, ne produce prima una copia SQLite consistente. Non serve importare database di vecchie versioni per testare questa generazione.

## Ponte verso la simulazione

`V15FoodCampaignTicks.Plan` costruisce il personale dal `FoodIndustryBuildings` del checkpoint, verifica il budget di popolazione, richiama `FoodIndustryTickPlanner` e restituisce il **prossimo checkpoint completo**, compresi animali e scorte bloccate in transito. Non cambia il tempo di gioco: per il momento è un calcolo puro per verifiche di continuità. Un test verifica tick → salvataggio → ripresa → tick, confrontando con la simulazione senza interruzione.

**Ancora non giocabile:** non è stato introdotto il catalogo costruibile, la sostituzione del vecchio `food-store`, unificazione col clock v14, capacità derivate dagli edifici, costi, prerequisiti o GUI. Questi sono gli ingressi della successiva **F1-D05d.3** e D05d.4. I coefficienti dei test non sono bilanciamento approvato. Il costo della Zecca e il minimo 10 lavoratori/modulo rimangono in E4b.

[ADR 0060](../adr/0060-salvataggi-di-sviluppo-e-checkpoint-filiera.md) · [Piano test RC49](../validation/F1_D05D2_RC49_TEST_PLAN.md) · [Roadmap](../ROADMAP.md).
