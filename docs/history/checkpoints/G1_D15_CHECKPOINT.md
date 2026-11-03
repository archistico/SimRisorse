# Checkpoint G1-D15 — SimRisorse v60-E3b3-RC5

**Data:** 03/11/2026. **Confermato dall'utente:** E3b3-RC4, 408 test verdi. **Candidato corrente:** RC5, E3b/3e.

## Delta

`src/SimRisorse.Core/Sessions/V14CampaignSession.cs`: nuovo `BuildCivicBuilding`, verifica prerequisiti condivisa. Nuovo `src/SimRisorse.Core/Sessions/V14CivicConstruction.cs`: esiti tipizzati.

Nuove suite:
- `tests/SimRisorse.Application.Tests/V14CivicConstructionTests.cs` (7 casi).
- `tests/SimRisorse.Infrastructure.Tests/V14ReplayPersistenceTests.cs` (2 casi).

**Validazione Windows RC5:** da eseguire (`.\validate.cmd`, 417 test attesi).

**Non cambiati:** v13 `GameSession`, catalogo, GUI Avalonia, SQLite v2 e v3, formati snapshot, test già presenti e script. Radice senza file storici sparsi.

## Roadmap

E3b/3e → conferma test → E3b/4 UI sperimentale → E4 bilanciamento e attivazione controllata → G1-N/M5.4 flotta. Ricerca a saldo libero e spesa volontaria approvata; valori economici candidati.

[Specifica](../../design/G1_D15_E3B3E_REPLAY_CIVILI_V14.md) · [ADR](../../adr/0030-e3b3e-edifici-civili-replay-v14.md) · [test](../../validation/V60_E3B3_RC5_TEST_PLAN.md).
