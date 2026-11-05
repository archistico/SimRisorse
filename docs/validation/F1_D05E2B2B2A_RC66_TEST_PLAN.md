# Piano test RC66 — Validatore civico v15 nativo

**Baseline Windows confermata:** RC63 771/771. RC64: +6 non validati. RC65: +7 non validati. RC66: +7 Application. **Totale RC66: 791 attesi, non eseguiti** (SDK .NET 10 assente).

```powershell
.\validate.cmd
.\run-game.cmd
.\run-game.cmd --v14
# eventuale stress combinato
 dotnet run --project tools/SimRisorse.Simulation -c Release --no-restore -- --audit-v15 --seconds 120 --profile combined
```

## Nuove regressioni (7)

1. Un checkpoint di 15 famiglie passa il validatore nativo mentre il validatore v14 originario lo rifiuta; nessuna mutazione dello stato.
2. Famiglia produttiva attiva mancante respinta.
3. Famiglia produttiva duplicata respinta.
4. Contatore acquisti retrocesso rispetto ai costruiti respinto.
5. Scorte cittadine oltre la capacità reale respinte.
6. Importazione temporanea di `food-store` occupato valida prima del ritiro esplicito, ma il ritiro libera la rappresentazione storica dal checkpoint.
7. Assegnazioni che superano la popolazione respinte.

## Regressioni obbligatorie

- Tutti i 784 test precedenti (compresi sqlite gen 6 e parità Desktop). Non cambiare il valore delle capacità, dei costi o delle ricette.
- Una nuova partita v15 mostra le sei filiere. Tutti gli edifici bloccati restano grigi e consultabili senza pulsante Costruisci attivo; dettagli con icone, due righe di pulsanti e barra crescita nella posizione RC63.
- SQLite v15 generation 6 resta separato da `%LOCALAPPDATA%\SimRisorse\experiments\civil-growth-v14.sqlite3`.
- Non affermare che RC66 abbia superato xUnit senza output Windows.
