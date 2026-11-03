# v60-E3b3-RC1 — Scopo, verifiche e limiti

**Sorgenti aggiunti:** `src/SimRisorse.Core/Sessions/V14CampaignSession.cs`.  
**Test aggiunti:** `tests/SimRisorse.Application.Tests/V14CampaignSessionTests.cs`, `tests/SimRisorse.Infrastructure.Tests/V14CampaignSessionPersistenceTests.cs`.

Il codice E3b2-RC3 già validato è mantenuto senza modifiche; nessun file di app, motore v13, SQLite v2, `validate.cmd` o catalogo corrente è alterato.

**Test aggiunti scritti:** 7. **Esecuzione xUnit:** non disponibile nell'ambiente di confezionamento. `validate.cmd` RC1 ancora da eseguire su Windows. I controlli statici e l'integrità ZIP, anche se positivi, non sostituiscono compilazione o runtime.

Attenzione: il tempo di simulazione non è ancora integrato, quindi questa non è una release giocabile della v14.
