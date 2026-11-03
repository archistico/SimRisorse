# G1-D26 — RC10: correzione CS0246 in V14SchoolSuppliesTests

**Data:** 3 novembre 2026 · **Baseline:** v60-E3b4-RC9 / G1-D25 · **Stato:** candidata alla validazione Windows.

## Evidenza del problema

Durante `./validate.cmd` l'utente ha ottenuto una build della soluzione fallita per **un solo errore di compilazione**:

```text
tests/SimRisorse.Application.Tests/V14SchoolSuppliesTests.cs(15,21):
error CS0246: 'GameScenario' non trovato
```

Dal log: Core, Application, Core.Tests, Infrastructure, Infrastructure.Tests, Simulation, Desktop e Desktop.Tests hanno compilato; la compilazione si interrompe nel progetto Application.Tests. **I test xUnit della RC9 non sono partiti**, pertanto nessuno dei 464 test può essere dichiarato superato sulla RC9.

## Causa e modifica

La classe `GameScenario` è definita in `src/SimRisorse.Core/Content/GameScenario.cs` nel namespace `SimRisorse.Core.Content`. Il nuovo test `V14SchoolSuppliesTests.cs` la usa nella firma del metodo `Ready` ma non importava il namespace.

È stata aggiunta una sola direttiva:

```csharp
using SimRisorse.Core.Content;
```

**Diff del codice di produzione: vuoto.** La patch non tocca motore, bilanciamento, GUI v13/v14, meccaniche di Matite, XAML, SQLite o salvataggi.

## Verifiche e prossima attività

- Ricerca statica del tipo e del suo namespace: **passata**.
- Import esatto nel file di test: **verificato**.
- Archiviazione, documentazione e riferimenti: **da verificare nella consegna**.
- Compilazione e 464 test xUnit su Windows: **da eseguire** (`./validate.cmd`).
- Controllo visivo GUI Scuola/Matite e parità v13/v14: **ancora aperto**.

**Ultima suite effettivamente verde confermata:** RC8, 456/456. La ricetta e gli ulteriori parametri della Scuola rimangono candidati; l'obbligo di almeno **2 Matite per studente** è confermato. E4 e navale sospesi fino ai gate P0.

[Piano RC10](../validation/G1_D26_RC10_TEST_PLAN.md) · [Piano RC9](../validation/G1_D25_RC9_TEST_PLAN.md) · [Roadmap](../ROADMAP.md) · [Handoff](../NEW_CHAT_HANDOFF.md).
