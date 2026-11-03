# Checkpoint G1-D16 · v60-E3b4-RC1

- **Gate confermato:** RC5, 417 test superati sul Windows dell'utente.
- **Nuovo lavoro:** finestra v14 opt-in `--experimental-v14` con Scuola, ricerca, produttori per livello, civili, inventario, tempo, save/load v3 isolato.
- **Delta tecnico:** `DesktopOptions.cs`, `App.axaml.cs` modificati; `V14PreviewViewModel.cs`, `V14PreviewWindow.axaml`, `V14PreviewWindow.axaml.cs` aggiunti; `V14PreviewTests.cs` con 10 test Desktop aggiunti.
- **Invariati:** tutto Core/Application/Infrastructure, scenari, modello militare, `GameSession`, `MainWindow`, SQLite v2/v3, script, benchmark.
- **Target:** 427 test; nessun dotnet disponibile nell'ambiente della consegna.
- **Rischio protetto:** la v14 non scrive né importa automaticamente il v13; file v14 incompatibili non vengono sovrascritti. Preview nuova in pausa.
- **Decisioni approvate:** saldo ricerca accumulato/speso liberamente e modello architetturale isolato. **Numeri** sperimentali non approvati.
- **Prossimo:** validazione Windows e audit GUI E3b/4b, poi E4 e M5.4.

[Specifiche](../../design/G1_D16_E3B4_ANTEPRIMA_DESKTOP_V14.md) · [ADR](../../adr/0031-e3b4-gui-v14-opt-in.md) · [piano test](../../validation/V60_E3B4_RC1_TEST_PLAN.md).
