# F1-D05e.2b.2b.1 — Contratto comandi nativi v15 / RC65

## Perimetro

- Proprietario della partita: `V15FoodIndustryCampaignSession`; il checkpoint è validato prima di ogni commit.
- `V15NativeCityCommands.cs` implementa direttamente acquisti civici, Tier Base/Grande/Avanzato, upgrade, personale, costruzione Scuola e preparazione Matite. `V15FoodIndustryCampaignSession.CityCommands.cs` esegue ricerca, formazione, esercito e interazione col Deposito.
- Politiche riutilizzate: `V14TierConstructionRules.Candidate` (fattore 1,15, moltiplicatori 2,6/6,5), `TieredConstruction`, `ProgressiveConstructionPricing`, `V14SchoolConstructionRules.Candidate` senza il vecchio prerequisito `food-store` (sostituito da `food-depot`), `V14SchoolSupplies`, `TechnologyPurchases`, `V15CityStorage`.
- Catalogo `TieredProductionCatalog` filtrato a 15 famiglie per sessione. `food-store` non è costruibile né possiede personale; l'edificio viene preservato solo in speciali snapshot ereditati non ritirati, finché non si richiede il ritiro esplicito.
- Nessun cambio a `V15SqliteFoodSaveStore` (generation 6), ai tempi del clock, ai ricettari, ai consumi, alla GUI o ai controlli di navigazione.

## Contratti d'errore e transazione

- `UnknownBuilding`/`UnknownFamily` per ID non noti e `food-store` ritirato; quantità e tier invalidi hanno gli stessi risultati precedenti.
- Costo progressivo quotato e applicato una sola volta; se non c'è copertura, la `ResourceBundle` proposta è mostrata ma nessun effetto viene committato.
- Personale: capienza del tier **prima** del budget globale; il budget usa `AvailableWorkers`, condiviso con le nuove filiere. Gli upgrade trasferiscono lavoratori esistenti, non li creano.
- `CraftPencils` verifica materiali e capacità della città (non usa la capacità del Deposito del Cibo pronto).
- Ricerca: ledger e prerequisiti puri, con riferimento anche agli edifici posseduti nelle nuove filiere; un doppio acquisto non scala nuovamente i punti.

## Residui da rimuovere

La struttura `V14CampaignSnapshot` nella città v15, il contatore `food-store` e la proiezione temporanea v14 usata dal validatore non vengono rimossi in RC65. L'adattatore del planner rimane locale, con produzione legacy disabilitata. Il successivo passaggio dovrà aggiornare anche SQLite e i test di coerenza; non è una modifica UI.
