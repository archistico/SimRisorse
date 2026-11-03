# G1-D26 — Piano di validazione RC10: compilazione Application.Tests

**Stato iniziale:** RC8 = **456/456** confermati. RC9: build fallita con **CS0246** sul test `V14SchoolSuppliesTests.cs:15`; suite non eseguita. RC10 corregge l'import, **nessun altro codice modificato**.

## Gate automatico

Estrarre **RC10** in una nuova directory (non copiare sopra la RC9). In Windows 11 con SDK .NET 10:

```powershell
.\validate.cmd
```

Risultato richiesto: soluzione compilata, **464/464 test passati**, zero test ignorati e nessuna regressione. Conservare `*.trx` e output di compilazione. Se emergono altri errori, registrarli e correggere prima di procedere a nuove funzionalità.

## Controllo mirato sul fix

Nel file `tests/SimRisorse.Application.Tests/V14SchoolSuppliesTests.cs`, la firma `private static (GameScenario Baseline, V14CampaignSession Session) Ready(...)` deve risolversi grazie a `using SimRisorse.Core.Content;`. Il riferimento di progetto a Core era già presente e non è stato modificato. Verificare che `Application.Tests` compili e che `V14SchoolSuppliesTests` passi.

## Gate manuale P0 (non coperto dalla patch)

Dopo i test automatici, seguire integralmente il [piano RC9](G1_D25_RC9_TEST_PLAN.md): Scuola sul modello Esercito, barra/tempo e coda, laboratorio Matite, 2 per studente, rimborsi, persistenza retrocompatibile e tabella Risorse a sette colonne. Controllare la GUI classica e la v14 a **1040×740** e **1240×850**. Fare backup dei salvataggi prima delle prove.

**E4 e navale rimangono sospesi** fino alla chiusura di P0. [Report](../design/G1_D26_RC10_FIX_IMPORT_GAMESCENARIO.md) · [Roadmap](../ROADMAP.md).
