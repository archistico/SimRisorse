# Piano di validazione — RC57 / F1-D05e.1

**Base Windows confermata dall’utente:** RC56, **731/731**. **RC57:** 12 nuovi casi xUnit (8 `[Fact]` Application, 1 `[Theory]` con 2 `[InlineData]`, 2 `[Fact]` Infrastructure), **743 totali attesi**, non eseguiti in questo ambiente privo dell’SDK .NET 10.

## Gate obbligatorio

```powershell
.\validate.cmd
```

Se fallisce: distinguere errori di compilazione, fallimenti xUnit e controlli post-test, leggere i TRX della nuova esecuzione; evitare correzioni che disabilitano un test o alterano la GUI storica.

## Test mirati

```powershell
dotnet test tests/SimRisorse.Application.Tests/SimRisorse.Application.Tests.csproj -c Release --filter FullyQualifiedName~F1D05e1V15StockAuditTests
dotnet test tests/SimRisorse.Infrastructure.Tests/SimRisorse.Infrastructure.Tests.csproj -c Release --filter FullyQualifiedName~F1D05e1V15StressPersistenceTests
```

| Gruppo | Casi | Aspettativa |
| --- | ---: | --- |
| Bilancio unitario e trasferimenti | 3 | Ogni risorsa riconciliata al millesimo, senza compensare i trasferimenti |
| Capienze/sovracapacità e `food-store` | 2 | Scorte mantenute, produttore legacy inerte |
| Guardie dell’auditor | 3 | Manipolazione ledger rilevata, step da zero/due secondi rifiutati |
| Clock lungo spezzato e inventario complessivo | 2 | Determinismo 120 secondi, conteggio unico di tutti i contenitori |
| SQLite riavvii/intermedi | 2 | Confronto con clock continuo e round-trip dei compartimenti |
| **Totale** | **12** | Nessuna dichiarazione di test superati prima del gate reale |

## Stress opzionale (non dentro validate.cmd)

```powershell
dotnet run --project tools/SimRisorse.Simulation -c Release --no-restore -- --audit-v15 --seconds 600
dotnet run --project tools/SimRisorse.Simulation -c Release --no-restore -- --audit-v15 --hours 24
# Facoltativo: --hours 48
```

Se ogni secondo quadra il comando termina con exit code 0 e stampa un report JSON. Se una risorsa diverge indica il secondo e l’ID, interrompendosi con errore. Il runner **non apre salvataggi personali**. Non equiparare un audit sintetico di 24h al benchmark economico E4a o a un test automatizzato di persistenza continuativa.

## Gate di preservazione

- `run-game.cmd` → solo GUI v14 e DB storico; `run-game.cmd --v15` → v15 con SQLite separato.
- Sette colonne Risorse, tier diretti, Scuola/Albero nel menu, pixel art invariati.
- Identificatori, coefficiente di costo +15%, rese e lavoro minimo delle industrie **immutati**.
- F1-D05e.2 ancora aperta per rimuovere fisicamente `food-store` dallo stato v15.

[Specifiche](../design/F1_D05E1_RC57_RICONCILIAZIONE_STRESS.md) · [ADR 0067](../adr/0067-riconciliazione-fisica-v15-e-stress-separato.md).
