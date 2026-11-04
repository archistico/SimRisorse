# G1-D39-F1 / RC25 — Correzione CS1061 nei test headless

**Data:** 04/11/2026. **Ultimo gate Windows confermato:** RC20 (490 test). **RC24:** compilazione fallita in `SimRisorse.Desktop.Tests` con due errori CS1061; la suite non è stata eseguita. **RC25:** correzione candidata; attesi **501 test**, senza nuovi casi e senza risultati Windows.

## Diagnosi confermata

Nel file `tests/SimRisorse.Desktop.Tests/V14BuildingUpgradeAuditTests.cs`, alle righe 71 e 136 della RC24, l'invocazione `window.MouseMove(...)` non era risolta perché mancava l'import `using Avalonia.Headless;`. Il metodo è un'estensione del supporto headless, già impiegata altrove in `V14UiParityRegressionTests.cs` e `DesktopTests.cs` con lo stesso import.

## Modifica

Aggiunto esclusivamente `using Avalonia.Headless;` al file di test interessato. Nessuna asserzione rimossa, nessun cambiamento ai due scenari hover/upgrade; nessun cambiamento ai sorgenti dell'applicazione, alla GUI v13/v14, al motore, a SQLite o ai salvataggi. L'obiettivo è risolvere i due errori **CS1061** senza ridurre la copertura.

## Gate

1. Estrarre RC25 in una cartella nuova e lanciare `./validate.cmd` su Windows con SDK .NET 10.
2. Verificare che `SimRisorse.Desktop.Tests` compili senza CS1061 e che la suite cumulativa esegua **501 test**; segnalare per intero eventuali fallimenti successivi.
3. Con salvataggi in copia, seguire il piano [RC25](../validation/G1_D39_RC25_TEST_PLAN.md) e la [matrice P0](../validation/G1_D39_P0_UI_MATRIX.md). Nessuna chiusura automatica del gate grafico senza osservazione Windows.
4. Riprendere **G1-D40** (UI-05/UI-14, flussi e popolazione) quando possibile; E4 resta sospeso.

**Nota:** i controlli statici su archivio e documenti non sostituiscono la compilazione/xUnit.

[Roadmap](../ROADMAP.md) · [Stato](../CURRENT_STATE.md) · [Handoff](../NEW_CHAT_HANDOFF.md).
