# 0004 — Popolazione e progressione militare come modelli espliciti

- Stato: Accettato
- Data: 2026-10-26
- Ambito: confini del dominio
- Origine: lavoratori e successiva progressione militare confermati dall'utente

## Contesto

Il gioco prevede assegnazioni di lavoratori, soldati, castelli e missioni. Nella bozza persone e soldati sono quantità nella stessa struttura delle materie prime. Gli screenshot presentano edifici, missioni e navi in elenchi simili, ma questo non implica che abbiano lo stesso comportamento.

## Decisione

Rappresentare esplicitamente persone e assegnazioni, distinguendo capacità abitativa, popolazione presente e posti di lavoro. Le persone assegnabili e le unità militari sono conteggi interi; un eventuale accumulo di crescita frazionaria non rende assegnabile una frazione di persona.

Distinguere edifici, contingenti, mezzi e missioni quando entrano nell'implementazione. Riutilizzare concetti comuni come costi e requisiti senza imporre una classe base universale.

Non vengono decisi in questo ADR un bacino condiviso civili/soldati, formule di crescita, cibo, congedi, perdite o esiti militari. Sono proposte e questioni aperte nel game design. La prima implementazione può usare una popolazione iniziale fissa.

## Alternative

- Trattare le persone esclusivamente come una risorsa scalare: rende più difficile impedire doppie assegnazioni e spiegare gli stati.
- Un modello unico `Costruzione` per qualunque voce dell'interfaccia: confonde ciclo di vita delle strutture e delle missioni.
- Implementare subito una simulazione militare completa: anticipa decisioni di gioco ancora aperte.

## Conseguenze

Il Core deve verificare disponibilità e assegnazioni. L'introduzione dei soldati non dovrà obbligare a riscrivere il modello economico, ma le regole di trasferimento tra stati richiedono una specifica prima del reclutamento.

## Verifica

Impedire assegnazioni superiori alle persone disponibili o ai posti di lavoro. Quando si implementa un trasferimento tra categorie, verificare che non crei o duplichi persone. Nascite, perdite e altri cambiamenti del totale devono essere eventi espliciti previsti dalle regole.

## Collegamenti

- [Game design](../GAME_DESIGN.md).
- [Architettura](../ARCHITECTURE.md).
