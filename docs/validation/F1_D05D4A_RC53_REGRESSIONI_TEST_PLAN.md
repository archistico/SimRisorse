# RC53 — Piano di verifica delle 8 regressioni RC52

**Evidenza disponibile:** TRX Windows del 05/11/2026 (suite RC52) — **703 eseguiti / 695 passati / 8 falliti / 0 ignorati**. Test Desktop superati. **RC53 è candidata: +2 nuovi `[Fact]`, 705 test attesi, non ancora eseguiti.** Il wrapper PowerShell termina con exit code 1 per i test falliti; il log non contiene diagnosi di errore compilatore C#.

## Comandi Windows (root RC53)

```powershell
.\validate.cmd

# Isolamento iniziale del gruppo di test che falliva
dotnet test tests/SimRisorse.Application.Tests/SimRisorse.Application.Tests.csproj -c Release --filter "FullyQualifiedName~F1D05d3FoodIndustryConstructionTests|FullyQualifiedName~F1D05d3bUnifiedCampaignClockTests|FullyQualifiedName~F1D05d4aCityCommandsTests"
dotnet test tests/SimRisorse.Infrastructure.Tests/SimRisorse.Infrastructure.Tests.csproj -c Release --filter "FullyQualifiedName~F1D05d3FoodIndustrySessionPersistenceTests|FullyQualifiedName~V15FoodSaveStoreTests"
```

## Pass/fail richiesti

1. Tutti e **otto** i test RC52 falliti devono ora riuscire, non solo essere rinominati/ignorati.
2. Il totale cumulativo atteso è **705** (+2 test nuovi: fish/bread aggregati in città/deposito e round-trip SQLite). Verificare il valore restituito dal runner; non fidarsi del mero conteggio statico.
3. Il test `PreparedFoodCannotAppearInCityAndDepot` continua a respingere `food` nella città sia via validazione diretta sia tramite file SQLite contraffatto.
4. `FishAndBreadCanWaitInTheCityWhileTheDepotHoldsOtherBatches` verifica il totale di Pesce/Pane, senza conteggio doppio del Cibo pronto.
5. `UndeliveredCityFishAndBreadSurviveV15SQLiteRoundTrip` verifica la precisione al millesimo dopo save/load di due inventari per ingrediente.
6. Le prove di costruttore devono partire da un checkpoint valido; lavoratori esterni, scuola e militari restano inclusi nel budget.
7. Tutti i Desktop.Tests devono restare verdi. Il gioco GUI v14 non è stato cambiato.
8. Al primo errore nuovo, conservare il `.trx` completo e lo stack: non riprendere l'integrazione GUI prima del consolidamento del gate.

## Precauzioni

- Il salvataggio di sviluppo v15 usa `PRAGMA user_version=5`; nessun passaggio su DB personali v14.
- Compilazione ed esecuzione .NET non disponibili nel confezionamento corrente.
- Il manifest SHA-256 nello ZIP verifica l'integrità dei file, **non** la correttezza dei test xUnit.

[Rapporto tecnico](../design/F1_D05D4A_RC53_FIX_REGRESSIONI.md) · [ADR 0064](../adr/0064-ingredienti-in-transito-citta-deposito-e-fixture-produttive.md) · [Stato](../CURRENT_STATE.md).
