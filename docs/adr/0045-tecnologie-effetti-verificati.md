# ADR 0045 — La ricerca acquistabile deve sbloccare un effetto reale

- Data: 2026-11-04
- Milestone: P1-D48 / RC34
- Stato: implementata, validazione Windows pendente

## Contesto

`TechnologyCatalog` descrive ricerche e famiglie sbloccate; `TieredProductionCatalog` determina realmente i livelli produttivi. Mantenere due rappresentazioni indipendenti senza confronto può consentire acquisti di tecnologie con un beneficio dichiarato ma inesistente.

## Decisione

Introdurre `TechnologyTierEffects`, che legge le regole concrete Base/Grande/Avanzato e verifica il catalogo quando viene creata/ripristinata una sessione v14. Ogni tecnologia disponibile deve avere almeno un beneficio di tier, gli ID famiglia dichiarati devono coincidere con la proiezione effettiva, e la ricerca Avanzato deve dipendere (anche transitivamente) dalla ricerca Grande corrispondente. Tecnologie differite non devono essere richieste da alcun tier implementato.

Mostrare i benefici reali nel dettaglio dell'Albero tecnologico, distinguendo Grande e Avanzato. Nessuna ricerca viene spesa automaticamente; Logistica e Navigazione restano visibili ma non acquistabili.

## Alternative escluse

- Fidarsi del solo campo `UnlockedFamilies`: non verifica l'effetto effettivo.
- Rendere acquistabili i placeholder a fronte di futuri bonus: contrario al criterio P1-D48.
- Cambiare ora bonus/costi: rinviato alle misurazioni E4a e approvazione E4b.

## Compatibilità e verifica

Non cambiano schema SQLite, salvataggi, coefficienti, v13, ricette o tempi. Il gate riguarda la coerenza delle regole al caricamento del catalogo. Aggiunti 9 casi xUnit, **533 complessivi attesi**, da validare su Windows.

[Rapporto](../design/P1_D48_RC34_ALBERO_TECNOLOGICO.md) · [Test](../validation/P1_D48_RC34_TEST_PLAN.md)
