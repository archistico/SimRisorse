# Piano di validazione — RC12 / G1-D28

**Ultimo checkpoint confermato:** RC11: tutti i test dichiarati passati dall'utente, 466 attesi (numero effettivo non comunicato). **RC12 candidata:** +2 nuovi casi Desktop, **468 test attesi**. Compilazione e xUnit non eseguibili in questo ambiente senza SDK .NET 10.

## Gate automatico Windows

1. Estrarre il pacchetto RC12 completo in una cartella nuova ed eseguire `./validate.cmd`. Conservare log e `.trx`; verificare **468/468** e nessun errore di compilazione.
2. `TechnologyCostDisplayHasTwoDecimalsButPurchaseUsesExactMilliPoints`: costo `100,00`, saldo e tasso a due cifre; 99,999 millipunti/unità visualizzati come 100,00 ma acquisto impossibile; a 100,000 esatti acquistabile; dopo acquisto di nuovo disabilitato. Rifiuti per tecnologia non implementata e prerequisiti mancanti.
3. `ResearchPurchaseButtonIsGreyUntilPointsAreAvailableAndNeverMovesOnHover` (Avalonia Headless): comando legato al bottone, `IsEnabled` **e** `IsEffectivelyEnabled` falsi quando mancano punti, sfondo grigio `#B8C2BC` anche in hover; verdi con saldo sufficiente; hover ocra `#E6D5AC` senza movimento; di nuovo disabilitato dopo acquisto.
4. Regression suite esistente: Scuola/Matite, costruzioni e lavoratori +1/+5/+10, Pane, Risorse a sette colonne, Esercito, migrazione e SQLite v13/v14.

## Verifica visiva manuale

- Avviare `./run-game.cmd --experimental-v14`; Albero tecnologico: punti a due decimali nel box, nella barra laterale e nel prezzo di ogni ricerca.
- Senza punti selezionare «Lavorazioni I»: pulsante grigio, descrizione «Punti insufficienti», niente acquisto al clic. Selezionare Logistica: non acquistabile anche con punti.
- Con punti effettivi sufficienti e prerequisiti soddisfatti: pulsante verde con hover ocra, nessuno spostamento; dopo acquisto torna grigio e la ricerca risulta acquistata.
- Ripetere alle dimensioni **1040×740** e **1240×850**. Confermare leggibilità e assenza di alterazioni nella GUI normale v13.

**P0 di parità grafica v13/v14 resta aperto anche con suite verde.** Il bilanciamento E4 non parte sulla sola base di questi test.

[Report RC12](../design/G1_D28_RC12_ALBERO_TECNOLOGICO_PUNTI_E_ACQUISTO.md) · [ADR 0037](../adr/0037-v14-ricerca-precisione-e-stati-acquisto.md).
