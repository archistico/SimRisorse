# RC60 — Piano di validazione hotfix personale scenario combinato

**Evidenza Windows RC59:** 755 test eseguiti, 754 superati, uno fallito per `Expected: 5 / Actual: 2`. Tutti i test Desktop e Infrastructure superati. **Nuovi test RC60:** +1; **totale atteso RC60: 756**, da confermare su Windows/.NET 10.

## Gate obbligatorio

```powershell
.\validate.cmd
```

Controllare che l'esito sia **756 test superati, 0 falliti, 0 ignorati**, e che la build completi senza errori. Se altri test falliscono, raccogliere nome e stack trace, senza considerare il gate verde.

## Controlli diretti

- `FixtureHasEveryNewSiteStaffedWithoutExceedingTheSharedPopulationBudget`: 24 residenti, 6 addetti v15, **2 liberi** e Forno attivo.
- **Nuovo** `FixtureRetainsThreeFoundationWorkersWhenComputingCombinedFreePopulation`: 3 occupati iniziali, 13 occupati alimentari storici, 16 addetti nelle famiglie produttive cittadine, 6 nelle nuove strutture, 0 riserve ulteriori; 16+6+2=24.
- Tutti i test Application che avanzano il clock combinato e tutte le ricette del Deposito; round-trip SQLite con interruzioni e frazioni di secondo; test Desktop v14/v15 esistenti.

## Audit facoltativo (solo dopo il gate)

```powershell
dotnet run --project tools/SimRisorse.Simulation -c Release --no-restore -- --audit-v15 --seconds 120 --profile combined
# Se il primo audit è verde, opzionale:
dotnet run --project tools/SimRisorse.Simulation -c Release --no-restore -- --audit-v15 --hours 24 --profile combined
```

Nel JSON: `SimulatedSeconds=RequestedSeconds`, `ImbalancedSeconds=0` e `LegacyProducedFoodMilliunits=0`, più produzione positiva negli stadi previsti. Non inventare quantitativi o esiti prima dell'esecuzione.

**Esclusioni:** nessuna modifica a GUI, SQLite, prezzi, ricette, produzioni e salvataggi; nessuna rimozione ancora dello slot `food-store`. Dopo il gate, prossima fase F1-D05e.2b.

[Analisi](../design/F1_D05E2A_RC60_FIX_PERSONALE.md) · [Stato](../CURRENT_STATE.md) · [Roadmap](../ROADMAP.md).
