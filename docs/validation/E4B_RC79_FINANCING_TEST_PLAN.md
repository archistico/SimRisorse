# RC79 / E4b.2a — Piano di verifica e limiti

**Stato al confezionamento: candidato NON compilato.** L'ambiente di preparazione non dispone di .NET 10, `dotnet`, PowerShell né di un compilatore C#. Non dichiarare 943 test passati: **926/926** è il risultato verificato sullo ZIP RC78 in ingresso, come riportato dai documenti consegnati e dalla conferma dell'utente. Sono stati aggiunti **17 nuovi casi xUnit**: 12 Core (compresi 4 dati del Theory) e 5 Application; totale cumulativo **943 atteso**, da verificare sul sistema Windows.

## Verifiche previste

- Core: deposito parziale/saldo, importi rifiutati senza mutazioni, rimborso con e senza spazio, importi al millesimo, capacità zero, casi molto grandi senza liste, parametri di rate invalidi.
- Application: 27 combinazioni curve/tier/lotti, confronto di costo totale sequenziale vs atomico, monotonicità delle soglie, riconciliazione fisica delle rate, output locale deterministico e indipendente dalla lingua.
- CLI: `--study-e4b-financing` deve produrre tre file aggiuntivi distinti, senza impatti sui cinque file e sulle evidenze dello studio RC78. Nessun test CLI automatico ulteriore conteggiato come xUnit; il gate F1 controllerà la CLI quando sarà eseguito.
- Nessun file Desktop o Infrastructure modificato; nessuna nuova generazione SQLite, nessun prezzo o costo di ricerca/squadra modificato.

### Comandi di verifica su Windows

```powershell
.\validate-f1.cmd
# contiene anche validate.cmd e richiede 943/943, smoke, audit e studi RC78/RC79

# opzionale: studio indipendente con 27 righe senza scomodare salvataggi
 dotnet run --project tools/SimRisorse.Simulation -c Release -- --study-e4b-financing --output .local-cache/e4b-v15/rc79-manual
```

Rivedere almeno i casi di Zecca Base A lotto 10 e B/C tier Avanzata; controllare il significato di *warehouse-payment-exceeds-capacity* e il numero delle rate, evitando di etichettare un percorso di Magazzini bloccato come stallo generale. Riprodurre l'output in una seconda directory e confrontare i tre file byte per byte, quindi verificare il rifiuto di directory già contenente gli output.

**Gate F1-D05e:** il confronto interattivo delle schermate v15 con la GUI v14 resta formalmente aperto anche se la suite diventa verde. Stress facoltativo 24h non eseguito. I DB sperimentali non richiedono migrazione, ma questa RC non ne cambia il formato.

[Revisione statica](RC79_STATIC_REVIEW.md) · [Specifica](../design/E4B_RC79_FINANCING_FEASIBILITY.md) · [ADR 0087](../adr/0087-studio-finanziamento-progressivo-senza-attivazione.md).
