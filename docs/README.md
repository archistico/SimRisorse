> **RC72 (candidata):** 826 test attesi, gate `validate-f1.cmd` predisposto ma non eseguito. [Piano RC72](validation/F1_D05E3_RC72_GATE_PLAN.md) · [ADR 0080](adr/0080-gate-f1-d05e-e-evidenza-riproducibile.md). Ultimo checkpoint validato RC63 (771/771).

> **Stato attuale:** RC71 candidata (822 test previsti), ultima validata RC63 (771). [Stato](CURRENT_STATE.md) · [Roadmap](ROADMAP.md) · [ADR 0079](adr/0079-planner-v15-senza-slot-produttore-ritirato.md).

# Indice della documentazione — 05/11/2026

I documenti operativi sono stati consolidati nella consegna **RC68-DOC01**. Non confondere una specifica storica con lo stato del codice attuale.

| Necessità | Documento autorevole |
| --- | --- |
| Prima lettura e comandi | [START_HERE.md](START_HERE.md) |
| Stato implementato, validazioni e lacune | [CURRENT_STATE.md](CURRENT_STATE.md) |
| **Piano dei prossimi lavori, dipendenze e criteri di uscita** | [ROADMAP.md](ROADMAP.md) |
| Passaggio completo ad altra chat/sviluppatore | [NEW_CHAT_HANDOFF.md](NEW_CHAT_HANDOFF.md) |
| Regole approvate, proposte e loro precedenza | [AUTHORITY_REGISTRY.md](AUTHORITY_REGISTRY.md) |
| Catalogo, edifici e risorse | [CATALOG_CURRENT.md](CATALOG_CURRENT.md) |
| Contratto di salvataggio | [SAVE_FORMAT.md](SAVE_FORMAT.md) |
| Contratto visivo vincolante | [PIXEL_ART_UI.md](PIXEL_ART_UI.md) e [ADR 0071](adr/0071-gui-v14-contratto-canonico-v15.md) |
| Tecnica, runbook e architettura | [DEVELOPMENT.md](DEVELOPMENT.md), [ARCHITECTURE.md](ARCHITECTURE.md) |
| Scelte progettuali dettagliate | [design/](design/) e [registro ADR](adr/README.md) |
| Piani di test, benchmark, controlli | [validation/](validation/), [benchmarks E4a](../benchmarks/e4a/README.md) |
| Storico dettagliato | [CHANGELOG](../CHANGELOG.md), [history/](history/), specifiche RC archiviate |

**Regola di lettura:** la sezione attiva dei documenti elencati sopra prevale sui rapporti e sui piani di versioni precedenti. I test RC64–RC68 non sono da considerare superati finché non vengono eseguiti. In caso di divergenze fra documenti: richieste più recenti dell'utente → ADR accettati più recenti → documenti operativi → codice e test verificati → note storiche.

**Pulizia:** il vecchio [HANDOFF.md](HANDOFF.md) è ridotto a rimando per compatibilità dei collegamenti; la cronologia delle modifiche rimane nel `CHANGELOG.md`, negli ADR e nei dossier storici. Non usare vecchie pagine `*_ARCHIVE.md` come roadmap attiva.
