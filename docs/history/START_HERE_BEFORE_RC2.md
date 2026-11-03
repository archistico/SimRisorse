# AGGIORNAMENTO ATTIVO — v60-E3b2-RC1 / G1-D9

La **v60-E3b-RC1** ha superato tutti i test su Windows secondo l’utente. Questa consegna E3b/2 aggiunge **solo un lettore/scrittore SQLite v3 separato**, test Infrastructure e documentazione, e **non è ancora stata testata su .NET**. La campagna v13, il suo salvataggio v2 e la UI non cambiano. [Checkpoint G1-D9](../history/checkpoints/G1_D9_CHECKPOINT.md) · [E3b/2 SQLite](../design/G1_D9_E3B2_SQLITE_V3_ISOLATO.md).

---

> **G1-D8 / v60-E3b-RC1 · 03/11/2026 · CANDIDATO NON TESTATO:** introdotto il contratto Core completo `V14CampaignSnapshot` (formato futuro 3) e conversione in memoria `civil-growth-13` → `civil-growth-14`, con nuovi test Application. **Non sono stati modificati `GameSession` v13, SQLite v2, la GUI o il gameplay; nessun DB personale toccato.** Ultima versione testata su Windows v59-RC1. [Checkpoint D8](../history/checkpoints/G1_D8_CHECKPOINT.md) · [Specifiche](../design/G1_D8_E3B_CONTRATTO_SNAPSHOT_MIGRAZIONI.md) · [ADR 0024](../adr/0024-v14-snapshot-migration-boundaries-proposta.md).

> **G1-D7 / v60-E3-RC1 · 03/11/2026 · PROTOTIPO DA VALIDARE:** completato nel Core isolato il ciclo Scuola → formazione → ricercatori attivi → saldo ricerca libero → acquisto volontario → sblocco livelli E2; 8 ricerche produttive candidate, Logistica/Navigazione non acquistabili. **Nessuna modifica a `civil-growth-13`, SQLite o GUI.** La v59-RC1 rimane l’ultima validata su Windows. [Checkpoint D7](../history/checkpoints/G1_D7_CHECKPOINT.md) · [Dettaglio E3](../design/G1_D7_E3_SCUOLA_RICERCA_IMPLEMENTAZIONE_ISOLATA.md) · [ADR 0023](../adr/0023-e3-school-technology-ledger-isolated-prototype.md).

> **G1-D6 / v60-E2-RC1 · 03/11/2026 · CODICE ISOLATO, DA VALIDARE:** aggiunti livelli produttivi Base/Grande/Avanzato (1/3/10), upgrade e simulazione a ricette invariabili. **Non** attivi nella campagna v13, non modificati SQLite e UI. [Checkpoint G1-D6](../history/checkpoints/G1_D6_CHECKPOINT.md) · [Analisi E2](../design/G1_D6_E2_IMPLEMENTAZIONE_ISOLATA.md). La v59-RC1 resta l’ultima validata su Windows.

> **G1-D5 / v60-E1-RC1 · 03/11/2026 · CODICE ISOLATO, NON VALIDATO:** l’utente ha scelto il saldo ricerca accumulabile liberamente e spendibile a richiesta (ADR 0021 accettato). Aggiunti nel Core prezzi progressivi parametrizzati, storico acquisti e contabilizzazione della ricerca, con nuovi test **da eseguire su Windows**. La campagna v13, UI, SQLite e benchmark NON sono cambiati: la v59-RC1 resta l’ultima versione validata. [Checkpoint](../history/checkpoints/G1_D5_CHECKPOINT.md) · [Note](../design/G1_D5_E1_IMPLEMENTAZIONE_ISOLATA.md).

> **G1-D4 · 03/11/2026 · SOLO DOCUMENTAZIONE:** approfondito albero tecnologico, settori, Scuola, tempi, controlli anti-softlock, migrazioni e rollout. Regole ancora **PROPOSTE**, nessuna modifica a sorgenti/test/scripts della v59-RC1. [Analisi G1-D4](../design/G1_D4_ALBERO_TECNOLOGICO_E_GAMEPLAY.md) · [ADR 0020](../adr/0020-tech-tree-sector-research-proposta.md).

> **G1-D3 · 03/11/2026 · PROPOSTA NON APPROVATA:** analisi quantitativa di incremento costi per famiglia, tre taglie (1/3/10), upgrade, Scuola, ricercatori e migrazioni SQLite. Nessuna modifica alla v59 eseguibile. [Contratto G1-D3](../design/G1_ECONOMIA_RICERCA_CONTRATTO_QUANTITATIVO.md) · [ADR 0019](../adr/0019-contratto-quantitativo-economia-ricerca-proposta.md).

> **G1-D2 (03/11/2026):** checkpoint documentale che propone costi crescenti, taglie degli edifici e ricerca. Nessuna modifica al gameplay né approvazione delle regole. Leggere `docs/design/G1_ECONOMIA_RICERCA_LIVELLI_PROPOSTA.md` e `docs/adr/0018-progressive-construction-tiers-and-research-proposta.md`.

# SimRisorse — ripresa v59-RC1 (codice), G1-D4 (documentazione) (3 novembre 2026)

**Base codice:** v59-RC1, `civil-growth-13`, **tutti i test `validate.cmd` superati su Windows secondo l'utente**. Questo è un checkpoint **solo documentale**, nessuna modifica di `src/`, `tests/`, `tools/`, `scripts/` o delle regole. La v58-RC5 era stata validata in precedenza. L'archivio include il progetto intero ma non richiede una nuova esecuzione dei test per codice modificato.

## Stato

- **16 risorse; 20 edifici-obiettivo implementati + Municipio iniziale**, cioè 21 strutture. Mancano **Cantiere navale e Castello** per arrivare a 22 obiettivi.
- Caserma individuale con costo 260 Legno lavorato + 200 Pietra, 5 Spade/recluta, 1 posto/30 s; economia, salvataggi e UI preservati e testati.
- La v59 aggiunge nove profili controllati e raccoglie il bilancio della campagna militare di otto ore. Il validatore è verde, i valori dei JSON non sono stati ancora letti.
- **G1 adesso:** proposta di navi individuali riutilizzabili, separazione esplorazione/conquista, requisiti deterministici, no perdite irreversibili. **Prezzi/quantità/tempi/stati tecnici non ancora approvati.** Non implementare una nave su un generico «continua».

## Leggere nell'ordine

1. [Stato attuale](../CURRENT_STATE.md)
2. [Roadmap](../ROADMAP.md)
3. [Registro decisioni](../AUTHORITY_REGISTRY.md)
4. [G1 — proposta completa per flotta e spedizioni](../design/G1_NAVAL_MISSIONS_DECISION_GATE.md)
5. [ADR 0017 — PROPOSTO](../adr/0017-naval-mission-boundaries-proposta.md)
6. [Checklist tecnica e file numerici necessari](../validation/G1_READY_FOR_IMPLEMENTATION.md)
7. [Handoff](../NEW_CHAT_HANDOFF.md) — le sezioni più vecchie di v59 sono testimonianze storiche, non fonte per la versione corrente.

**Passo successivo:** acquisire i due report `.local-cache/m4/military-*.json` dalla v59-RC1 usata per il gate Windows; discutere/approvare separatamente N-01…N-07, S-01…S-10 e soglia militare dell'isola; poi iniziare M5.4a con nuovo ZIP e regressioni.
