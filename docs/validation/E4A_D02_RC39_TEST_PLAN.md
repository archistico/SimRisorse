# RC39 / E4a-D02 — Piano di validazione Windows

**Ultimo gate confermato:** RC38, 541/541 test passati. **Nuovo obiettivo RC39:** 545 test cumulativi (quattro nuovi), non eseguiti nel confezionamento.

## Gate obbligatori

1. Estrarre RC39 in una directory nuova; eseguire `./validate.cmd`. Confermare **545 PASS, zero FAIL, zero SKIP**.
2. Verificare `E4aNaturalBenchmarkTests` (partenza invariata 8/120/80/30, evento Pescatore/Deposito, primo Cibo reale, timeout senza successo inventato, serializzazione deterministica, argomenti invalidi).
3. Eseguire E4A-01 due volte con destinazioni diverse. Confrontare JSON e CSV escludendo percorsi esterni, che non vengono inclusi nel payload.
4. Eseguire separatamente E4A-02…E4A-05, oppure il comando unico. Verificare per ciascuno `outcome`, `stopReason`, eventi e campioni. **Non presupporre che tutti e cinque riescano:** eventuali stalli vanno registrati e analizzati.
5. Verificare la prevenzione della sovrascrittura ripetendo un comando sulla medesima destinazione; deve fallire e lasciare i risultati precedenti intatti.
6. Controllare che `%LOCALAPPDATA%\SimRisorse\experiments\civil-growth-v14.sqlite3` e lo slot v13 non vengano aperti o modificati dal benchmark.

```powershell
.\validate.cmd
dotnet run --project tools/SimRisorse.Simulation -c Release -- --benchmark-e4a-natural --scenario E4A-01 --output .local-cache/e4a/natural-01-a
dotnet run --project tools/SimRisorse.Simulation -c Release -- --benchmark-e4a-natural --scenario E4A-01 --output .local-cache/e4a/natural-01-b
dotnet run --project tools/SimRisorse.Simulation -c Release -- --benchmark-e4a-natural --output .local-cache/e4a/natural-all-a
```

**Interpretazione:** `pass` significa soltanto raggiungimento del target con la policy `natural-reference-v1`; `timeout` oppure `blocked` sono risultati diagnostici reali, non test xUnit falliti. Il comando restituisce codice 1 se almeno uno scenario non termina con `pass`, ma scrive comunque risultati JSON/CSV per l'analisi. Le prove non devono essere spacciate per valori già misurati.

[Rapporto tecnico](../design/E4A_D02_RC39_BENCHMARK_NATURALI.md) · [Roadmap](../ROADMAP.md)
