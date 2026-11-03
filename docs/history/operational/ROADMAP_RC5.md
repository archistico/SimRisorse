# Stato operativo — v60-E3b3-RC5 (E3b/3e)

**Ultima conferma Windows:** RC4, **408/408** test superati secondo l'utente. **RC5 candidata:** costruzione civili (Case/Magazzini/Caserme) in v14, +9 casi, **417 test attesi** e replay command-stream attraverso SQLite v3. La compilazione e i test .NET RC5 restano pendenti.

- GameSession `civil-growth-13`, GUI, catalogo storico e SQLite v2 **invariati**.
- Il prototipo conserva saldo ricerca libero e spesa volontaria. Coefficiente +15% e altri valori **non approvati**.
- Prima il gate Windows, poi E3b/4 interfaccia v14 sperimentale e E4 bilanciamento/attivazione.

[Specifiche D15](../../design/G1_D15_E3B3E_REPLAY_CIVILI_V14.md) · [ADR 0030](../../adr/0030-e3b3e-edifici-civili-replay-v14.md) · [checkpoint](../../history/checkpoints/G1_D15_CHECKPOINT.md).

---

## Priorità attuale — v60-E3b3-RC4, E3b/3d (03/11/2026)

- **Ultima versione confermata dall'utente:** E3b3-RC3, `validate.cmd` completo (396 test passati); orologio unico v14 validato.
- **Questa release candidata RC4:** comandi tier nel Core v14 (`BuildProducer`, `UpgradeProducer`, `AssignProducerWorkers`, `ReleaseProducerWorkers`) e configurazione prezzi iniettabile; 12 test nuovi, **408 test attesi**, da eseguire su Windows.
- **E3b/3e dopo il gate RC4:** replay più esteso di comandi + clock + SQLite v3, e test fallimenti senza commit.
- **E3b/4:** interfaccia Scuola, ricerca, costruzione, upgrade e lavoratori.
- **E4:** bilanciamento, anti-softlock, stabilità SQLite e attivazione con rollback; poi navigazione G1-N/M5.4.
- **Invariati:** campagna `civil-growth-13`, UI corrente, SQLite v2, motore militare e prezzi giocabili. Numeri sperimentali non ancora ratificati.

[Specifica G1-D14](../../design/G1_D14_E3B3D_COSTRUZIONE_UPGRADE_TIER_V14.md) · [ADR 0029](../../adr/0029-e3b3d-comandi-tier-sessione-v14-isolati.md) · [checkpoint](../../history/checkpoints/G1_D14_CHECKPOINT.md).

---

## Priorità attuale — gate v60-E3b3-RC3 (3 novembre 2026)

- RC2 (`E3b/3c`, clock unico v14): 396 test eseguiti, 389 superati, 7 falliti per fixture che prenotava 10 abitanti con popolazione 8.
- RC3 corregge solo due conteggi errati nei test Application e controlla la disponibilità residua zero; **test Windows pendenti**.
- Dopo `validate.cmd` verde (396 test attesi + benchmark): E3b/3d (costruzione, upgrade, lavoratori), E3b/3e (replay/salvataggi), E3b/4 (UI), E4 (bilanciamento/attivazione), quindi flotta.
- Campagna giocabile v13, SQLite v2 e GUI invariati; niente nuovi costi in gioco prima che la nuova economia sia completa.

[Rapporto RC3](../../validation/V60_E3B3_RC3_FIX_CLOCK_FIXTURE.md) · [Checkpoint G1-D13](../../history/checkpoints/G1_D13_CHECKPOINT.md).

---

## Milestone attuale — E3b/3c, orologio unico v14 (v60-E3b3-RC2)

- **Validato Windows:** v60-E3b3-RC1 (`validate.cmd` passato, conferma dell'utente, 387 test attesi).
- **Implementato, non ancora convalidato:** clock v14 isolato (produzione, razioni, crescita abitanti, training militare e scolastico, punti ricerca), pausa/ripresa e avanzamento atomico, +9 test; **396 attesi**.
- **Prossimi:** gate Windows RC2; E3b/3d costruzione e upgrade tier nella sessione; E3b/3e replay SQLite; E3b/4 GUI; E4 bilanciamento e attivazione.
- `civil-growth-13`/SQLite v2/Desktop invariati. Parametri economici ancora proposti, salvo accumulo libero/spesa volontaria della ricerca.
- [Rapporto G1-D12](../../design/G1_D12_E3B3C_TEMPO_UNICO_V14.md) · [ADR 0028](../../adr/0028-e3b3c-orologio-unico-v14-isolato.md) · [checkpoint](../../history/checkpoints/G1_D12_CHECKPOINT.md).

---

## Milestone attuale — E3b/3a-b, sessione v14 isolata (v60-E3b3-RC1)

- **Validato:** v60-E3b2-RC3 (`validate.cmd` passato secondo l'utente).
- **Implementato, da validare:** `V14CampaignSession` — bootstrap/restore, costruzione Scuola, studenti, ricercatori e spesa volontaria della ricerca. +7 test, target 387; nessuna modifica al gameplay v13.
- **Prossimi gate:** E3b/3c tempo unico, E3b/3d comandi tier, E3b/3e save/restore completo; E3b/4 interfaccia; E4 bilanciamento e attivazione.
- **Non ratificati:** crescita +15%, prezzi e tempi Scuola e tier, albero dei costi e condizione di vittoria. [Dettaglio](../../design/G1_D11_E3B3_SESSION_ISOLATA.md).

---

## Milestone attuale — E3b/2, integrità SQLite v3 (v60-RC3)

- **Validato su Windows:** v60-E3b2-RC2, `validate.cmd` interamente superato (segnalazione dell'utente).
- **Implementato, da testare:** RC3 salva il conteggio degli studenti in formazione e impedisce la perdita silenziosa dell'ultima posizione; 3 casi nuovi, obiettivo 380 test. [Dettagli](../../validation/V60_E3B2_RC3_SQLITE_INTEGRITY.md).
- **Prossima milestone:** E3b/3a, bootstrap/restore di sessione v14 isolata, poi comandi Scuola, ricerca, tier e persistenza; E3b/4 UI; E4 bilanciamento. [Piano](../../design/G1_D10_E3B3_SESSION_PLAN.md).
- **Gate di attivazione:** v14 non deve entrare nella campagna `civil-growth-13` né nel vecchio database prima di completamento di ricerca, upgrade, UI, migrazioni e benchmark.

**Le sezioni successive sono archivio storico e non prevalgono su questo stato.**

---

## Storico — milestone v60-E3b2-RC2 (ora validata)

**Conferma Windows:** v60-E3b-RC1, validata dall’utente. **E3b2-RC1:** 377 test, 372 superati e 5 falliti nella nuova suite SQLite v3. **E3b2-RC2:** corretti i test (ordine delle collezioni con chiave e riga di formazione inesistente), ripulita la radice; validazione Windows ancora da effettuare. La campagna `civil-growth-13`, la GUI e lo store v2 restano invariati. E3b/3 segue il gate di E3b2. [Dettaglio delle correzioni](../../validation/V60_E3B2_RC2_SQLITE_TEST_FIX.md) · [Guida aggiornata](../../START_HERE.md).

---

> **G1-D9 / E3b/2:** persistenza SQLite v3 isolata implementata, test da eseguire. La v60-E3b-RC1 ha superato `validate.cmd` secondo l’utente. E3b/3 integra una sessione v14, E3b/4 le schermate, E4 bilanciamento/gate; nessuna attivazione dei prezzi progressivi prima del sistema completo. [Checkpoint](../../history/checkpoints/G1_D9_CHECKPOINT.md).

> **G1-D8 / 03-11-2026:** E3b/1 introduce nel Core il modello completo di snapshot v14 e la migrazione in memoria da uno snapshot v13 validato. **Non** esiste ancora il nuovo schema SQLite eseguibile né Scuola/UI/gameplay v14. Seguono E3b/2 salvataggi versionati, E3b/3 nuova sessione e meccaniche, E3b/4 GUI, E4 benchmark e gate Windows. [Checkpoint D8](../../history/checkpoints/G1_D8_CHECKPOINT.md).

> **AGGIORNAMENTO G1-D7 · 03/11/2026:** E1+E2+E3a implementati come servizi Core separati, **non validati .NET**; E3b (integrazione `GameSession`, regole v14, Scuola, SQLite e UI) e E4 (attivazione simultanea, benchmark anti-softlock) ancora da sviluppare. Regola **approvata**: punti ricerca liberi e spesa volontaria. Parametri numerici ancora candidati. [Checkpoint D7](../../history/checkpoints/G1_D7_CHECKPOINT.md). Sotto rimane la roadmap storica navale.

> **G1-D6 / E2 — CANDIDATO NON TESTATO:** predisposti livelli e upgrade nel Core isolato. `civil-growth-13` invariato. Prossimo E3: Scuola, ricercatori, ricerca, integrazione versionata e salvataggi. La v59-RC1 è l’ultima validata. [Checkpoint](../../history/checkpoints/G1_D6_CHECKPOINT.md).

> **G1-D5 / E1 · 03/11/2026 — IMPLEMENTAZIONE PARZIALE ISOLATA:** pricing parametrico e saldo ricerca libero nel Core con test nuovi, non collegati alla campagna. Scelta dell’utente approvata solo per l’accumulo libero e la spesa volontaria della ricerca; coefficienti/tech/tier ancora proposti. Validazione v60 pendente; v59 resta baseline provata. [Checkpoint](../../history/checkpoints/G1_D5_CHECKPOINT.md).

> **G1-D4 · 03/11/2026 — PROPOSTA NON APPROVATA:** design completo dell’albero ricerche, con **8 tecnologie candidate attuabili**, 3 differite e copertura delle 16 famiglie produttive. Dati e test documentali in [G1-D4](../../design/G1_D4_ALBERO_TECNOLOGICO_E_GAMEPLAY.md). Base eseguibile v59-RC1/`civil-growth-13` invariata; navale sospeso fino a nuovo bilanciamento.

## Roadmap prioritizzata dopo G1-D4 — economia prima della navigazione (PROPOSTA)

| Ordine | Gate | Scopo | Blocco di rilascio |
|---|---|---|---|
| 1 | G1-E (decisioni) | Confermare prezzi, taglie, Scuola, ricerca, migrazione e obiettivi vittoria | Non ancora approvato |
| 2 | E1 | Calcolo prezzi +15% per famiglia, cronologia e transazioni; suite dedicata | Non attivare in campagna v13 |
| 3 | E2 | 16 famiglie a tier 1/3/10 e upgrade, senza alterare il catalogo storico | Non esporre tier prima degli sblocchi |
| 4 | E3 | Scuola, formazione, ricercatori, 8 tech, UI e salvataggi `civil-growth-14` | Abilitare v14 soltanto come insieme coerente E1+E2+E3 |
| 5 | E4 | Test, simulazioni e prova di raggiungibilità su Windows e salvataggi S1 | Non dichiarare stabilità senza esito |
| 6 | G1-N | Aggiornare prezzi navali e requisiti alla nuova economia | Riprendere decisioni N/S + JSON M5.3c |
| 7 | M5.4–M7 | Cantiere/flotta, spedizioni, conquista, Castello e vittoria | Incrementi successivi con ZIP integrale e test |

**Baseline testata:** v59-RC1, `civil-growth-13`; nessun nuovo codice nella consegna G1-D4. Vedere [contratto tecnologico G1-D4](../../design/G1_D4_ALBERO_TECNOLOGICO_E_GAMEPLAY.md). La roadmap navale originale di seguito è conservata come storico precedente alla proposta economica; non autorizza a saltare E1-E4.

> **G1-D3 · 03/11/2026 · PROPOSTA NON APPROVATA:** analisi quantitativa di incremento costi per famiglia, tre taglie (1/3/10), upgrade, Scuola, ricercatori e migrazioni SQLite. Nessuna modifica alla v59 eseguibile. [Contratto G1-D3](../../design/G1_ECONOMIA_RICERCA_CONTRATTO_QUANTITATIVO.md) · [ADR 0019](../../adr/0019-contratto-quantitativo-economia-ricerca-proposta.md).

> **G1-D2 · 03/11/2026 · Nuova proposta in discussione:** richiesta di progressione più impegnativa con costi crescenti, edifici produttivi base/grandi/avanzati (1/3/10 lavoratori), Scuola, ricercatori e ricerca tecnologica. **Non ancora approvata né implementata.** Prima di M5.4 navale verificare la proposta economica, gli obiettivi di vittoria e la migrazione. [Studio completo](../../design/G1_ECONOMIA_RICERCA_LIVELLI_PROPOSTA.md) · [ADR 0018 proposto](../../adr/0018-progressive-construction-tiers-and-research-proposta.md). `civil-growth-13` e tutti i sorgenti restano invariati.

# SimRisorse — roadmap operativa attiva

**3 novembre 2026 · checkpoint di sola progettazione v59-G1-D1.** Baseline eseguibile **v59-RC1** `civil-growth-13`, test `validate.cmd` **tutti superati secondo l'utente**. Non sono state aggiunte nuove meccaniche. [Roadmap antecedente](../../ROADMAP_V58_RC5_ARCHIVE.md).

| Milestone | Attività | Stato reale | Gate per avanzare |
| --- | --- | --- | --- |
| R0 | Audit v41/v57 | Completata | Chiusa |
| R1 | Riunificazione codice militare sulla v57 | Implementata e validata in RC5 | Chiusa |
| R2 | Compatibilità SQLite fork v41/versioni storiche | Implementata, test sintetici superati | Su DB personali: prova su copie ancora consigliata |
| R3 | Validazione completa del ramo riunificato | `validate.cmd` v58-RC5 e v59-RC1 superati secondo l'utente | Chiusa per gate automatici |
| **M5.3c** | Capacità teoriche 5/10/20, 1/2/4 Caserme; campagna militare 8 h | **Automazione validata, interpretazione numerica aperta** | Acquisire due JSON e leggere il bilancio |
| **G1** | Contratto navale/missioni; N-01…N-07 e S-01…S-10 | **Proposta documentata, non approvata** | Decisioni esplicite + dati M5.3c |
| M5.4a | Cantiere e acquisto navi individuali | Non iniziata | Test Core/Application, nessuna spesa parziale |
| M5.4b | Flotta su snapshot/SQLite/UI | Non iniziata | Migrazione con backup e restore senza duplicati |
| M5.4c | Preflight capienza/soldati | Non iniziata | Controllo senza mutazioni |
| M5.5 | Esplorazione e gestione viaggio | Non iniziata | Stati, pausa, salvataggio, tempi equivalenti |
| M5.6 | Conquista deterministica | Non iniziata | Prima isola obbligatoria, zero perdite irreversibili |
| M5.7 | Castello, 22º tipo-obiettivo | Non iniziata | Sblocco solo dopo conquista |
| M6 | Vittoria, statistiche, prova completabilità | Non iniziata | Campagna reale giocabile; ~8h/32+h sono obiettivi |
| M7 | UI/qualità/distribuzione | Non iniziata | Validazione completa, pacchetto finale |

## Prossime azioni ordinate

1. Ottenere `military-balance-8-hours.json` e `military-capacity-benchmark.json` generati dalla v59 su Windows; la suite verde non sostituisce l'analisi dei numeri.
2. Rivedere insieme [il contratto G1](../../design/G1_NAVAL_MISSIONS_DECISION_GATE.md) distinguendo le tre preferenze già espresse dai quantitativi ancora aperti.
3. Approvare esplicitamente il blocco **N** (prezzi/modello navale), e il blocco **S** (semantica delle missioni) come prerequisito delle entità future. Le soglie della prima isola possono rimanere in sospeso finché non leggiamo i dati.
4. Registrare la decisione nell'ADR 0017, aggiornando lo stato da *Proposto* ad *Accettato* soltanto su indicazione esplicita dell'utente.
5. Dopo tale gate: M5.4a in una nuova release eseguibile, quindi M5.4b, M5.4c, M5.5–M5.7 e bilanciamento M6. A ogni incremento ZIP integrale, test Windows, ADR e roadmap.

**Governance:** `civil-growth-13` è l'ultima versione del ruleset; `civil-growth-14` è soltanto un nome proposto. Non alterare i cataloghi storici, la popolazione, il Municipio o il valore dei benchmark senza decisione e regressione mirata. Evitare perdita di dati: database personali soltanto su copie.
