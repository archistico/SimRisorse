# G1-D28 / RC12 — Due decimali ricerca e pulsante Acquista coerente

**Baseline:** RC11/G1-D27; il 03/11/2026 l'utente ha confermato il superamento dei test (466 attesi, conteggio effettivo non trasmesso). **RC12:** 468 test attesi, build/test Windows non eseguiti qui (SDK .NET 10 assente).

## Difetto e causa

Nella sezione Albero tecnologico il saldo ricerca era reso con tre decimali (`N3`) e i prezzi della ricerca senza decimali (`N0`). Inoltre `BuyTechnologyCommand.CanExecute` verificava soltanto che `SelectedTechnology` non fosse `null`: il pulsante verde poteva apparire abilitato anche con punti insufficienti, tecnologia già acquistata, prerequisiti o edifici mancanti, o tecnologia non implementata. L'API di acquisto core rifiutava queste richieste, ma **la GUI suggeriva erroneamente che l'acquisto fosse possibile**.

## Intervento

- Saldo, tasso punti al secondo e prezzi tecnologie mostrano due decimali italiani (`N2`: `0,00 punti`, `100,00 punti`, `0,00 punti/s`). La conversione resta solo presentazionale.
- Ogni riga calcola `CanPurchase` contemporaneamente al proprio stato descrittivo (`Acquistata`, `Non disponibile`, `Richiede ...`, `Punti insufficienti`, `Acquistabile`). Per il saldo la verifica usa gli esatti `ResearchBalanceMilliPoints` e `tech.Cost.MilliPoints`.
- Il ViewModel espone `CanBuySelectedTechnology` e lo notifica sia alla selezione sia dopo simulazione/acquisto/caricamento. `BuyTechnologyCommand.CanExecute` e `V14BuyTechnology.IsEnabled` usano la stessa proprietà.
- Classe visiva `primary research`: grigio disabilitato `#B8C2BC`, verde attivo `#176858`, hover ocra preesistente e bordo di spessore invariato. Il dettaglio selezionato resta visibile anche quando non acquistabile.
- Non cambia l'algoritmo di acquisto del Core (precisione interna 1/1000), le regole della ricerca, i costi, il formato SQLite e i salvataggi.

## Caso limite di arrotondamento

Con **99,999** punti effettivi e tecnologia da **100,000**, entrambi vengono mostrati a due cifre come **100,00**. L'acquisto deve rimanere grigio e lo stato deve recitare **«Punti insufficienti»**, perché il calcolo non arrotonda prima del confronto. A **100,000** effettivi il pulsante diventa verde e può essere cliccato; dopo l'acquisto è nuovamente disabilitato. Non sostituire il confronto in millipunti con confronti di stringhe o valori visualizzati.

## File coinvolti

`src/SimRisorse.Desktop/Presentation/V14PreviewViewModel.cs`, `src/SimRisorse.Desktop/Views/V14PreviewWindow.axaml`, `src/SimRisorse.Desktop/App.axaml` (solo nuovo selettore `Button.primary.research`), due file di test Desktop e documentazione. Nessuna modifica a `MainWindow.axaml`, alla campagna v13, ai Core/Application/Infrastructure produttivi, alla Scuola/Matite o ai database.

[ADR 0037](../adr/0037-v14-ricerca-precisione-e-stati-acquisto.md) · [Piano RC12](../validation/G1_D28_RC12_TEST_PLAN.md) · [Roadmap](../ROADMAP.md).
