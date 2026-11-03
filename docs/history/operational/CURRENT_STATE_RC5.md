# Stato operativo — v60-E3b3-RC5 (E3b/3e)

**Ultima conferma Windows:** RC4, **408/408** test superati secondo l'utente. **RC5 candidata:** costruzione civili (Case/Magazzini/Caserme) in v14, +9 casi, **417 test attesi** e replay command-stream attraverso SQLite v3. La compilazione e i test .NET RC5 restano pendenti.

- GameSession `civil-growth-13`, GUI, catalogo storico e SQLite v2 **invariati**.
- Il prototipo conserva saldo ricerca libero e spesa volontaria. Coefficiente +15% e altri valori **non approvati**.
- Prima il gate Windows, poi E3b/4 interfaccia v14 sperimentale e E4 bilanciamento/attivazione.

[Specifiche D15](../../design/G1_D15_E3B3E_REPLAY_CIVILI_V14.md) · [ADR 0030](../../adr/0030-e3b3e-edifici-civili-replay-v14.md) · [checkpoint](../../history/checkpoints/G1_D15_CHECKPOINT.md).

---

## Stato corrente (03/11/2026) — v60-E3b3-RC4 candidata

- **Validazione più recente:** RC3 E3b/3c, 396 test superati secondo l'utente.
- **RC4 E3b/3d:** nuovi comandi nella sessione v14 isolata per costruzione Base/Grande/Avanzato, upgrade e staffing, con prezzi E1, catalogo E2, prerequisiti e sblocchi E3a. Nuove suite Application/Infrastructure, **408 test attesi**, non verificati in .NET nel container.
- **v13 pubblica** invariata (GameSession, SQLite v2, Avalonia, contenuti, militari); **SQLite v3** già presente nel ramo sperimentale, senza nuove modifiche allo schema.
- **Parametri 15% e 2,6/6,5** rimangono candidati, configurabili nel prototipo. L'accumulo ricerca libero e la spesa volontaria sono approvati.
- **Gate futuri:** test Windows RC4 → replay E3b/3e → GUI E3b/4 → E4 validazione/bilanciamento/attivazione.

[Dettagli](../../design/G1_D14_E3B3D_COSTRUZIONE_UPGRADE_TIER_V14.md).

---

## Stato prioritario — v60-E3b3-RC3 (3 novembre 2026)

**Ultima versione completamente verde su Windows:** E3b3-RC1 (387 test). **RC2:** 396 test, 389 superati, 7 falliti nella fixture condivisa del clock. **RC3:** popolazione corretta a 10 nella fixture (10 abitanti riservati) e a 13 nel caso impianto avanzato (13 lavoratori), aggiunta asserzione sulla disponibilità residua nulla. Modificati soltanto i test Application e la documentazione; **RC3 non validata su Windows**. La campagna v13, SQLite v2, GUI e logica v14 sono invariati.

[Rapporto](../../validation/V60_E3B3_RC3_FIX_CLOCK_FIXTURE.md) · [Guida](../../START_HERE.md) · [Roadmap](../../ROADMAP.md).

---

## Stato prioritario — v60-E3b3-RC2 (3 novembre 2026)

**Confermato su Windows:** v60-E3b3-RC1, tutti i test superati secondo l'utente. **Nuova candidata:** E3b/3c, orologio unico v14 isolato, +9 test non eseguiti localmente; target 396. Scuola/ricerca rimangono funzionalità di un prototipo, non sono disponibili nel gioco v13. `GameSession` v13, UI e SQLite v2 restano invariati.

[Dettaglio tecnico](../../design/G1_D12_E3B3C_TEMPO_UNICO_V14.md) · [Test plan](../../validation/V60_E3B3_RC2_CLOCK_TEST_PLAN.md) · [Roadmap](../../ROADMAP.md).

---

## Stato prioritario — v60-E3b3-RC1 (3 novembre 2026)

**Confermato Windows:** v60-E3b2-RC3, test passati secondo l’utente (380 attesi). **Candidato nuovo:** E3b/3a-b, sessione v14 isolata con comandi Scuola/ricerca, +7 test scritti, validazione .NET ancora pendente (target 387). I sorgenti della campagna `civil-growth-13`, UI Avalonia, persistenza v2 e benchmark restano invariati. [G1-D11](../../design/G1_D11_E3B3_SESSION_ISOLATA.md).

**Non completato:** avanzamento del tempo unico, comandi tier e upgrade della sessione, GUI v14 ed E4. I parametri economici rimangono candidati.

---

## Stato aggiornato — v60-E3b2-RC3 (3 novembre 2026)

**RC2:** test Windows `validate.cmd` tutti superati secondo l’utente. **RC3:** nuova correzione di integrità per SQLite v3 sperimentale, con conteggio persistito degli studenti in formazione e 3 test da eseguire; target 380.

Il ruleset giocabile `civil-growth-13`, lo store SQLite v2 e la GUI sono invariati. Nessuna modifica ai salvataggi personali. [Rapporto RC3](../../validation/V60_E3B2_RC3_SQLITE_INTEGRITY.md). Il prossimo sviluppo, dopo il gate RC3, è E3b/3a: sessione v14 isolata.

**Le sezioni sotto riportano lo storico delle decisioni precedenti, non lo stato corrente.**

---

## Storico — stato alla v60-E3b2-RC2 (superato dalla RC3)

**Conferma Windows:** v60-E3b-RC1, validata dall’utente. **E3b2-RC1:** 377 test, 372 superati e 5 falliti nella nuova suite SQLite v3. **E3b2-RC2:** corretti i test (ordine delle collezioni con chiave e riga di formazione inesistente), ripulita la radice; validazione Windows ancora da effettuare. La campagna `civil-growth-13`, la GUI e lo store v2 restano invariati. E3b/3 segue il gate di E3b2. [Dettaglio delle correzioni](../../validation/V60_E3B2_RC2_SQLITE_TEST_FIX.md) · [Guida aggiornata](../../START_HERE.md).

---

> **ATTIVO — G1-D9 / E3b2 (03/11/2026).** La v60-E3b-RC1 è stata confermata verde su Windows dall’utente. Implementato in isolamento `V14SqliteSaveStore` con schema v3, migrazione v13 solo in copia e backup S1. Ancora non eseguiti i test di questa milestone; UI, v13 e SQLite v2 restano invariati. [Checkpoint](../../history/checkpoints/G1_D9_CHECKPOINT.md).

> **03/11/2026 · G1-D8 / E3b/1 — NON VALIDATO:** introdotto uno snapshot candidato v14 completo e conversione v13→v14 **solo in memoria**, con controllo dello stoccaggio, Municipio, popolazione militare/civile/ricerca, 16 famiglie e storico acquisti. Nessuna modifica al gioco, a `GameSnapshot` v2 o SQLite. [Checkpoint](../../history/checkpoints/G1_D8_CHECKPOINT.md) · [Contratto](../../design/G1_D8_E3B_CONTRATTO_SNAPSHOT_MIGRAZIONI.md). v59-RC1 resta l’ultima confermata verde.

> **03/11/2026 · G1-D7 / v60-E3-RC1 (NON VALIDATA):** servizi puri E1+E2+E3 presenti: prezzi progressivi, tier 1/3/10, Scuola con training/coda, qualificati/attivi e acquisti volontari di tecnologie. Solo prototipo, nessuna nuova Scuola a catalogo, nessun salvataggio v3 e nessun collegamento alla campagna `civil-growth-13`. Test da eseguire su Windows; baseline validata v59-RC1. [Checkpoint](../../history/checkpoints/G1_D7_CHECKPOINT.md) · [Contratto](../../design/G1_D7_E3_SCUOLA_RICERCA_IMPLEMENTAZIONE_ISOLATA.md).

> **03/11/2026 · G1-D6 / v60-E2-RC1:** E1+E2 presenti nel Core **in isolamento**; livelli 1/3/10, upgrade, capacità locale e proiezione sul pianificatore storico. Nessun cambio alla campagna `civil-growth-13` o alla persistenza. Test E1/E2 da eseguire; riferimento validato v59-RC1. [Dettagli](../../design/G1_D6_E2_IMPLEMENTAZIONE_ISOLATA.md).

> **v60-E1-RC1 / G1-D5 · 03/11/2026 — CODICE NUOVO NON VALIDATO:** motore puro per quotare edifici, contatori di famiglia, pagamento atomico immutabile e saldo ricerca spendibile su richiesta; **non connessi** a GameSession, SQLite o UI. La v59-RC1 rimane l’ultima campagna con test Windows confermati. [Dettaglio](../../design/G1_D5_E1_IMPLEMENTAZIONE_ISOLATA.md).

> **G1-D4 · 03/11/2026 — PROPOSTA NON APPROVATA:** design completo dell’albero ricerche, con **8 tecnologie candidate attuabili**, 3 differite e copertura delle 16 famiglie produttive. Dati e test documentali in [G1-D4](../../design/G1_D4_ALBERO_TECNOLOGICO_E_GAMEPLAY.md). Base eseguibile v59-RC1/`civil-growth-13` invariata; navale sospeso fino a nuovo bilanciamento.

## Stato effettivo alla consegna G1-D4

**Implementato e confermato:** solo il motore v59-RC1, `civil-growth-13`, con risultato Windows `validate.cmd` positivo dichiarato dall’utente. **Proposto e NON implementato:** costruzioni a costo progressivo, livelli 1/3/10, Scuola, ricercatori, punti ricerca, albero tecnologico, snapshot v3 e `civil-growth-14`, 23 tipi-obiettivo con la Scuola. **Rinviati:** Logistica e Navigazione, e in generale tutto il sistema navale. I due JSON militari prodotti in locale non sono ancora stati forniti/analizzati; non dichiarare completato il bilanciamento della campagna.

> **G1-D3 · 03/11/2026 · PROPOSTA NON APPROVATA:** analisi quantitativa di incremento costi per famiglia, tre taglie (1/3/10), upgrade, Scuola, ricercatori e migrazioni SQLite. Nessuna modifica alla v59 eseguibile. [Contratto G1-D3](../../design/G1_ECONOMIA_RICERCA_CONTRATTO_QUANTITATIVO.md) · [ADR 0019](../../adr/0019-contratto-quantitativo-economia-ricerca-proposta.md).

# SimRisorse — stato corrente

**3 novembre 2026 · checkpoint documentale v59-G1-D1.** Baseline eseguibile **v59-RC1**, ruleset `civil-growth-13`, SQLite snapshot v2. L'utente ha confermato che **tutti i test di `validate.cmd` della v59-RC1 sono passati** sul proprio PC Windows. Il log numerico e i JSON prodotti sono ancora da esaminare. Questo checkpoint G1 non cambia sorgenti, test, simulatori né scenari: la convalida del codice rimane quella della v59-RC1.

## Componenti disponibili

- C#/.NET 10, Core/Application/Infrastructure/Avalonia Desktop/Simulation; salvataggi SQLite.
- **16 risorse**, **21 strutture** a catalogo = 20 edifici-obiettivo e Municipio iniziale non conteggiato nella vittoria. Mancano Cantiere (21º obiettivo) e Castello (22º).
- Municipio unico iniziale (+1.000 capacità per risorsa), Magazzino +500, produttori +50 ai propri output, capacità Pane uniforme, dettagli e tooltip coerenti.
- Caserma: 260 Legno lavorato + 200 Pietra, 5 Spade/recluta, un posto individuale per Caserma, 30s; coda/annullamento con rimborso entro capacità. Nessun congedo nel ruleset v13. Soldati/reclute sempre residenti e con razioni ordinarie.
- UI Edifici/Risorse/Esercito, retro 8-bit compatto; salvataggi storici con backup S1 in migrazione. Prove automatiche di migrazione superate, DB personali reali non verificati in questa sessione.
- Benchmark M5.3c: 9 profili controllati + rapporto reale della strategia militare a 8 ore; suite completa validata dall'utente. **Dati numerici non ancora esaminati.**

## Non implementato

Cantiere, navi, flotta, missioni, esplorazione, conquista, stati delle isole, Castello e vittoria. Nessuna nuova versione di ruleset `civil-growth-14` esiste ancora.

## Decisioni di progettazione

Sono orientamenti espliciti da approfondire: **esplorazione distinta dalla conquista; conquista deterministica con requisiti; nessuna perdita irreversibile inizialmente**. Non risultano approvati N-01…N-07, capacità, costi, durate, simultaneità, rientri e quantità di soldati necessari. Leggere la [scheda G1](../../design/G1_NAVAL_MISSIONS_DECISION_GATE.md) e l'[ADR 0017 proposto](../../adr/0017-naval-mission-boundaries-proposta.md).

## Prossimo incremento

Leggere `military-balance-8-hours.json` e `military-capacity-benchmark.json` prodotti dalla v59-RC1; discutere e approvare N/S; poi introdurre soltanto M5.4a mantenendo Core, factory v13 e salvataggi storici intatti.
