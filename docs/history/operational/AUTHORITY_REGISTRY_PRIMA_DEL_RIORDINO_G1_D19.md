## Aggiornamento RC1 GUI v14 — 03/11/2026

**Gate confermato dall’utente:** RC5, 417/417 test. La nuova RC1 aggiunge una finestra `--experimental-v14` isolata, senza trasformare la modalità v13 e i suoi salvataggi; 10 test Desktop nuovi, 427 attesi, non eseguiti in questo ambiente. Il contratto G1-D16 e ADR 0031 autorizzano soltanto l'anteprima, non l'approvazione dei coefficienti economici. Accumulo libero/spesa volontaria della ricerca resta decisione vincolante. [ADR 0031](../../adr/0031-e3b4-gui-v14-opt-in.md).

---

## Aggiornamento di autorità RC5 (03/11/2026)

**Confermato Windows:** RC4 408/408. **RC5:** codice e test E3b/3e predisposti, validazione .NET pendente. ADR 0030 accettato soltanto come implementazione sperimentale nel prototipo: non attribuisce l'approvazione dei numeri +15% / 2,6 / 6,5 o di costi Scuola al giocatore. Il ruleset v13 e tutti i file persistenti restano intatti.

---

## Aggiornamento decisioni v60-E3b3-RC4 — 03/11/2026

**Fatti validati:** il gate Windows E3b3-RC3 ha superato i 396 test secondo l'utente. **Candidata E3b/3d:** implementazione isolata di costruzione e upgrade per livelli e personale nella sessione v14. Nessuna approvazione implicita dei coefficienti (1,15; 2,6; 6,5), dei costi Scuola o dell'albero tecnologico quantitativo. **Restano regole richieste:** prezzi crescenti per famiglia, possibilità di costruzione/upgrade a tre taglie e ricerca accumulata/spesa liberamente. `civil-growth-13`/SQLite v2/Desktop non cambiati. [ADR tecnico](../../adr/0029-e3b3d-comandi-tier-sessione-v14-isolati.md).

---

## Aggiornamento tecnico E3b/3c — 3 novembre 2026

E3b3-RC1 validata da `validate.cmd` secondo l'utente. RC2 aggiunge un orologio unico **soltanto** nel Core v14, secondo ADR 0028 sperimentale. Nessuna approvazione dei prezzi, della frequenza di ricerca o dei requisiti tecnologie; restano approvate soltanto conoscenza accumulabile liberamente e spesa volontaria. Nessuna modifica ai file di salvataggio v13 o alla GUI.

---

## Aggiornamento tecnico E3b/3 — 3 novembre 2026

L’utente ha confermato **tutti i test della v60-E3b2-RC3 passati su Windows**. La nuova candidata E3b3-RC1 fornisce una sessione v14 isolata e transizioni atomiche per Scuola e tecnologie (ADR 0027 **sperimentale**); test .NET RC1 non ancora eseguiti. **Non approva** coefficienti, tempi o costi: la sola decisione sulla conoscenza rimane accumulo libero e spesa volontaria. `civil-growth-13` resta la partita giocabile.

---

## Aggiornamento tecnico RC3 — 3 novembre 2026

**Ultimo gate automatico validato:** v60-E3b2-RC2 secondo l’utente. Nella RC3 viene adottata, **solo nel ramo sperimentale**, la difesa SQLite v3 del conteggio indipendente degli studenti in formazione ([ADR 0026](../../adr/0026-e3b2-integrita-numero-studenti-sqlite-v3.md)); richiede validazione Windows. Restano approvati i punti ricerca liberi e la spesa volontaria; i coefficienti economici e gli effetti delle ricerche sono ancora candidati. La campagna `civil-growth-13` non cambia.

---

## Storico — stato alla v60-E3b2-RC2 (superato dalla RC3)

**Conferma Windows:** v60-E3b-RC1, validata dall’utente. **E3b2-RC1:** 377 test, 372 superati e 5 falliti nella nuova suite SQLite v3. **E3b2-RC2:** corretti i test (ordine delle collezioni con chiave e riga di formazione inesistente), ripulita la radice; validazione Windows ancora da effettuare. La campagna `civil-growth-13`, la GUI e lo store v2 restano invariati. E3b/3 segue il gate di E3b2. [Dettaglio delle correzioni](../../validation/V60_E3B2_RC2_SQLITE_TEST_FIX.md) · [Guida aggiornata](../../START_HERE.md).

---

**Nota decisionale:** i parametri numerici dei prezzi progressivi, livelli e ricerca non sono ancora approvati; saldo ricerca libero e spesa volontaria sono già approvati. Non utilizzare la pulizia dei file come approvazione del contratto economico.

> **G1-D9 / E3b/2:** v60-E3b-RC1 ha superato `validate.cmd` secondo l’utente. La candidata successiva introduce `V14SqliteSaveStore` **isolato** con formato v3 e protezione backup S1; NON modifica la v13. L’ADR 0025 e i parametri economici restano proposti; nessuna attivazione nel gioco. [Checkpoint](../checkpoints/G1_D9_CHECKPOINT.md).

> **Aggiornamento tecnico G1-D8:** l’identificativo `civil-growth-14` è ora riservato alla **candidata economia con ricerca e livelli**, non alla precedente ipotesi navale. Il prototipo in memoria non equivale ad approvazione né attivazione. Restano approvati soltanto i punti ricerca liberi/spendibili volontariamente; prezzi, taglie, Scuola e albero non sono numeri definitivi. [ADR 0024](../../adr/0024-v14-snapshot-migration-boundaries-proposta.md).

> **G1-D7 / 03-11-2026:** l’unico nuovo vincolo accettato in questa fase resta **ricerca accumulabile liberamente con spesa volontaria**, ADR 0021. Albero (8 tecnologie attuabili nel prototipo, 3 differite), durata Scuola, capacità e costi, rimborso studenti e ritorno civile sono **proposte non approvate**; E1/E2/E3 non attivi nella v13. [ADR 0023](../../adr/0023-e3-school-technology-ledger-isolated-prototype.md) · [Checkpoint](../checkpoints/G1_D7_CHECKPOINT.md).

> **G1-D6:** confermata come decisione dell’utente soltanto la libera accumulazione/spesa dei punti ricerca (ADR 0021). Parametri economici, tier/sblocchi, Scuola e regola di vittoria rimangono **candidati**; E2 è codice isolato non approvazione implicita. [ADR 0022](../../adr/0022-e2-tiered-productive-buildings-prototype.md).

> **Decisione nuova del 03/11/2026 (ADR 0021 ACCETTATO):** la ricerca produce punti liberi, conservati e spendibili a scelta. Costi dei tier, coefficienti, Scuola, albero e migrazioni NON sono approvati. La v60-E1-RC1 contiene modelli Core isolati, non nuove regole di gioco. [Checkpoint](../checkpoints/G1_D5_CHECKPOINT.md).

> **G1-D4 · 03/11/2026 — PROPOSTA NON APPROVATA:** design completo dell’albero ricerche, con **8 tecnologie candidate attuabili**, 3 differite e copertura delle 16 famiglie produttive. Dati e test documentali in [G1-D4](../../design/G1_D4_ALBERO_TECNOLOGICO_E_GAMEPLAY.md). Base eseguibile v59-RC1/`civil-growth-13` invariata; navale sospeso fino a nuovo bilanciamento.

## Decisioni in preparazione — G1-D4

- **Preferenze espresse dall’utente (vincolanti per la progettazione):** costi crescenti per famiglia, costruzione diretta e upgrade dei tier, Scuola che forma ricercatori dalla popolazione e genera conoscenza. Non scambiare queste preferenze con l’approvazione di coefficienti numerici, formula o salvataggi.
- **Numeri e semantica da approvare:** 15% di crescita; tier 1/3/10 e moltiplicatori 1/2,6/6,5; costi/tempi Scuola; saldo globale ricerca; otto ricerche settoriali; conversione e 23º obiettivo.
- **ADR ancora proposti:** [0018](../../adr/0018-progressive-construction-tiers-and-research-proposta.md), [0019](../../adr/0019-contratto-quantitativo-economia-ricerca-proposta.md), [0020](../../adr/0020-tech-tree-sector-research-proposta.md). Nessun contenuto navale/tecnologico implementato.

> **G1-D3 · 03/11/2026 · PROPOSTA NON APPROVATA:** analisi quantitativa di incremento costi per famiglia, tre taglie (1/3/10), upgrade, Scuola, ricercatori e migrazioni SQLite. Nessuna modifica alla v59 eseguibile. [Contratto G1-D3](../../design/G1_ECONOMIA_RICERCA_CONTRATTO_QUANTITATIVO.md) · [ADR 0019](../../adr/0019-contratto-quantitativo-economia-ricerca-proposta.md).

# SimRisorse — registro decisioni attivo (v59-G1-D1)

**3 novembre 2026.** Questo registro distingue le regole adottate dalle proposte non ratificate. In caso di contraddizione con dossier v55 e documenti storici, prevalgono le decisioni qui registrate e il codice v59-RC1 verificato.

1. **Baseline software confermata:** `civil-growth-13`, 16 risorse, 20 edifici-obiettivo implementati + Municipio fisso extra. Dopo Cantiere e Castello: 22 obiettivi + Municipio = 23 strutture. La vittoria richiede tutti e 22 i tipi e la conquista obbligatoria.
2. **Economia e stoccaggio vincolanti:** Municipio unico, +1000 a ogni risorsa; Magazzino +500 per risorsa, produttori +50 agli output, Pane con regola uniforme e capacità condivisa. Non cambiare prezzi e razioni senza gate e decisione.
3. **Militari v13 in codice:** Caserma 260 Legno lavorato + 200 Pietra; recluta 5 Spade; 1 posto per Caserma; 30 s ciascuna; annullamento coda/formazione con rimborso nei limiti di capacità; congedo non disponibile. Soldati/reclute parte della popolazione residente, consumano razioni ordinarie.
4. **Salvataggi:** snapshot SQLite v2; migrazioni legacy/fork v41 come ADR 0015; backup prima della prima sovrascrittura. Prove su database reali personali non ancora effettuate.
5. **Gate v59:** tutti i test `validate.cmd` **dichiarati superati dall'utente**. Ciò conferma la validazione automatica del codice v59, **non** i valori di scorte dopo otto ore, che devono essere letti dai due report JSON. ADR 0016 è adottato come metodo di misura.
6. **Orientamenti di gameplay:** esplorazione separata dalla conquista, successo deterministico con requisiti e assenza di perdite irreversibili inizialmente sono **base di discussione** espressa dall'utente; NON costituiscono approvazione di prezzi/quantitativi/funzionalità dettagliate.
7. **G1 ancora in proposta:** N-01…N-07 e S-01…S-10 elencati in [G1_NAVAL_MISSIONS_DECISION_GATE](../../design/G1_NAVAL_MISSIONS_DECISION_GATE.md). ADR 0017 è **PROPOSTO**. Contingente minimo prima conquista, tempi, ricompense, simultaneità e ritorni sono esplicitamente aperti.
8. **Primo futuro ruleset:** `civil-growth-14` è soltanto un identificativo candidato per M5.4, non un catalogo presente né un gate passato. L'utente deve approvare esplicitamente il contratto prima di implementare il Cantiere.
9. **Consegne:** ogni avanzamento di codice fornisce ZIP integrale, test verificati/pendenti, milestone, ADR e roadmap. Un aggiornamento documentale non deve essere presentato come nuova versione funzionante.

Fonti attive: [CURRENT_STATE](../../CURRENT_STATE.md), [ROADMAP](../../ROADMAP.md), [G1 decision gate](../../design/G1_NAVAL_MISSIONS_DECISION_GATE.md), [ADR 0017](../../adr/0017-naval-mission-boundaries-proposta.md).

## Decisione grafica G1-D17 / ADR 0032 (2026-11-03)

**Autoritativa:** il giocatore richiede espressamente l'interfaccia v14 nello stile e nella struttura del gioco originale, con Scuola e Albero tecnologico **nel menu sinistro**, nessuna ComboBox di taglia nei dettagli e ogni livello come **edificio direttamente selezionabile/costruibile**. I valori di bilanciamento rimangono proposte non approvate. Vedi [ADR 0032](../../adr/0032-gui-v14-identica-navigazione-classica-tier-come-edifici.md).
