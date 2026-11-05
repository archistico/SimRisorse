**ADR corrente RC71:** [0079 — Planner v15 senza slot `food-store`](0079-planner-v15-senza-slot-produttore-ritirato.md). RC71 candidata (822 test attesi), RC63 ultimo checkpoint validato (771). Nessuna modifica alle regole v14, alla GUI e a SQLite generation 6. [Roadmap](../ROADMAP.md).

**ADR corrente RC70:** [0078 — Verifica semantica prima della promozione SQLite v15](0078-verifica-semantica-salvataggio-v15.md). RC70 candidata, **818 test attesi**; RC63 ultimo checkpoint validato con 771. [Stato](../CURRENT_STATE.md) · [Roadmap](../ROADMAP.md).

**ADR corrente RC69:** [0077 — Radice civica v15 nativa, SQLite generation 6 invariata](0077-radice-civica-v15-nativa.md). RC69 candidata con **814 test attesi**; RC63 ultimo checkpoint validato con 771. [Stato](../CURRENT_STATE.md) · [Roadmap](../ROADMAP.md).

## RC66 — [ADR 0074: validazione civica v15 nativa](0074-validatore-civico-v15-senza-proiezione-v14.md)

**ADR corrente RC62:** [0070 — GUI v15 predefinita con filtro delle filiere alimentari](0070-modalita-v15-predefinita-filtro-filiere.md). Supera la sola scelta opt-in dell'[ADR 0065](0065-gui-v15-opt-in-e-salvataggio-isolato.md), conserva isolamento salvataggi. [ADR 0069](0069-generazione6-senza-famiglia-produttiva-legacy.md) resta il contratto dello schema SQLite 6. RC61 760/761 test su Windows; RC62 762 attesi, da verificare. [Stato](../CURRENT_STATE.md) · [Roadmap](../ROADMAP.md).

---

**RC59:** [ADR 0068 — Stress combinato prima della rottura strutturale v15](0068-stress-combinato-filiere-e-checkpoint-legacy.md).

> **ADR corrente RC57: [0067 — Riconciliazione fisica v15 e stress opt-in](0067-riconciliazione-fisica-v15-e-stress-separato.md).** RC56 validata con 731 test, RC57 candidata con 743 attesi. [Stato](../CURRENT_STATE.md) · [Roadmap](../ROADMAP.md).

---

> **ADR corrente RC56: [0066 — Flussi reali v15, trasferimenti non contati e ETA locale](0066-flussi-v15-misurati-dal-clock-e-eta-locale.md).** RC55 validata con 713 test, RC56 candidata con 731 attesi. [Stato](../CURRENT_STATE.md) · [Roadmap](../ROADMAP.md).

> **ADR corrente: [0065 — GUI v15 opt-in e salvataggi separati](0065-gui-v15-opt-in-e-salvataggio-isolato.md).** RC54 candidata, RC53 validata con 705 test. I precedenti ADR rimangono validi come storia delle scelte.

> **Indice corrente RC53:** [ADR 0064 — Pesce e Pane possono transitare tra città e Deposito](0064-ingredienti-in-transito-citta-deposito-e-fixture-produttive.md); il Cibo pronto resta esclusivo del Deposito. [ADR 0063](0063-comandi-v15-cibo-esclusivo-e-catalogo-di-presentazione.md) resta valido per i comandi v15. RC52 695/703 test, RC53 705 attesi (non eseguiti).

> **Indice corrente RC52:** [ADR 0063 — Comandi e Cibo esclusivo del Deposito v15](0063-comandi-v15-cibo-esclusivo-e-catalogo-di-presentazione.md) · [ADR 0062 — Clock unificato](0062-clock-unico-v15-e-produttore-cibo-esclusivo.md) · [ADR 0060 — DB sperimentali](0060-salvataggi-di-sviluppo-e-checkpoint-filiera.md). [Stato](../CURRENT_STATE.md) · [Roadmap](../ROADMAP.md). I riferimenti precedenti sono storici.

> **Indice corrente RC51:** [ADR 0062 — Orologio unico e produttore di Cibo esclusivo v15](0062-clock-unico-v15-e-produttore-cibo-esclusivo.md): implementato headless, test .NET pendenti; [ADR 0061 — Edifici alimentari diretti](0061-edifici-alimentari-diretti-e-capacita-derivate.md); [ADR 0060 — Nessuna compatibilità obbligatoria dei vecchi DB](0060-salvataggi-di-sviluppo-e-checkpoint-filiera.md). [Stato](../CURRENT_STATE.md) · [Roadmap](../ROADMAP.md). Le versioni più vecchie riportate sotto sono archivio.

> **Indice corrente RC50:** [ADR 0061 — Catalogo alimentare direttamente costruibile e capacità derivate](0061-edifici-alimentari-diretti-e-capacita-derivate.md), implementazione headless D05d.3a; [ADR 0060 — Libertà di cambiare gli schemi di sviluppo](0060-salvataggi-di-sviluppo-e-checkpoint-filiera.md). [Stato](../CURRENT_STATE.md) · [Roadmap](../ROADMAP.md). Le note storiche sottostanti non prevalgono sullo stato RC50.

> **Indice corrente RC48:** [ADR 0059 — Confini delle filiere e proprietà esclusiva degli stock](0059-confini-produzione-alimentare-e-proprieta-stock.md) — tick isolato, schema di transito e nuove strutture da completare; [ADR 0058 — Migrazione v14→v15 senza overwrite](0058-migrazione-v14-v15-senza-sovrascrittura.md) — servizio isolato, verifica completa e origine intatta; [ADR 0057 — SQLite v15 autonomo](0057-sqlite-v15-separato-per-deposito-alimentare.md).  **Storico RC45-DOC01:** [ADR 0056 — progressione quasi esponenziale e squadre minime](0056-progressione-tardiva-e-squadre-minime-proposta.md) — principio approvato, coefficienti e implementazione da definire. [Stato corrente](../CURRENT_STATE.md) · [Roadmap](../ROADMAP.md). Le annotazioni storiche riportate sotto non rappresentano il gate corrente.

# ADR corrente — RC40 / F1-D01

[ADR 0050 — Deposito alimentare unificato e filiere alternative](0050-filiera-alimentare-unificata-deposito.md). Direzione approvata; solo nucleo Core isolato e test in RC40, **non gameplay attivo**. RC38 541 test confermati; RC39 545 e RC40 552 attesi, non eseguiti.

---

# ADR corrente — RC39 / E4a-D02

[ADR 0049 — Benchmark naturali con policy deterministica esplicita](0049-benchmark-naturali-politica-esplicita.md). RC38 confermata (541 test); RC39 candidata (545 attesi). Nessun nuovo valore di bilanciamento approvato e nessuna misura economica generata nel confezionamento.

---

# ADR corrente — RC36 / P1-D50

[ADR 0047 — Gate P1 e misure E4a senza rebalance](0047-gate-p1-e-misure-e4a.md) · [Rapporto](../design/P1_D50_RC36_CONSOLIDAMENTO_E_GATE_E4A.md). Stato: protocolli predisposti e test aggiunti, validazione Windows pendente.

---

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

- [ADR 0039 — Protezione checkpoint e autosalvataggio v14](0039-protezione-checkpoint-e-autosalvataggio-v14.md) — **Accettato** come contratto sicurezza, RC17 da validare Windows.

- [ADR 0057 — SQLite v15 alimentare separato](0057-sqlite-v15-separato-per-deposito-alimentare.md): schema, scorte non duplicate e protezione dei checkpoint v14; codice candidato RC46.

- [ADR 0060 — Salvataggi di sviluppo e checkpoint completo delle filiere](0060-salvataggi-di-sviluppo-e-checkpoint-filiera.md): **accettato**, RC49; sostituisce l’obbligo di migrare ciascun vecchio DB nelle ADR 0057–0059, conserva isolamento e integrità.

- [ADR 0085 — Produzione non realizzata e recupero controllato](0085-misure-produzione-non-realizzata-e-recupero-controllato.md): implementata RC77; misure dal clock reale e confronti finanziati dichiarati, separati dalle partite naturali.

- [ADR 0086 — Confronto isolato delle curve economiche](0086-confronto-curve-economiche-isolato.md): strumento RC78 verificato; parametri candidati, audit di liquidità circoscritto e squadre analitiche senza applicazione al gioco.
