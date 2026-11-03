> **G1-D24 / RC8:** [ADR 0034 — Stati Edifici, comandi separati e Scuola](0034-v14-stati-edifici-e-layout-azioni.md), requisito UX accettato; test Windows 456 attesi, non eseguiti.

> **Ultimo ADR tecnico sperimentale:** [0030 — Civili e replay v14](0030-e3b3e-edifici-civili-replay-v14.md). Coefficienti e soglie numeriche restano candidati.

> **G1-D9 / E3b/2:** nuovo [ADR 0025 — persistenza SQLite v3 isolata e copy-on-write](0025-e3b2-sqlite-v3-copy-on-write-isolato.md), PROPOSTO e implementato in ramo sperimentale non validato. Ultimo gate Windows positivo: v60-E3b-RC1.

> **ADR 0021 — ACCETTATO (03/11/2026):** [Saldo ricerca libero e spesa volontaria](0021-ricerca-saldo-libero-spesa-su-scelta.md). La decisione approva la **modalità** della ricerca, non i costi delle tecnologie né l’intero ADR 0020, che resta proposto. Il codice v60-E1-RC1 è ancora isolato dalla campagna e non validato su Windows.

> **G1-D4 · Nuovo [ADR 0020 — PROPOSTO](0020-tech-tree-sector-research-proposta.md):** ricerche settoriali con punti spendibili, Scuola e prerequisiti. Non modifica gli ADR accettati né abilita ancora una nuova campagna.

> **Indice attivo al 03/11/2026:** v59-RC1 validata da `validate.cmd` secondo l'utente. Aggiunto [ADR 0017 — PROPOSTO, NON ACCETTATO](0017-naval-mission-boundaries-proposta.md) per il contratto G1. L'ADR 0016 è implementato e validato come strumentazione; restano da esaminare i JSON della simulazione reale. Le note v58/v57 sottostanti sono storiche.

> **Indice aggiornato alla v59-RC1.** Il gate `validate.cmd` della v58-RC5 è stato dichiarato superato dall’utente; per la nuova RC1 non abbiamo ancora risultati Windows.

> **Aggiornamento 03/11/2026 · v58-RC2 (non validata):** questo documento contiene materiale del precedente checkpoint v57 e alcune parti sono storiche. Per lo stato attuale del ramo unificato leggere [docs/CURRENT_STATE.md](../CURRENT_STATE.md), [docs/ROADMAP.md](../ROADMAP.md) e [docs/validation/V58_RECONCILIATION.md](../validation/V58_RECONCILIATION.md). Nuovo ruleset candidato: `civil-growth-13` (addestramento individuale); flotta non implementata.

# Registro delle decisioni architetturali

Gli ADR documentano una decisione, le alternative considerate e le conseguenze. La data iniziale del registro è il 2026-10-26. Lo stato di un ADR non indica che la relativa funzionalità sia già implementata.

## Stati

- **Proposto**: scelta candidata, ancora da validare o confermare.
- **Accettato**: vincolo adottato per lo sviluppo successivo; l'implementazione può essere ancora assente.
- **Superato**: sostituito da un nuovo ADR, che deve essere collegato al precedente.
- **Respinto**: proposta valutata e non adottata, conservata con la motivazione.

Le preferenze del prodotto espresse dall'utente sono distinte dalle scelte tecniche di impostazione. Le regole di gioco ancora aperte sono in [Game design](../GAME_DESIGN.md), senza trasformarle implicitamente in decisioni attraverso un ADR.

## Indice

| ADR | Titolo | Stato | Implementazione |
| --- | --- | --- | --- |
| [0001](0001-modular-monolith-and-vertical-features.md) | Monolite modulare e funzionalità verticali | Accettato | Core, Application, Infrastructure, Desktop e Simulation implementati |
| [0002](0002-simulation-time-and-pause.md) | Tempo di simulazione, pausa e riproducibilità | Accettato | Core e adattatore monotono desktop verificati |
| [0003](0003-versioned-content-catalog.md) | Catalogo versionato e stato separato dalle definizioni | Accettato | Catalogo immutabile e validato in memoria, con analisi di raggiungibilità degli sblocchi; formato esterno futuro |
| [0004](0004-population-and-military-boundaries.md) | Popolazione e progressione militare come modelli espliciti | Accettato | Assegnazioni e crescita civile implementate; militari futuri |
| [0005](0005-desktop-stack-candidate.md) | C#/.NET, Avalonia e SQLite per il primo desktop | Accettato | Desktop definitivo e adattatore SQLite collegati in M3 |
| [0006](0006-economic-step-and-resource-precision.md) | Precisione delle risorse e prenotazioni nel passo economico | Accettato | Implementato e verificato in M2 |
| [0007](0007-desktop-persistence-and-lifecycle.md) | Desktop, salvataggi e ciclo di vita della partita | Accettato | Implementato e verificato in M3 |
| [0008](0008-avalonia-12.md) | Avalonia 12 per il desktop | Accettato | Avalonia 12.1.4, xUnit 3 headless e provider SQLite aggiornati e verificati |
| [0009](0009-civil-growth-and-save-generations.md) | Crescita civile e generazioni di salvataggio | Accettato | Primo scenario civile, snapshot/file v2 e compatibilità v1 verificati |
| [0010](0010-storage-capacity-and-rules-conversion.md) | Capacità di magazzino e conversione esplicita delle regole | Accettato | Magazzino, `civil-growth-2`, conversione da `civil-growth-1` e copia di sicurezza verificati |
| [0011](0011-advanced-housing-and-bread-rations.md) | Pane separato e razione degli abitanti avanzati | Accettato | `civil-growth-4` implementato, gate Windows e benchmark pendenti |
| [0012](0012-retro-8bit-desktop-visual-language.md) | Identità visiva retro 8-bit essenziale | Accettato | Progettazione soltanto; futura GUI pixel art non ancora implementata |
| [0013](0013-accelerated-housing-and-fractional-growth.md) | Case che accelerano gli arrivi, frazioni e compatibilità SQLite v2 | Accettato | `civil-growth-5` implementato; gate Windows v24 da eseguire |
| [0014](0014-first-iron-industry-and-civil-save-upgrade.md) | Prima industria del ferro, regole v6 e continuità S1 | Accettato | M5.1a verificata: 191 test, conversione v5→v6 e backup, benchmark v4/v5 preservati |
| [0015](0015-individual-military-training-branch-reconciliation.md) | Recupero del ramo v41 e addestramento individuale v13 | Accettato come base del ramo v13 | RC5 validata dall’utente, DB reali da controllare su copie |
| [0016](0016-military-benchmark-separate-controlled-from-campaign.md) | Separare benchmark addestramento controllato e campagna reale | Accettato (metodo) | v59-RC1, test v59-RC1 superati; lettura valori pendente |
| [0024](0024-v14-snapshot-migration-boundaries-proposta.md) | Snapshot v14 e migrazione v13→v14 | **PROPOSTO** | Core isolato testato su Windows nella v60-E3b-RC1 |
| [0025](0025-e3b2-sqlite-v3-copy-on-write-isolato.md) | SQLite v3 copy-on-write e backup S1 | **PROPOSTO** | E3b/2, Infrastructure isolata; test .NET pendenti |
| [0023](0023-e3-school-technology-ledger-isolated-prototype.md) | E3, Scuola e tecnologie come prototipo Core isolato | Proposto (numeri; prototipo) | E3a Core/test scritti, non validati; v13 invariata |
| [0017](0017-naval-mission-boundaries-proposta.md) | Flotta persistente ed esplorazione/conquista separate | **PROPOSTO** | G1 solo documentazione; nessun codice navale |

## Come aggiungere una decisione

1. Copiare il [modello](TEMPLATE.md), assegnando il successivo numero a quattro cifre.
2. Descrivere il problema concreto e le alternative realistiche.
3. Specificare cosa viene deciso e cosa resta aperto.
4. Collegare i documenti e le milestone interessate.
5. Aggiornare questo indice; aggiornare roadmap e handoff se cambia il lavoro successivo.

Una decisione accettata non si riscrive per nascondere un cambio di direzione. Si crea un nuovo ADR e si marca il precedente come superato. Le correzioni editoriali sono consentite.


| [0018](0018-progressive-construction-tiers-and-research-proposta.md) | Costi progressivi, tier e Scuola | **PROPOSTO** | Nessuna implementazione; design G1-D4 |

| [0019](0019-contratto-quantitativo-economia-ricerca-proposta.md) | Contratto quantitativo economia e migrazioni | **PROPOSTO** | Nessuna implementazione; design G1-D4 |

| [0020](0020-tech-tree-sector-research-proposta.md) | Ricerche settoriali e punti spendibili | **PROPOSTO** | Nessuna implementazione; design G1-D4 |

- [ADR 0033 — Preservazione funzionale GUI v13 → v14](0033-preservazione-funzionale-gui-v13-v14.md)
- [ADR 0034 — Stati Edifici e layout delle azioni v14](0034-v14-stati-edifici-e-layout-azioni.md)

- [ADR 0038 — Roadmap in sei fasi e tooltip capacità reale](0038-roadmap-gate-gui-e-audit-matite.md) — **Accettato**, RC13 con test Windows pendenti.
