# RC38 / E4a-D01-F1 — Piano di validazione Windows

**Baseline informativa:** RC37 compilata su Windows: **540 PASS / 1 FAIL / 541**. Errore confinato all'ordine dei record `OtherBuildings` nel test end-to-end; gli altri test hanno superato. **RC38 da validare:** 541/541 attesi.

## Controlli obbligatori

1. Estrarre lo ZIP RC38 in una directory nuova; non riutilizzare oggetti compilati di RC37.
2. Da PowerShell eseguire `./validate.cmd` e conservare il file `.trx` generato.
3. Verificare che `PencilsToStudentToResearchToLargeTierSurvivesTwoSqliteRoundTrips` sia PASS, in particolare entrambi i round trip SQLite.
4. Verificare **541 superati, zero falliti e zero ignorati**. Ogni risultato diverso mantiene il gate aperto.

```powershell
.\validate.cmd
```

**Dopo la validazione della suite**, per i due benchmark controllati:

```powershell
dotnet run --project tools/SimRisorse.Simulation -c Release -- --benchmark-e4a-school --output .local-cache/e4a/school-run-001
dotnet run --project tools/SimRisorse.Simulation -c Release -- --benchmark-e4a-school --output .local-cache/e4a/school-run-002
```

Controllare che JSON e CSV siano generati, che due run producano dati coerenti e che nessun test o simulazione tocchi i database delle partite reali. Conservare i risultati fuori dallo ZIP. Gli scenari ufficiali restano `not-run` finché non si dispone delle misure e della loro evidenza.

## Limiti

La modifica normalizza solo l'ordine di quattro insiemi identificati; l'ordine degli avanzamenti individuali degli studenti resta confrontato. Nessuna migrazione di database, nessun cambiamento di gameplay, nessun rebalance E4b.

[Rapporto F1](../design/E4A_D01_RC38_FIX_ORDERING_SQLITE.md) · [Piano originale RC37](E4A_D01_RC37_TEST_PLAN.md) · [Roadmap](../ROADMAP.md)
