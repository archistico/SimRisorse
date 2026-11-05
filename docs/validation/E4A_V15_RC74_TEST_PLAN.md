# RC74 — Validazione dell'analizzatore E4a v15

**Esecuzione .NET 10:** non effettuata. **Test cumulativi previsti:** 843 (RC73 834 + 9 test RC74). Confermati dall'utente solo fino a RC63: 771/771. I test del postprocessore **non** dimostrano da soli la riuscita del gate F1 o la raggiungibilità degli edifici.

## Test introdotti

`E4aV15ProgressionAnalysisTests` (9):

1. Milestone osservata: completamento, durata, tempo d'attesa e materiali tracciati;
2. timeout non classificato come blocco strutturale;
3. diagnostica abbreviata distinta dai risultati a orizzonte pieno;
4. comando bloccato esplicitamente, senza durata fittizia;
5. più vincoli d'attesa consecutivi, senza sovrapposizione dei secondi;
6. scenari duplicati o lista vuota rifiutati;
7. completamento non sostenuto e log non ordinato rifiutati;
8. CSV/Markdown con classificazioni e limiti espliciti;
9. postprocessing di un risultato diagnostico reale RC73 a tempo zero.

## Procedura Windows

```powershell
.\validate-f1.cmd
# Atteso: 843/843, nessuno skipped, GUI smoke e audit F1 superati.

dotnet run --project tools/SimRisorse.Simulation -c Release -- --benchmark-e4a-v15-natural --scenario V15-N01 --max-seconds 120 --output .local-cache/e4a-v15/diagnostica-rc74

dotnet run --project tools/SimRisorse.Simulation -c Release -- --analyze-e4a-v15-natural --input .local-cache/e4a-v15/diagnostica-rc74 --output .local-cache/e4a-v15/analisi-rc74
```

Verificare che `DiagnosticCount` rifletta il risultato abbreviato, che `CompletionSeconds` sia nullo per esiti senza milestone e che i CSV/Markdown non confondano `timeout` con softlock. Ripetere l'analizzatore sulla stessa directory di output: deve rifiutare la sovrascrittura. Usare una nuova cartella per ogni prova. Per i benchmark a orizzonte pieno seguire la specifica RC73: le prove non devono leggere i salvataggi personali.

## Controlli di regressione

- File della GUI e schema SQLite identici alla RC73; nessuna ricetta, costo o variazione del clock.
- Le CLI storiche v14 `--benchmark-e4a-natural` e v15 `--audit-v15` restano separate.
- Nessuna riga del manifest `benchmarks/e4a-v15/scenarios.csv` deve essere cambiata da `not-run` senza risultati eseguiti e revisionati.
- Dopo il gate e le misure: archiviare report effettivi (fuori dalla distribuzione) con build, ID ruleset, policy, orizzonte, timestamp e note di interpretazione.
