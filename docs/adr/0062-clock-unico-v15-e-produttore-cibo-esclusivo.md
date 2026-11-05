# ADR 0062 — Un solo clock v15, Cibo prodotto solo dal nuovo Deposito

**5 novembre 2026 · Accettata per implementazione RC51, test .NET ancora pendenti**

## Problema

Il catalogo storico v14 contiene `food-store` (Pesce → Cibo) e la RC50 aggiunge un diverso `food-depot` (cinque ingredienti → Cibo). Eseguire entrambi i clock oppure entrambi i produttori nel medesimo secondo causerebbe doppia produzione e potrebbe consumare contemporaneamente un medesimo lotto. La popolazione v14 consuma il Cibo dalla città mentre la v15 lo conserva nel Deposito, in forma incompatibile.

## Decisione

- `V15UnifiedCampaignClock` usa **un solo ciclo transazionale**: produzioni storiche escluse dal vecchio `food-store`, nuovo ciclo delle filiere, razioni atomiche da Deposito, crescita, progressi scuola/militari, validazione e commit unico.
- `ProductionPlanner` accetta una esclusione opzionale; tutti i call site v14 rimangono identici. Ogni nuovo produttore non autorizzato di Cibo nel catalogo viene rifiutato prima dell'anticipo.
- Il Cibo pronto è proprietà esclusiva del Deposito; il Pane grezzo è custodito nei suoi compartimenti e protetto per le abitazioni avanzate. Non si sottrae Cibo alla città una seconda volta.
- Sessioni v15 nuove o ripristinate partono in pausa; secondi parziali, progressi militari e ricerca sono conservati; `Save/Load` non cambia lo schema SQLite 5.
- `food-store` resta come **voce storica inattiva nel ramo v15**, soltanto finché non viene completata la ricomposizione del catalogo. È vietato collegarlo a una UI v15 come produzione attiva. `StepFood` non è un'azione da attivare nell'orologio giocabile.
- La compatibilità dei vecchi salvataggi sperimentali può essere interrotta per semplificare la nuova architettura, senza toccare i file reali dell'utente.

## Alternative scartate

Lanciare un tick v14 seguito da un tick del Deposito v15: il primo ha già eseguito la trasformazione Pesce → Cibo e la deduzione delle razioni. Conservare entrambi i contatori del Cibo senza conversione: crea due proprietari e rende impossibile una contabilità affidabile. Modificare il clock v14 in produzione: altera inutilmente la GUI storica prima del completamento v15.

## Conseguenze

RC51 è una versione headless con clock unificato, **non** una GUI v15. Rimangono l'unificazione del catalogo lato comandi e tecnologie, l'adeguamento dei comandi di iscrizione Scuola ai nuovi stock, la GUI e i test completi su Windows. Nessuna modifica al bilanciamento E4b.

[Specifica](../design/F1_D05D3B_RC51_CLOCK_UNIFICATO.md) · [Test](../validation/F1_D05D3B_RC51_TEST_PLAN.md).
