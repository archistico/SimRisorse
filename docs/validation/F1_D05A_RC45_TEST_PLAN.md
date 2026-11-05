# RC45 / F1-D05a-F1 — Piano di validazione Windows

**Base:** RC44 ha eseguito 600 test: 599 superati e 1 fallito nel solo test `CattleAndGameAreNotFoodUntilButcheredAndDepotProcessed`, perché il modello ha prodotto correttamente 6 Carne con la vecchia fixture.

## Test mirato

Dalla cartella integrale RC45, con SDK .NET 10:

```powershell
dotnet test tests/SimRisorse.Core.Tests/SimRisorse.Core.Tests.csproj -c Release --filter "FullyQualifiedName~CattleAndGameAreNotFoodUntilButcheredAndDepotProcessed"
```

Verificare che il Macellaio usi 2 Bovini e 1 Selvaggina in due lotti alternativi, produca 4 Carne e che il deposito crei Cibo solo dalla Carne macellata.

## Gate cumulativo

```powershell
.\validate.cmd
```

**Obiettivo: 600/600 test superati.** Il test mirato da solo non chiude il gate. Verificare anche che i salvataggi v14 e il gioco siano invariati rispetto a RC44; **non** eseguire migrazioni v15 (non sono ancora state implementate).

**Non dichiarare RC45 validata** prima del riscontro Windows. Dopo la conferma si potrà procedere con F1-D05b — formato SQLite v15 separato, senza sovrascrivere il file v14.
