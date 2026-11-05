# F1-D05e.1 — RC57, controllo fisico e stress deterministico

## Obiettivo

Nessuna modifica di GUI o formule economiche. Rendere diagnosticabile una variazione ingiustificata delle risorse dopo un tick v15 e dare un comando riproducibile per sessioni simulate lunghe. RC56 (731 test superati) è la base.

## Implementazione

| Percorso | Responsabilità |
| --- | --- |
| `src/SimRisorse.Core/Food/V15StockReconciliation.cs` | `PhysicalTotals` e `ReconcileSecond` puri; elenco completo di differenze per ID in milliunità |
| `tests/SimRisorse.Application.Tests/F1D05e1V15StockAuditTests.cs` | Conservazione 1 s, proprietari fisici, stock sovracapienti, produttore legacy, integrità del ledger falsificato, equivalenza continua/spezzata (120 s) |
| `tests/SimRisorse.Infrastructure.Tests/F1D05e1V15StressPersistenceTests.cs` | Checkpoint ripresi quattro volte, decimali di secondo, intermediate stock e round-trip SQLite per ogni proprietario |
| `tools/SimRisorse.Simulation/V15StressAudit.cs` | Runner isolato che chiama il vero clock e riconcilia ogni secondo fino a 172800 s |
| `tools/SimRisorse.Simulation/Program.cs` | Nuovo flag `--audit-v15`; JSON riassuntivo o errore sul primo secondo incoerente |

Un trasferimento di Pesce o Pane dalla città al Deposito cambia la posizione della scorta, **non** il totale; analogamente Bovini/Selvaggina trasformati e Carne/Frutta/Verdura consegnate non sono ricompensati come nuove risorse. Il ledger include anche il Cibo pronto del Deposito, non un «Cibo» cittadino duplicato. Gli stock possono superare la nuova capacità se già presenti: **mai troncarli** durante l’operazione.

### Limite metodologico

`V15CampaignAdvancePlan.FirstSecondProduction` e `FirstSecondConsumption` descrivono soltanto il primo secondo completo. `ReconcileSecond` accetta esclusivamente un passo lungo esattamente un secondo con un secondo intero eseguito; usarlo con un `Plan(2h)` darebbe una falsa anomalia. Per i test di stress, avanzare e verificare ogni secondo. Il percorso di gioco ordinario continua a calcolare i gross flow solamente sul primo secondo e non effettua automaticamente l’audit.

### Esecuzione Windows

```powershell
.\validate.cmd

dotnet run --project tools/SimRisorse.Simulation -c Release --no-restore -- --audit-v15 --seconds 600

dotnet run --project tools/SimRisorse.Simulation -c Release --no-restore -- --audit-v15 --hours 24

# Stress esteso opzionale
dotnet run --project tools/SimRisorse.Simulation -c Release --no-restore -- --audit-v15 --hours 48
```

Questi comandi usano una **campagna sintetica** (Pescatore, Deposito con addetti), non i propri database e non costituiscono un benchmark del bilanciamento dei costi o della durata di gioco. Per benchmark economici e scalabilità useremo E4a. Controllare `ImbalancedSeconds == 0`, `LegacyProducedFoodMilliunits == 0` e `SimulatedSeconds == RequestedSeconds` nel JSON, oltre all’exit code 0.

## Follow-up F1-D05e.2

- Estrarre dal checkpoint v15 l’eredità strutturale `food-store` e il coupling ai contatori/tech legacy, *solo* dopo definizione di schema autonomo; accettata rottura DB sperimentali e conservazione del DB personale v14.
- Profilo di stress alimentare con tutti i rami attivi, scuola e militari sotto pressione: i test RC57 coprono la riconciliazione e gli stock intermedi ma il runner da 24h usa una fixture di filiera Pesce→Deposito.
- Controllo visivo interattivo della GUI v15 e analisi dei risultati stress reali prima di dichiarare D05e completata.

[ADR](../adr/0067-riconciliazione-fisica-v15-e-stress-separato.md) · [Piano test](../validation/F1_D05E1_RC57_TEST_PLAN.md).
