## Stato prioritario — v60-E3b3-RC3 (3 novembre 2026)

**Ultima versione completamente verde su Windows:** E3b3-RC1 (387 test). **RC2:** 396 test, 389 superati, 7 falliti nella fixture condivisa del clock. **RC3:** popolazione corretta a 10 nella fixture (10 abitanti riservati) e a 13 nel caso impianto avanzato (13 lavoratori), aggiunta asserzione sulla disponibilità residua nulla. Modificati soltanto i test Application e la documentazione; **RC3 non validata su Windows**. La campagna v13, SQLite v2, GUI e logica v14 sono invariati.

[Rapporto](../../validation/V60_E3B3_RC3_FIX_CLOCK_FIXTURE.md) · [Guida](../../START_HERE.md) · [Roadmap](../../ROADMAP.md).

---

## Stato prioritario v60-E3b3-RC2

Ultimo `validate.cmd` verde confermato: RC1 (387 test attesi). RC2 introduce orologio v14 e +9 test (396 attesi), non ancora validati. Continuità della v13 assicurata per isolamento del codice; dopo gate Windows: E3b/3d tier, E3b/3e replay, E3b/4 interfaccia. [Ripresa](../../START_HERE.md) · [Progetto](../../design/G1_D12_E3B3C_TEMPO_UNICO_V14.md).

---

## Ripresa operativa attuale — v60-E3b3-RC1

- **Base testata dall’utente:** v60-E3b2-RC3, 380 test attesi tutti superati.
- **Nuova candidata:** sessione v14 isolata `V14CampaignSession`, E3b/3a-b, sette test nuovi da compilare/eseguire su Windows (target 387).
- **V13 e SQLite v2:** invariati; il ramo v14 non è avviabile dalla GUI.
- **Prossimo:** test Windows, poi E3b/3c ciclo del tempo unico (razioni, crescita, produzione, addestramento, ricerca); E3b/3d tier; E3b/4 UI; E4 gate finale.
- **Documenti:** [G1-D11](../../design/G1_D11_E3B3_SESSION_ISOLATA.md), [ADR 0027](../../adr/0027-e3b3-session-v14-isolata.md), [START_HERE](../../START_HERE.md), [ROADMAP](../../ROADMAP.md).

---

## Ripresa corretta — v60-E3b2-RC3

RC2 ha superato tutti i test su Windows (conferma utente). RC3 risolve la mancata rilevazione di una `progression_training` tagliata nell’ultima posizione, aggiungendo un contatore di righe in `progression_school_state`. Nuove tre prove xUnit, **validazione RC3 in sospeso**. La vecchia campagna `civil-growth-13` resta invariata. [Dettagli](../../validation/V60_E3B2_RC3_SQLITE_INTEGRITY.md), [Roadmap E3b/3](../../design/G1_D10_E3B3_SESSION_PLAN.md).

---

## Ripresa prioritaria — v60-E3b2-RC2

**Conferma Windows:** v60-E3b-RC1, validata dall’utente. **E3b2-RC1:** 377 test, 372 superati e 5 falliti nella nuova suite SQLite v3. **E3b2-RC2:** corretti i test (ordine delle collezioni con chiave e riga di formazione inesistente), ripulita la radice; validazione Windows ancora da effettuare. La campagna `civil-growth-13`, la GUI e lo store v2 restano invariati. E3b/3 segue il gate di E3b2. [Dettaglio delle correzioni](../../validation/V60_E3B2_RC2_SQLITE_TEST_FIX.md) · [Guida aggiornata](../../START_HERE.md).

---

> **RIPRESA ATTIVA G1-D9 — 03/11/2026.** v60-E3b-RC1 testata con successo dall’utente; questo incremento v60-E3b2-RC1 contiene un **nuovo store SQLite v3 isolato**, undici test e documentazione da validare su Windows. Inalterati v13, vecchio SQLite, sessione e interfaccia. [G1-D9](../checkpoints/G1_D9_CHECKPOINT.md) e [ADR 0025](../../adr/0025-e3b2-sqlite-v3-copy-on-write-isolato.md).

> **RIPRESA ATTIVA 03/11/2026 — v60-E3b-RC1, NON VALIDATA.** Contratto di snapshot v14 (solo Core) e import da `civil-growth-13` implementati come oggetti separati, con 7 test Application scritti e non eseguiti. **Nessun SQLite v3 o GameSession v14; niente modifica alla GUI.** Per riprendere: [D8](../checkpoints/G1_D8_CHECKPOINT.md) e [specifica](../../design/G1_D8_E3B_CONTRATTO_SNAPSHOT_MIGRAZIONI.md). Baseline verificata v59-RC1.

> **RIPRESA ATTIVA 03/11/2026 — v60-E3-RC1, NON VALIDATA.** Base v59-RC1 (`civil-growth-13`) confermata verde dall’utente. E1/E2/E3 sono prototipi Core isolati, senza modifica di salvataggi/UI o gameplay: E3 introduce Scuola, formazione, qualifica, punti e acquisti tecnologia e DTO sperimentale. Solo accumulo/spesa libera è decisione approvata; costi e tempi restano proposti. Leggere [D7](../checkpoints/G1_D7_CHECKPOINT.md), [dettaglio E3](../../design/G1_D7_E3_SCUOLA_RICERCA_IMPLEMENTAZIONE_ISOLATA.md) e [ADR 0023](../../adr/0023-e3-school-technology-ledger-isolated-prototype.md) prima di continuare con E3b.

> **Handoff aggiornato G1-D6 (03/11/2026):** v60-E2-RC1 è un prototipo Core isolato, non validato. Sono presenti E1 (costi/ricerca) ed E2 (famiglie a tier, upgrade, proiezione). Ultima base testata Windows: v59-RC1. Non attivare v14 o modificare DB senza E3+E4 e validazioni. [Checkpoint](../checkpoints/G1_D6_CHECKPOINT.md).

> **G1-D4 · 03/11/2026 — PROPOSTA NON APPROVATA:** design completo dell’albero ricerche, con **8 tecnologie candidate attuabili**, 3 differite e copertura delle 16 famiglie produttive. Dati e test documentali in [G1-D4](../../design/G1_D4_ALBERO_TECNOLOGICO_E_GAMEPLAY.md). Base eseguibile v59-RC1/`civil-growth-13` invariata; navale sospeso fino a nuovo bilanciamento.

> **STATO ATTIVO 03/11/2026 — v59-G1-D1:** la v59-RC1 `civil-growth-13` ha superato tutti i test Windows secondo l'utente. Nuova proposta navale G1, nessun codice modificato. Documenti autorevoli: [00_START_HERE](../../START_HERE.md), [CURRENT_STATE](../../CURRENT_STATE.md), [ROADMAP](../../ROADMAP.md), [AUTHORITY_REGISTRY](../../AUTHORITY_REGISTRY.md), [G1_NAVAL_MISSIONS_DECISION_GATE](../../design/G1_NAVAL_MISSIONS_DECISION_GATE.md). Il testo storico seguente rimane per tracciabilità.

> **Stato attivo 03/11/2026 — v59-RC1:** v58-RC5 `validate.cmd` passato (conferma utente). La v59-RC1 aggiunge il benchmark M5.3c e 9 test parametrici, nessuna modifica al motore gameplay, ed è da verificare su Windows. Per evitare informazioni obsolete: [00_START_HERE](../../START_HERE.md), [CURRENT_STATE](../../CURRENT_STATE.md), [ROADMAP](../../ROADMAP.md), [AUTHORITY_REGISTRY](../../AUTHORITY_REGISTRY.md). L’hand-off v57 è conservato in [archivio](../../NEW_CHAT_HANDOFF_V57_ARCHIVE.md).

