# F1-D05e.2b.2b.2a / RC66 — Contratto del validatore v15

## Stato

RC66 è un incremento del disaccoppiamento. `V15FoodCampaignCheckpoints.Validate` chiama `V15CityCampaignValidator.Validate`, **non** più `V14CampaignSnapshots.Validate` e **non** più `V15LegacyProducerAdapter.ForV14Rules`.

## Dati verificati

1. Identificatori, versione DTO ereditata e integrità dei record: dati civici completi, tempi non negativi, crescita frazionaria.
2. **15 famiglie attive esatte** del catalogo v15 senza doppioni, con tier validi, assegnazioni nei posti e sblocchi ricerca per Grande/Avanzato.
3. Eventuale sedicesima famiglia storica `food-store` solo per importazioni non ancora ritirate: conteggio e personale sono validati e concorrono al budget, ma la sua capienza e ricetta non sono usate dal sistema v15. Lo store generation 6 rifiuta tale salvataggio finché occupata.
4. Edifici civili completi, Municipio fisso e invariato, eventuale Scuola e relativi contatori non retrocessi; il contatore storico ritirato può sopravvivere come dato **non produttivo**.
5. Controllo militare e Scuola, personale delle 15 famiglie e (se presente) importazione occupata, rispetto della popolazione totale. La disponibilità per le 6 nuove filiere viene poi controllata nel checkpoint completo.
6. Capacità cittadina tramite `V15CityStorage.Calculate`, senza contabilizzare l'edificio dismesso o confondere Deposito e città. Validazione risorse locali, esclusività Cibo e prodotti intermedi nel livello checkpoint.

## Confini non toccati

- `V15FoodCampaignCheckpoint.CityCampaign` conserva la classe DTO `V14CampaignSnapshot` per evitare una migrazione contemporanea di più consumer. Il validatore non dipende più dalle regole v14.
- `V15UnifiedCampaignClock` conserva uno slot artificiale **locale** solo per il `ProductionPlanner` del vecchio catalogo. Non viene inserito nel checkpoint o nel database.
- Contatori storici e formato SQLite generation 6 invariati.
- GUI e salvataggio personale v14 invariati (vincolo approvato RC63).

## Prossima sequenza

Validare RC64–RC66 insieme su Windows; poi RC67: introdurre DTO/checkpoint civico v15 autonomo e rimuovere il contatore `food-store` senza perdita silenziosa. In seguito adattatore del clock al catalogo senza famiglia ritirata, poi benchmark E4a ed economia E4b. Evitare modifiche di UX senza richiesta.

[ADR 0074](../adr/0074-validatore-civico-v15-senza-proiezione-v14.md) · [Piano RC66](../validation/F1_D05E2B2B2A_RC66_TEST_PLAN.md).
