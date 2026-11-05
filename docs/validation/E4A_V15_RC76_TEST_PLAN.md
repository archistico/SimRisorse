> **Aggiornamento RC76-HF01:** due errori CS0117 dei benchmark sono stati corretti introducendo una proiezione pubblica condivisa della forza lavoro; +2 test, **869** cumulativi attesi. Vedere [piano hotfix](RC76_HF01_COMPILATION_TEST_PLAN.md). Il totale originale di 867 qui sotto appartiene alla RC76 precedente.

# RC76 — Piano di validazione E4a.2c

**Suite cumulativa attesa:** 867 test = 857 RC75 + 10 test RC76. Nessun test RC76 eseguito qui.

## Test nuovi: `E4aV15IndustrialConstraintTests`

1. Campione di avvio con scorte cittadine reali, senza grants.
2. Deficit misurato al millesimo e conservazione del primo valore osservato.
3. Compatibilità con report RC75 privi di telemetria: `unmeasured`.
4. Magazzino pieno osservato come pressione, non come deadlock.
5. Capacità zero non scambiata per saturazione.
6. `InsufficientPopulation` distinto da carenza di materiali.
7. Eventi ripetuti non sommati come costo cumulativo.
8. Deficit negativo rifiutato.
9. Duplicati scenario/politica rifiutati.
10. CSV e Markdown dichiarano esplicitamente la natura dell'evidenza.

## Accettazione cumulativa

```powershell
.\validate-f1.cmd
```

Attendere esattamente **867 test verdi**, zero skipped, smoke Desktop e audit baseline/combined. La revisione manuale della GUI rimane indispensabile; i test non la sostituiscono.

## Verifiche diagnostiche facoltative, non gate economico

```powershell
dotnet run --project tools/SimRisorse.Simulation -c Release -- --benchmark-e4a-v15-industry --scenario V15-I01 --policy research-first --max-seconds 120 --output .local-cache/e4a-v15/rc76-prova-001

dotnet run --project tools/SimRisorse.Simulation -c Release -- --analyze-e4a-v15-industry --input .local-cache/e4a-v15/rc76-prova-001 --output .local-cache/e4a-v15/rc76-analisi-001
```

Confrontare `constraints.json/csv/md` e i risultati originari; i deficit delle iscrizioni si riferiscono a Cibo nel Deposito e Matite in città. Evitare la sovrascrittura di output già esistenti.

**Non attestati:** compilazione .NET 10, test xUnit RC76, benchmark a orizzonte completo, validazione GUI manuale.
