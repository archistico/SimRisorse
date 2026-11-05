# RC56 — Piano validazione F1-D05d.4b.2

## Stato

**RC55 convalidata dall'utente: 713 test passati. RC56 candidata con 18 nuovi casi, 731 attesi: .NET 10 non eseguito in confezionamento.**

## Comandi Windows

Nella radice della versione estratta in directory pulita:

```powershell
.\validate.cmd

dotnet test tests\SimRisorse.Application.Tests\SimRisorse.Application.Tests.csproj -c Release --filter FullyQualifiedName~F1D05d4b2EconomyProjectionTests

dotnet test tests\SimRisorse.Desktop.Tests\SimRisorse.Desktop.Tests.csproj -c Release --filter FullyQualifiedName~V15ResourceFlowDisplayTests

.\run-game.cmd --v15 --save-path "$env:TEMP\SimRisorse-rc56-prova.sqlite3"
```

## Casi automatizzati

1. Pescatore + Deposito con addetto: Pesce +1/−1, Cibo +1/−0,080 nel secondo con 8 abitanti; saldo Cibo +0,920.
2. Deposito senza personale: ricevimento Pesce, senza attribuire un secondo consumo o una produzione fittizia.
3. Vecchio `food-store`: nessun consumo Pesce né produzione Cibo nella v15.
4. Cibo pronto saturo: processo bloccato *prima* del pasto, ma pasto ancora consumato se disponibile.
5. Razione avanzata atomica: senza Pane, non consumare Cibo anche se disponibile.
6. Stalla/Cacciatore/Orto/Deposito: risorse generate e usate senza contare come flusso la consegna al deposito.
7. Conservazione al millesimo per **tutte** le risorse mostrate: `stock dopo = stock prima + flusso prodotto − flusso consumato`.
8. Proiezione con frazioni di secondo, senza mutare la sessione/clock attivo.
9. Capienze con owner, somma esatta dei compartimenti, Cibo unicamente nel Deposito.
10. GUI: sette colonne, valori italiani e saldo con segno, quantità arrotondata, six scenari ETA (saturazione, netto ≤ 0, zero capienza, ore ≥ 24), riga stabile dopo refresh.

## Controlli visuali non coperti dai test automatici

- `run-game.cmd` senza flag: scheda Risorse v14 invariata; mouse/scrollbar, selezione ocra, colonna ETA senza sovrapposizioni.
- `--v15`: lista alfabetica, stessa tabella a 7 colonne, tutte le celle numeriche allineate a destra. Hover capacità: mostra città/recinti/transito/deposito con somma verificabile.
- Costruire Pescatore e Deposito, assegnare 1 addetto: confermare aumento Cibo, consumo Pesce, decremento per razione e differenza netta; mettere in pausa e confrontare valori (non devono variare con il tempo fermo).
- Rimuovere addetto dal Deposito: Cibo prodotto deve diventare zero se nessun altro addetto; non devono apparire valori inventati.
- Saturazione capienza, scorte esaurite, saldi zero/negativi, allineamento e nessun salto dei controlli all'hover.
- Salvare/caricare v15 nel percorso temporaneo; confrontare scorte; controllare che il file v14 personale non venga toccato.

## Gate successivo

Se `validate.cmd` è verde, RC56 diventa base per **F1-D05e**: stress ore/giorni simulati, riconciliazione del clock reale a lungo termine, pulizia legacy e audit approfondito di GUI/SQLite. Non cambiare formule E4b prima di E4a.
