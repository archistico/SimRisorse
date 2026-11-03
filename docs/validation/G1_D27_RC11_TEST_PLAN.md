# Piano di validazione — RC11 / G1-D27

**Baseline RC10:** test dichiarati passati dall'utente, 464 attesi (conteggio effettivo non comunicato). **RC11:** +2 casi automatici, **466 attesi**, nessuna compilazione o esecuzione xUnit in questo ambiente senza .NET 10.

## Test automatici

1. Estrarre ZIP RC11 in cartella nuova su Windows 11; eseguire `./validate.cmd` e conservare `.trx` e riepilogo (aspettativa **466 passati su 466**).
2. `ConstructionAndWorkerRowsHaveSeparateLabelsAndThreePositiveBatchButtons` a **1040×740** e **1240×850**: label di riga, pulsanti +1/+5/+10, bind di ciascun comando, righe separate, Costruisci nascosto per prerequisiti mancanti, `−1` ancora presente.
3. `WorkerBatchUsesAvailablePeopleAndWorkplacesWithoutPartialAssignments`: con sei Segherie base e cinque abitanti liberi, il batch `+5` riempie i posti in un solo commit; `+10` rimane disabilitato e non assegna niente; il rilascio `−1` continua a funzionare; i tasti si ricalcolano.
4. `MissingResourcesMakeBuildButtonVisiblyDisabledWithoutHidingTheDetails`: i tre lotti Costruisci non sono cliccabili senza risorse; testo del costo resta visibile; stile grigio.
5. `V14BuildHoverIsDistinctAndDoesNotMoveTheActions` a due dimensioni: hover ocra, geometria invariata delle **due righe**.
6. Rieffettuare l'intera suite della RC10: Scuola/Matite, salvataggi v14/v13, quote cumulative, upgrade, lavoro, tabella Risorse a 7 colonne, Esercito, diagnostica Pane.

## Verifiche visuali manuali P0

- `./run-game.cmd --experimental-v14` a **1040×740** e **1240×850**: la prima riga recita «Costruisci» e tre pulsanti `+1/+5/+10`; la seconda «Lavoratore» e tre pulsanti `+1/+5/+10`, con un piccolo `−1` accanto al testo. Nessuna etichetta tagliata o pulsante sovrapposto.
- Passare il mouse su ciascun Costruisci attivo: hover ocra; spostamenti assenti. Bloccare risorse: i pulsanti diventano grigi ma restano visibili (se la ricerca è sbloccata). Bloccare tecnologia: la riga Costruisci è nascosta, la riga dell'edificio resta selezionabile.
- Controllare quantitativi: le tre colonne della prima riga **costruiscono edifici**, le tre della seconda **assegnano lavoratori**. Senza abitanti liberi o senza postazioni, lotti 5/10 disabilitati; nessuna assegnazione parziale.
- Controllare quote progressive +5/+10, gestione −1, upgrade, pagina Risorse immutata, Scuola e Matite ancora utilizzabili.
- Confrontare la GUI v13 (`./run-game.cmd`) e il file SQLite v13: devono restare immutati. Nessun automatismo di migrazione.

**Gate:** test Windows + verifica visiva/manuale P0 ancora aperti; non attivare E4.

[Rapporto RC11](../design/G1_D27_RC11_LAYOUT_COSTRUZIONE_LAVORATORI.md) · [ADR](../adr/0036-v14-layout-costruzione-e-lotti-lavoratori.md) · [Roadmap](../ROADMAP.md).
