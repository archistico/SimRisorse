# RC45-DOC01 — Registro di pulizia e consolidamento documentale

**4 novembre 2026 · Solo documentazione.** Baseline applicativa RC45 confermata con 600 test. Non sono stati modificati codice C#, XAML, test, script né schemi SQLite.

## Scelta editoriale

- I documenti `docs/ROADMAP.md`, `docs/CURRENT_STATE.md`, `docs/NEW_CHAT_HANDOFF.md`, `docs/HANDOFF.md`, `docs/README.md`, `docs/START_HERE.md`, `docs/AUTHORITY_REGISTRY.md`, `docs/GAME_DESIGN.md` e `docs/CATALOG_CURRENT.md` sono tornati a essere documenti **correnti**, invece di concatenazioni di vecchi rilasci con baseline non più vere.
- Il README della radice presenta solo l'avvio attuale e rimanda agli indici. Il `CHANGELOG.md` mantiene lo storico cronologico; i 55 ADR preesistenti sono stati mantenuti e l'ADR 0056 definisce il nuovo indirizzo senza retroattività.
- Le specifiche datate ancora utili a migrazioni, test storici, asset o derivazione delle decisioni rimangono consultabili in `docs/design/`, `docs/validation/`, `docs/history/` e `docs/references/`. I vecchi ZIP di release consentono la ricostruzione dei dettagli di ogni checkpoint.
- La vecchia analisi [Carne](../scenarios/MEAT_DESIGN_REVIEW.md) è stata marcata **storica/superata**: non è più una direttiva contraria alla filiera F1 approvata.

## File eliminati perché senza dipendenze accertate

Le ricerche dei riferimenti sono state effettuate sui documenti e sui sorgenti del pacchetto RC45; i seguenti nove file non avevano collegamenti o menzioni nominali nelle altre risorse del repository. Si tratta di copie di stati precedenti o di audit superati; non sono parte dei formati di salvataggio, delle fonti applicative o degli ADR.

- `docs/AUTHORITY_REGISTRY_V57_ARCHIVE.md`
- `docs/CURRENT_STATE_V58_RC5_ARCHIVE.md`
- `docs/history/operational/CURRENT_STATE_RC5.md`
- `docs/history/operational/START_HERE_RC5.md`
- `docs/history/operational/README_RC5.md`
- `docs/history/operational/NEW_CHAT_HANDOFF_G1_D28_ARCHIVE.md`
- `docs/history/operational/ROADMAP_G1_D28_ARCHIVE.md`
- `docs/history/operational/CURRENT_STATE_G1_D28_ARCHIVE.md`
- `docs/validation/G1_D23_RC7_STATIC_AUDIT.md`

**Non eliminati:** i checkpoint storici, la lunga cronologia `CHANGELOG.md`, gli ADR, la matrice di confronto GUI, le prove e i piani di migrazione, le immagini originali e i report E4a. Anche se non operativi, possono servire per audit, tracciabilità e regressioni. Le specifiche non correnti devono essere lette in contesto; la versione più recente si trova sempre in [roadmap](../ROADMAP.md) e [stato](../CURRENT_STATE.md).

## Artefatti nuovi

- [Specifica E4b costi/squadre](../design/E4B_P00_COSTI_PROGRESSIVI_E_SQUADRE_MINIME.md).
- [Piano prove economiche](../validation/E4B_P00_COSTI_SQUADRE_TEST_MATRIX.md).
- [ADR 0056](../adr/0056-progressione-tardiva-e-squadre-minime-proposta.md).
- [Roadmap riordinata](../ROADMAP.md), [stato](../CURRENT_STATE.md), [handoff](../NEW_CHAT_HANDOFF.md) e [registro decisioni](../AUTHORITY_REGISTRY.md).

I nuovi prezzi restano **solo proposti**. La prossima milestone di codice resta F1-D05b.
