## Stato prioritario — v60-E3b3-RC3 (3 novembre 2026)

**Ultima versione completamente verde su Windows:** E3b3-RC1 (387 test). **RC2:** 396 test, 389 superati, 7 falliti nella fixture condivisa del clock. **RC3:** popolazione corretta a 10 nella fixture (10 abitanti riservati) e a 13 nel caso impianto avanzato (13 lavoratori), aggiunta asserzione sulla disponibilità residua nulla. Modificati soltanto i test Application e la documentazione; **RC3 non validata su Windows**. La campagna v13, SQLite v2, GUI e logica v14 sono invariati.

[Rapporto](../../validation/V60_E3B3_RC3_FIX_CLOCK_FIXTURE.md) · [Guida](../../START_HERE.md) · [Roadmap](../../ROADMAP.md).

---

## Ripresa attiva — v60-E3b3-RC2

La v60-E3b3-RC1 ha passato `validate.cmd` su Windows secondo l'utente. Il nuovo candidato RC2 aggiunge il clock unico della sessione v14 con test di equivalenza, pausa e replay SQLite (+9 casi non ancora validati). Il gioco giocabile resta `civil-growth-13`. Leggere [G1-D12](../../design/G1_D12_E3B3C_TEMPO_UNICO_V14.md) e [guida](../../START_HERE.md).

---

## Ripresa attiva — v60-E3b3-RC1

RC3 E3b2 validata su Windows dall’utente. E3b/3a-b implementata in **isolamento**: bootstrap v13→v14, restore, costruzione Scuola, formazione/ricercatori e acquisto volontario tecnologie; sette nuovi test **da eseguire**. Campagna v13, GUI, SQLite v2 immutati. Leggere [G1-D11](../../design/G1_D11_E3B3_SESSION_ISOLATA.md) e [guida](../../START_HERE.md).

---

## Checkpoint attivo — v60-E3b2-RC3

RC2 validata su Windows dall'utente; RC3 aggiunge la verifica SQLite v3 del numero degli studenti in formazione e 3 nuovi test pendenti. Non modificare la partita v13/SQLite v2. Prossimo: E3b/3a, bootstrap/restore separato. [Rapporto](../../validation/V60_E3B2_RC3_SQLITE_INTEGRITY.md) · [Piano](../../design/G1_D10_E3B3_SESSION_PLAN.md).

---

## Ripresa attuale — v60-E3b2-RC2

**Conferma Windows:** v60-E3b-RC1, validata dall’utente. **E3b2-RC1:** 377 test, 372 superati e 5 falliti nella nuova suite SQLite v3. **E3b2-RC2:** corretti i test (ordine delle collezioni con chiave e riga di formazione inesistente), ripulita la radice; validazione Windows ancora da effettuare. La campagna `civil-growth-13`, la GUI e lo store v2 restano invariati. E3b/3 segue il gate di E3b2. [Dettaglio delle correzioni](../../validation/V60_E3B2_RC2_SQLITE_TEST_FIX.md) · [Guida aggiornata](../../START_HERE.md).

---

> **RIPRESA E3b/1 — 03/11/2026:** v60-E3b-RC1 aggiunge snapshot candidato v14 e conversione validata strutturalmente solo in memoria (test .NET non ancora eseguiti). **Non aprire né sovrascrivere database personali con questo ramo.** Vedere [checkpoint D8](../checkpoints/G1_D8_CHECKPOINT.md).

> **RIPRESA CORRENTE v60-E1-RC1 / G1-D5:** il documento storico che segue resta archivio; v59-RC1 è la campagna validata. In v60 sono aggiunti soltanto componenti Core isolati per prezzo progressivo e saldo ricerca libero, con test non ancora eseguiti. La modalità di ricerca spendibile a scelta è approvata; numeri e attivazione no. Leggere [checkpoint](../checkpoints/G1_D5_CHECKPOINT.md) e [stato](../../CURRENT_STATE.md).

> **G1-D4 · 03/11/2026 — PROPOSTA NON APPROVATA:** design completo dell’albero ricerche, con **8 tecnologie candidate attuabili**, 3 differite e copertura delle 16 famiglie produttive. Dati e test documentali in [G1-D4](../../design/G1_D4_ALBERO_TECNOLOGICO_E_GAMEPLAY.md). Base eseguibile v59-RC1/`civil-growth-13` invariata; navale sospeso fino a nuovo bilanciamento.

> **G1-D3 · 03/11/2026 · PROPOSTA NON APPROVATA:** analisi quantitativa di incremento costi per famiglia, tre taglie (1/3/10), upgrade, Scuola, ricercatori e migrazioni SQLite. Nessuna modifica alla v59 eseguibile. [Contratto G1-D3](../../design/G1_ECONOMIA_RICERCA_CONTRATTO_QUANTITATIVO.md) · [ADR 0019](../../adr/0019-contratto-quantitativo-economia-ricerca-proposta.md).

> **STATO ATTIVO 03/11/2026 — v59-G1-D1:** codice v59-RC1 `civil-growth-13` validato dal tuo `validate.cmd`; G1 è un dossier progettuale senza codice nuovo. Leggere [00_START_HERE](../../START_HERE.md), [stato](../../CURRENT_STATE.md), [roadmap](../../ROADMAP.md) e [G1 navale](../../design/G1_NAVAL_MISSIONS_DECISION_GATE.md). **Le informazioni v57/v58 sottostanti sono archivio storico** e non rappresentano la baseline corrente.

> **RIPRESA ATTIVA 03/11/2026 — v59-RC1:** la v58-RC5 ha passato `validate.cmd` secondo l’utente; questa RC1 aggiunge solo strumentazione M5.3c (9 profili di addestramento + 9 test, bilancio della simulazione reale a 8 ore) ed è **da validare**. Ruleset `civil-growth-13`; navi non implementate. Stato aggiornato in [CURRENT_STATE](../../CURRENT_STATE.md), [ROADMAP](../../ROADMAP.md), [M5.3c](../../validation/M5_3C_V59_MILITARY_BALANCE.md). Le note v57 e v58 sottostanti sono fotografie storiche.

> **Aggiornamento 3 novembre 2026 — v58-RC3 (da validare).** Il gate Windows sulla RC2 ha compilato ed eseguito 299 test: 295 superati e 4 falliti. La RC3 corregge queste anomalie; richiede nuovamente `\.\validate.cmd` su Windows. Consultare [report RC3](../../validation/V58_RC3_299_TESTS.md), [stato](../../CURRENT_STATE.md), [roadmap](../../ROADMAP.md) e [rilascio candidato](../releases/V58_RILASCIO_CANDIDATO.md). I paragrafi v57 sottostanti sono documentazione storica.

> **Aggiornamento 03/11/2026 · v58-RC2 (non validata):** questo documento contiene materiale del precedente checkpoint v57 e alcune parti sono storiche. Per lo stato attuale del ramo unificato leggere [docs/CURRENT_STATE.md](../../CURRENT_STATE.md), [docs/ROADMAP.md](../../ROADMAP.md) e [docs/validation/V58_RECONCILIATION.md](../../validation/V58_RECONCILIATION.md). Nuovo ruleset candidato: `civil-growth-13` (addestramento individuale); flotta non implementata.

# SimRisorse — handoff operativo

**28 ottobre 2026 · v57 documentale, codice v55.** Per le istruzioni da consegnare a una nuova chat aprire per primo [`../00_START_HERE.md`](../../START_HERE.md). Questa pagina contiene **solo lo stato attivo**. Le fotografie precedenti sono conservate in [HANDOFF_ARCHIVE_V56.md](../../HANDOFF_ARCHIVE_V56.md) e [HANDOFF_HISTORY.md](../../HANDOFF_HISTORY.md).

## Punto di ripresa

- **Software:** v55; ruleset **`civil-growth-12`**, **21 strutture** catalogate, **16 risorse**. La v57 aggiorna documentazione soltanto.
- **Nomi esatti e ID:** [CATALOG_CURRENT.md](../../CATALOG_CURRENT.md), inclusi `planks` = **Legno lavorato** e `stonecutter` = **Tagliapietra**.
- **Prima campagna:** 22 edifici-obiettivo scelti, 20 già nel gioco; **Municipio iniziale è extra** e non partecipa alla condizione di vittoria. Mancano **Cantiere** e **Castello**; una conquista obbligatoria sblocca il Castello.
- **Economia:** filiere civili e industriali, Forgia/Spade, Zecca/Monete, Caserma/Esercito implementati; niente navigazione, isole, conquiste o vittoria.
- **Stoccaggio:** 1 Municipio unico (+1.000 a tutte le risorse), produttori +50 a ogni proprio output, case +10/+25 Cibo e Pane, Caserma +20 Spade, Magazzino +500 a tutte; inventario sempre condiviso. Dotazioni iniziali **8 abitanti, 120 Tavole, 80 Pietra, 30 Cibo** con 5 edifici iniziali incluso il Municipio.
- **Persistenza:** SQLite e snapshot **v2**, migrazioni S1 `civil-growth-1…-12` con backup; conversione v11→v12 aggiunge Municipio1 senza ripetere scorte e conserva esercito e orologio. Non toccare salvataggi personali.
- **UI:** schede Edifici, Risorse, Esercito; nomi Risorse alfabetici, scrollbar affiancata, selezione ocra, tooltip di capacità e dettaglio uniforme con icone, diagnosi produttive. Municipio senza comandi costruzione/personale.

## Gate ancora pendente

**287 casi xUnit attesi** su v55/v57. Qui è stata eseguita soltanto una verifica **statica**; .NET SDK assente. Chiedere l'esito di `.\validate.cmd` sul PC Windows prima di attribuire un test passato. Controllare i render/UI a **1040×740** e **1240×850**. Dettaglio: [V55_STATIC_AUDIT.md](../../validation/V55_STATIC_AUDIT.md). La conferma utente «test passati» sulla v45 e i log intermedi riguardano versioni precedenti, **non** certificano v55.

## Prossimo passo autorizzato

1. Acquisire gate Windows e correggere eventuali errori di compilazione, suite, simulazioni o smoke **senza cambiare il gameplay**.
2. Sottoporre all'utente N-01…N-07 nella [scheda M5.4 v55](../../design/M5_4_DECISION_GATE_V55.md) se il gate è positivo. **Nessuna opzione navale è ancora approvata.**
3. Dopo approvazione esplicita, introdurre la futura `civil-growth-13` per Cantiere/flotta: M5.4a Core/Application, M5.4b SQLite/Desktop, M5.4c preflight. Successivamente spedizioni, conquista, Castello, vittoria e bilanciamento.

## Riferimenti obbligatori

| Documento | Perché leggerlo |
| --- | --- |
| [CURRENT_STATE.md](../../CURRENT_STATE.md) | Baseline reale, numeri, prove disponibili |
| [NEW_CHAT_HANDOFF.md](../../NEW_CHAT_HANDOFF.md) | Contesto completo e regole di collaborazione |
| [AUTHORITY_REGISTRY.md](../../AUTHORITY_REGISTRY.md) | Decisioni vincolanti contro proposte |
| [ROADMAP.md](../../ROADMAP.md) | Sequenza delle milestone ancora da completare |
| [M5.4 N-01…N-07](../../design/M5_4_DECISION_GATE_V55.md) | Contratto navale da scegliere |
| [V55 audit](../../validation/V55_STATIC_AUDIT.md) | Gate statico e protocollo Windows |

In ogni avanzamento riferire: **versione, milestone, test confermati e pendenti, modifiche, prossima roadmap, decisioni ancora aperte**. Non inventare risultati di benchmark e non ripartire da M0.
