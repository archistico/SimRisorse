# Milestone E4a-D01-F1 / RC38 — Correzione test dopo gate parziale (2026-11-04)

**RC37 su Windows: 540/541 test PASS**; il test d'integrazione SQLite confrontava impropriamente l'ordine degli edifici, non la loro identità. RC38 modifica solo le asserzioni della prova end-to-end, confrontando gli insiemi identificati per chiave. **Gate RC38 ancora aperto: 541/541 attesi.** Fino alla validazione non considerare conclusa P1-D50. Il benchmark E4a-D01 è implementato ma non misurato; gli scenari E4A-06/07 rimangono `not-run`.

**Sequenza immediata:** 1) validazione RC38; 2) due benchmark controllati indipendenti con output CSV/JSON; 3) E4a-D02 progressione naturale; 4) D03 contesa Carbone e deposito; 5) D04 riproducibilità e analisi; 6) E4b bilanciamento soltanto dopo evidenze e approvazione.

[Rapporto F1](design/E4A_D01_RC38_FIX_ORDERING_SQLITE.md) · [Piano Windows](validation/E4A_D01_RC38_TEST_PLAN.md).

---

# Avanzamento E4a-D01 / RC37 — Misure prima del bilanciamento

**RC37 implementata ma non validata su Windows.** Il runner E4a esegue E4A-06/07 come scenari controllati reali nella v14, con fixture esplicita, progressi e CSV/JSON. **541 test attesi**, ultima baseline confermata RC30 (511), nessun risultato di benchmark registrato e gate P1-D50 ancora pendente. Non modificare prezzi, coefficienti o ricette prima di E4b e dell'approvazione.

| Milestone | Stato | Consegna/criterio |
| --- | --- | --- |
| **E4a-D01 / RC37** | Implementata, da testare | Runner 1 vs 2 Scuole, 3 test, output misurabili, 30 s/campione |
| **E4a-D02** | Prossima | E4A-01…05: strategie naturali ripetibili, log dei blocchi e timeout senza fondi artificiali |
| **E4a-D03** | Pianificata | E4A-08/09: competizione Carbone, deposito pieno e inattività reale |
| **E4a-D04** | Pianificata | E4A-10: salvataggio/ripresa SQLite, aggregazione risultati e rapporto dei colli di bottiglia |
| **E4b** | Dopo misure + approvazione | Proporre cambi di costi/consumi/produzione senza applicarli prima della decisione |
| **E4c → M5.4–M7** | Future | Consolidamento, poi navale/esplorazione/conquista |

**Gate:** `validate.cmd` cumulativo (541 test attesi), due run distinte del benchmark con esito e serie CSV/JSON, verifica delle fixture e hash build, nessuna regressione nei salvataggi. [Rapporto E4a-D01](design/E4A_D01_RC37_BENCHMARK_SCUOLE.md) · [Piano di validazione](validation/E4A_D01_RC37_TEST_PLAN.md) · [ADR 0048](adr/0048-benchmark-scuole-controllato.md).

---

# Aggiornamento operativo — P1-D50 / RC36

**P1-D50 implementata, ma gate non ancora superato.** Consolidati i contratti Scuola/Matite/Ricerca/tier; due test supplementari di persistenza SQLite v14, **538 test cumulativi attesi**. RC30 (511) rimane l'ultimo gate Windows confermato. RC31–RC36 non validate cumulativamente in questo ambiente. GUI v14 ordinaria, v13 legacy con file separati.

| Fase | Stato | Uscita / dipendenza |
| --- | --- | --- |
| P1-D46…D49 | Implementate, test Windows pendenti | Formazione, Matite, tecnologie, end-to-end |
| **P1-D50 / RC36** | **Implementata, da validare** | **538 test e controllo Scuola/SQLite; non dichiarare P1 chiusa prima del gate** |
| **E4a** | **Protocollo e manifest pronti; nessuna misurazione effettuata** | Benchmark natural/controlled, tempi di progressione, colli di bottiglia |
| E4b | Successiva a E4a e approvazione esplicita | Scegliere e modificare i coefficienti |
| E4c | Dopo E4b | Stabilizzazione e migrazioni sicure |
| M5.4–M7 | Future | Navale, conquista e obiettivo finale |

[Rapporto D50](design/P1_D50_RC36_CONSOLIDAMENTO_E_GATE_E4A.md) · [Piano test](validation/P1_D50_RC36_TEST_PLAN.md) · [Protocollo E4a](design/E4A_PROTOCOLLO_BENCHMARK.md) · [ADR 0047](adr/0047-gate-p1-e-misure-e4a.md)

---

# Aggiornamento operativo — P1-D49 / RC35

**Implementati i test di integrazione, validazione Windows pendente.** Tre scenari sulla stessa campagna v14 attraversano Matite, studenti, qualifica, assegnazione, punti ricerca, acquisto volontario, livello Grande, salvataggi/ripresa; coprono anche pausa e fallimenti atomici. **536 test cumulativi attesi**. RC30 (511) rimane l'ultima suite confermata; RC31–RC35 non sono state eseguite qui. Nessun coefficiente o comportamento di gameplay modificato.

| Milestone | Stato | Uscita prevista |
| --- | --- | --- |
| P1-D46 / RC32 | Implementata, test pendenti | Formazione individuale |
| P1-D47 / RC33 | Implementata, test pendenti | Filiera Matite e atomicità |
| P1-D48 / RC34 | Implementata, test pendenti | Tecnologia → effetto effettivo |
| **P1-D49 / RC35** | **Test integrati aggiunti, non eseguiti** | **536 test e prova di ripresa SQLite** |
| **P1-D50** | **Prossima** | **Gate cumulativo P1; rapporto invarianti e decisioni candidate per E4a** |
| E4a → E4b → E4c | Successive | Misurare → approvare coefficienti → consolidare |
| M5.4–M7 | Successive | Navi, esplorazione, conquista e completamento |

[Rapporto D49](design/P1_D49_RC35_INTEGRAZIONE_PROGRESSIVA.md) · [Piano test RC35](validation/P1_D49_RC35_TEST_PLAN.md) · [ADR 0046](adr/0046-catena-integrata-scuola-ricerca-tier.md)

---

# Aggiornamento operativo — P1-D48 / RC34

**Implementata, validazione Windows pendente.** Catalogo tecnologico controllato contro i benefici reali di 16 famiglie e relativi tier; dettaglio Albero tecnologico con edifici Grande/Avanzato effettivamente sbloccati. Logistica/Navigazione ancora non acquistabili. **9 nuovi casi, 533 attesi cumulativi**; RC30 ultimo checkpoint confermato (511), RC31–RC34 candidate.

**Sequenza:** P1-D49 integrazione Scuola/Matite/ricerca/tier/salvataggi → P1-D50 gate P1 → E4a analisi quantitativa → E4b bilanciamento con approvazione → E4c consolidamento → M5.4–M7. Nessun valore economico modificato. [Rapporto D48](design/P1_D48_RC34_ALBERO_TECNOLOGICO.md) · [Piano test RC34](validation/P1_D48_RC34_TEST_PLAN.md) · [ADR 0045](adr/0045-tecnologie-effetti-verificati.md).

---

# Aggiornamento operativo — P1-D47 / RC33

**P1-D47 implementata, validazione Windows pendente.** Unico controllo per produzione Matite, suggerimenti quantitativi +1/+5/+10, test per lotti atomici e rimborsi misti. Nessuna modifica ai coefficienti economici o ai salvataggi. Ultima baseline confermata: RC30, 511 test. RC31–RC33: **524 test cumulativi attesi**.

| Milestone | Stato | Prossimo gate |
| --- | --- | --- |
| P1-D46 / RC32 | Implementata, non testata su Windows | Avanzamenti individuali Scuola |
| **P1-D47 / RC33** | **Implementata, non testata** | **524 test, prova GUI/stock/rimborso** |
| P1-D48 | Prossima | Catalogo tecnologie, prerequisiti ed effetti effettivi |
| P1-D49 | Pianificata | Integrazione end-to-end e salvataggi |
| P1-D50 | Pianificata | Gate P1 e dossier E4a |
| E4a → E4b → E4c → M5.4–M7 | Successive | Misure → valori approvati → stabilizzazione → navale |

**Decisione rimandata:** produzione automatica Matite o edificio dedicato. Conservare intanto la produzione manuale nella Scuola; non modificare la ricetta 1 Legno lavorato + 1 Carbone → 4 Matite, né i 2 pezzi minimi per studente. [Rapporto P1-D47](design/P1_D47_RC33_FILIERA_MATITE.md) · [Piano test](validation/P1_D47_RC33_TEST_PLAN.md) · [ADR 0044](adr/0044-unica-verifica-laboratorio-matite.md).

---

# Aggiornamento operativo — P1-D46 / RC32

**G1-D45:** la v14 e' stata approvata dall'utente come GUI ordinaria ed e' predefinita nella RC31; v13 rimane in modalita' legacy. La raccolta delle 44 prove P0 formali e' storica e non e' piu' un blocco concordato per P1. RC30: checkpoint Windows dichiarato verde (511 test previsti). RC31 (515) e RC32 (518) richiedono test Windows cumulativi.

| Milestone | Stato | Oggetto | Gate |
| --- | --- | --- | --- |
| **P1-D46 / RC32** | Implementata, test Windows pendenti | Avanzamenti individuali Scuola; 3 test | 518/518, prova UI e compatibilita' salvataggi |
| **P1-D47** | Prossima | Filiera Matite, input e stoccaggio, audit rimborsi | Scelte quantita' solo dopo verifica; test deterministici |
| **P1-D48** | Pianificata | Catalogo ricerca, prerequisiti ed effetti reali | Nessun acquisto senza effetto verificato |
| **P1-D49** | Pianificata | Flussi end-to-end e salvataggi | Restore e avanzamento coerenti |
| **P1-D50** | Pianificata | Gate P1 / dossier E4a | Suite verde e scenari approvati |
| **E4a → E4b → E4c** | Future | Analisi → bilanciamento approvato → consolidamento | Benchmark e backup |
| **M5.4–M7** | Future | Navi, esplorazione, conquista, obiettivo | Gate modulari |

[Rapporto D46](design/P1_D46_RC32_CICLO_FORMAZIONE_SCUOLA.md) · [Piano RC32](validation/P1_D46_RC32_TEST_PLAN.md)

---

# SimRisorse — RC31 / G1-D45: v14 predefinita (candidata)

**Aggiornamento 04/11/2026:** la **RC30** ha superato i test secondo la conferma dell'utente (**511 test previsti**). La GUI v14 è stata dichiarata soddisfacente e diventa l'interfaccia ordinaria; lo storico dossier di **44 prove visive non acquisite** resta disponibile ma non è più prerequisito per avviare P1. **RC31** aggiunge quattro test di routing (515 attesi) e deve ancora essere compilata/testata su Windows.

Avvio: `.\run-game.cmd` → v14, sullo **stesso** slot SQLite v14 già usato (`%LOCALAPPDATA%\SimRisorse\experiments\civil-growth-v14.sqlite3`). `.\run-game.cmd --legacy-v13` → v13, DB storico separato. `--experimental-v14` funziona ancora come alias. `--scenario ...` e `--smoke-test` rimangono legacy per compatibilità. Nessuna migrazione automatica né modifica al bilanciamento.

**Prossima milestone:** P1-D46 audit ciclo Scuola; D47 Matite; D48 tecnologie; D49 integrazione; D50 gate. Quindi E4a → E4b → E4c → M5.4–M7. [ADR 0042](adr/0042-promozione-v14-e-conservazione-slot.md) · [Rapporto D45](design/G1_D45_RC31_PROMOZIONE_V14.md) · [Piano RC31](validation/G1_D45_RC31_TEST_PLAN.md).

---

## Storico della roadmap precedente (superato dalla decisione RC31)

# Avanzamento G1-D44 — RC30, qualità delle evidenze prima delle correzioni

La milestone D43 richiede ancora 44 prove visive reali (22 UI-ID × 2 viewport): **0/44 acquisite in questa sessione**. RC29 aveva due falsi positivi nello strumento di raccolta: PNG presenti ma corrotti e 44 `NA` considerabili completi. RC30 risolve entrambi e introduce `Verify`, oltre a un auto-test PowerShell locale. È un **incremento di qualità degli strumenti P0**, non la chiusura delle anomalie visive D44.

**Stato gate:** baseline RC27 507 test confermati; RC28–RC30 511 test xUnit attesi da validare, auto-test PowerShell da eseguire a parte, P0 visivo aperto. [Rapporto D44](design/G1_D44_RC30_RAFFERZAMENTO_EVIDENZE_P0.md) · [Piano test](validation/G1_D44_RC30_TEST_PLAN.md). **Prossimo lavoro condizionato dalle evidenze:** validare RC30 su Windows, completare D43, trattare in D44 solo i FAIL confermati e poi D45; P1/E4 restano sospesi.

---

# G1-D43 / RC29 — Preparazione delle evidenze visive P0

**Stato operativo (04/11/2026).** RC27 è l'ultimo checkpoint applicativo confermato (**507 test**). RC28 è candidata con **511 test attesi**, non ancora verificati su Windows; RC29 **non aggiunge test .NET e non modifica l'applicazione**. Introduce soltanto il registratore locale per le evidenze visive, il report HTML e la documentazione del confronto. **Nessuna delle 44 prove UI-01…UI-22 × 2 dimensioni è stata eseguita in questa consegna; P0 rimane APERTO.**

**D43:** usare [il confronto statico verificato](design/G1_D43_RC29_AUDIT_VISIVO.md), [la procedura per acquisire le prove](validation/G1_D43_RC29_PIANO_EVIDENZE.md) e `scripts/p0-ui-evidence.ps1`. `-Action Init` crea 44 righe in `.local-cache/p0-ui-evidence/results.csv` senza sovrascrivere risultati già presenti; `-Action Report` genera il confronto HTML affiancato e segnala le evidenze mancanti. Le immagini sono acquisite solo per scelta esplicita dell'operatore e **non entrano nello ZIP**. La cattura Clipboard è facoltativa.

**Prossimi gate:** quando Windows è disponibile verificare RC28/RC29 con `./validate.cmd` e acquisire prove v13/v14 a 1040×740 e 1240×850. D44 solo difetti dimostrati, D45 chiusura P0 subordinata a 22 voci documentate, suite verde e approvazione visiva. Nessun E4 e nessuna modifica ai salvataggi.

---

# SimRisorse — Roadmap operativa dettagliata dopo RC27

**Roadmap operativa:** G1-D42 / 04-11-2026. **Baseline Windows confermata:** RC27 / G1-D41, 507 test. **Candidato di sviluppo:** RC28 / G1-D42, +4 esecuzioni headless, **511 test attesi**, ancora da eseguire su Windows. Sono stati modificati soltanto gli stili focus della finestra v14; nessuna regola economica cambiata. **P0 grafico: APERTO** finché non sono raccolte le prove manuali sulle 22 voci UI-01…UI-22.

Questa è la **roadmap canonica da usare nei prossimi incrementi**. La precedente, con tutti i checkpoint D29–D41, è [archiviata senza perdita di informazioni](history/operational/ROADMAP_G1_D41_RC27_ARCHIVE.md). Per le verifiche P0 seguire la [matrice delle 22 voci](validation/G1_D39_P0_UI_MATRIX.md) e il [runbook G1-D42](validation/G1_D42_P0_RUNBOOK.md).

## 1. Obiettivo, ordine e criteri generali

Realizzare un gestionale di risorse 2D in C#/.NET 10 con GUI desktop pixel art classica, economia coerente, salvataggi protetti e progressione civile, tecnologica e successivamente militare/navale. Ordine **vincolante**:

1. **G1 / P0: parità funzionale e visiva GUI v13→v14** (D42–D45 proposti).
2. **P1: completamento qualitativo Scuola e Albero tecnologico** (D46–D50 proposti).
3. **E4a: analisi economica con benchmark riproducibili** (nessun rebalance).
4. **E4b: proposte, approvazione e attuazione del bilanciamento**.
5. **E4c: consolidamento salvataggi, migrazione opt-in e stabilità v14**.
6. **M5.4–M7: navi, esplorazione, conquista, territori, obiettivo finale e distribuzione**.

Gli identificativi D42–D50 sono **identificativi incrementali di lavoro, non scadenze**. Una fase può richiedere più release; il numero di test futuro non è predeterminato. Ogni gate fallito blocca la promozione alla fase successiva; se non è possibile testare su Windows si può proseguire con analisi, documentazione e test aggiunti, ma non dichiarare il gate approvato.

**Invarianti non negoziabili:** GUI v13 e relativi salvataggi intatti; sette colonne Risorse, ordinamento italiano, hover ocra senza spostamento, edifici bloccati grigi ma selezionabili; edifici Base/Grande/Avanzato distinti (nessuna ComboBox dei livelli); due righe Costruisci +1/+5/+10 e Lavoratore −1/+1/+5/+10; Scuola costruita da Edifici, senza tasto Costruisci nella scheda; almeno due Matite per studente; v14 predefinita con DB separato, v13 accessibile via --legacy-v13; Scuola, militari e lavoratori senza duplicazione di abitanti; nessuna perdita irreversibile introdotta senza approvazione.

## 2. Sequenza delle milestone e gate

| Milestone | Priorità | Risultato da produrre | Dipende da | Gate di uscita |
| --- | --- | --- | --- | --- |
| **G1-D42** | P0 | Focus list/menu v14 +4 test headless, rapporto e check-list; TAB completo ancora da verificare su Windows | RC27 verde | RC28 test Windows superati + controlli focus annotati, senza redesign |
| **G1-D43** | P0 | Esecuzione guidata delle 22 prove visive a 1040×740 e 1240×850 | D42 | Evidenze e PASS/FAIL/NA motivata per ogni UI-ID |
| **G1-D44** | P0 | Correzione ordinata delle sole anomalie P0 confermate, con regressioni | D43 | Nessun FAIL P0 non risolto; suite automatica verde |
| **G1-D45** | P0 | Audit conclusivo e firma del gate P0 | D44 | 22/22 esiti documentati, screenshot, smoke e salvataggi protetti |
| **P1-D46** | P1 | Revisione completa del ciclo di vita Scuola | D45 | Formazione/iscrizioni/rimborsi/assegnazioni coerenti |
| **P1-D47** | P1 | Revisione filiera Matite e stoccaggio; decisione sulla produzione futura | D46 | Scelta approvata, niente modifiche implicite alla ricetta |
| **P1-D48** | P1 | Catalogo tecnologie: requisiti, costi, effetti reali e dipendenze | D47 | Nessuna tecnologia acquistabile senza effetto definito |
| **P1-D49** | P1 | Integrazione GUI, ricerca e salvataggi in scenari completi | D48 | Test deterministici, compatibilità con partite esistenti |
| **P1-D50** | P1 | Chiusura fase civile/tecnologica e dossier per E4a | D49 | Suite verde, scenari e documentazione firmati |
| **E4a-01…04** | P2 | Strumentazione, simulazioni, colli di bottiglia, relazione | D50 | Risultati replicabili, nessun coefficiente cambiato |
| **E4b-01…04** | P3 | Proposta valori → decisione → implementazione → confronto | E4a | Valori approvati, benchmark pre/post e suite verde |
| **E4c-01…04** | P4 | Backup, migrazione opt-in, rollback, rilascio v14 | E4b | Nessuna perdita dati, prova rollback, approvazione |
| **M5.4 → M7** | P5 | Navale, esplorazione, conquista, finale, packaging | E4c | Gate specifici di ogni modulo e collaudo finale |

## 3. Fase 1 — Chiusura P0 della GUI, G1-D42–D45

### G1-D42 — Focus, tastiera, stabilità del layout

**Avanzamento RC28:** aggiunti stili di focus circoscritti alle liste v14 (bordo a spessore costante) e quattro esecuzioni di test headless in due viewport. [Rapporto tecnico](design/G1_D42_RC28_FOCUS_TASTIERA.md) · [Piano validazione](validation/G1_D42_RC28_TEST_PLAN.md). **Gate non ancora superato** finché mancano test Windows e controllo TAB/Shift+TAB. La pianificazione dei task sotto resta valida.

**Ambito:** UI-01, UI-07, UI-10, UI-12, UI-18 e UI-19. **Non aggiungere funzionalità**.

- Verificare spostamento con TAB e Shift+TAB in menu, Risorse, Edifici, Esercito, Scuola e Albero tecnologico; la selezione da tastiera deve aggiornare il dettaglio corretto.
- Verificare Enter/Spazio sui soli comandi abilitati; un edificio bloccato deve essere selezionabile ma non costruibile; focus e hover devono restare leggibili e non provocare movimenti.
- Verificare che un cambio edificio riporti all'inizio i due scroll del dettaglio, ma un tick sullo stesso edificio non azzeri lo scorrimento e non sposti il focus (RC27).
- Verificare visibilità del riquadro diagnostica massimo 104 px, pulsanti, scorrimento con testi lunghi e comportamento alle due viewport. Isolare eventuali difetti con test headless, senza sostituire l'osservazione Windows.
- Produrre rapporto con passi di riproduzione, gravità, screenshot richiesto e test da aggiungere. In assenza di un difetto dimostrato non cambiare lo XAML.

**Exit:** codice invariato o correzioni mirate; nessuna regressione introdotta; documento D42 con anomalie e piano prove D43.

### G1-D43 — Audit visivo/interattivo completo

**Ambito:** tutte le **22 voci** della [matrice P0](validation/G1_D39_P0_UI_MATRIX.md), nelle viewport **1040×740** e **1240×850**, a DPI costante e con salvataggi di prova.

- **Navigazione + Risorse (UI-01…06):** menu/tab, sette colonne, celle numeriche allineate a destra, scrollbar, 17 risorse v14, ordinamento, selezione ocra, tooltip capacità, saldo e `si riempie tra`.
- **Edifici (UI-07…13):** livelli separati, bloccati grigi ma selezionabili, prerequisiti, costo progressivo cumulativo, disabled/hover, lavoratori, upgrade, diagnostica e Pane.
- **Popolazione + Esercito (UI-14…15, UI-22):** crescita accelerata e tempo residuo, limiti abitativi, razioni Cibo/Pane, avanzamento reclute, rimborsi, assenza di doppi impegni popolazione.
- **Scuola + ricerca (UI-16…17):** 120 s attuali come **valore candidato**, 2 Matite minime, progresso, ricercatori, acquisti disabilitati senza risorse, saldo ricerca visualizzato a due decimali senza arrotondare la disponibilità sottostante.
- **Comandi e sessione (UI-18…21):** focus/hover, selezioni dopo tick/Salva/Carica, Nuova partita/Annulla, checkpoint precedente non sovrascritto prima di un Salva manuale, DB isolati e ripresa.

Per ogni UI-ID registrare **PASS / FAIL / NA motivata**, versione, viewport, dati usati, data, esito e riferimento screenshot o TRX. La v13 può essere **NA motivata** soltanto per funzionalità nuove v14 (Scuola, Albero, Matite/tiers). Non inventare esiti senza prova. Seguire il [runbook](validation/G1_D42_P0_RUNBOOK.md).

**Exit:** elenco numerato e riproducibile di tutti i FAIL, oppure 22 PASS/NA motivati; non chiudere il gate in questa milestone se esistono FAIL.

### G1-D44 — Correzioni P0 fondate su evidenze

- Triagiare FAIL: **P0-bloccante** (perdita salvataggio, comando pagabile/cliccabile erroneamente, impossibilità di usare UI), **P1-funzionale** (dettagli errati, selezione/focus) e **P2-cosmetico** (spaziature, colore non critico). Gli esiti non approvati restano aperti.
- Correggere per area, in lotti piccoli, senza ridisegnare la tabella Risorse o l'interfaccia classica v13.
- Ogni difetto: un test di regressione idoneo, controllo delle due viewport se rilevante, diff tracciato, documentazione e ZIP integrale.
- Ricontrollare i flussi Salva/Carica/Nuova partita su copie anonimizzate prima di chiudere UI-20/UI-21.

**Exit:** nessun FAIL bloccante né funzionale non risolto; test automatici verdi; prove Windows ripetute sui casi corretti.

### G1-D45 — Gate P0: certificazione grafica e funzionale

- Eseguire `validate.cmd` sul candidato cumulativo; registrare numero di test effettivo e TRX.
- Firmare **22 righe UI-01…UI-22** con PASS oppure NA motivata; zero FAIL aperti; disporre di prove a entrambe le dimensioni.
- Verificare differenze v13/v14 autorizzate, niente regressioni in Risorse/Edifici/Esercito, focus e layout stabile, protezione salvataggi e prova di riapertura.
- Aggiornare `CURRENT_STATE.md`, matrice, handoff e changelog, con dichiarazione chiara dell'approvazione visiva dell'utente.

**Exit vincolante:** P0 dichiarato chiuso **solo dopo** conferma manuale. In caso contrario D45 resta aperta; P1/E4 non sono certificati.

## 4. Fase 2 — P1 / Scuola e Albero tecnologico, D46–D50

### P1-D46 — Scuola: flusso e invarianti

- Audit iscrizione, capacità classi/posti, coda, progresso individuale, annullamento, rimborsi, qualificazione e assegnazione ricercatori con lotti +1/+5/+10.
- Verificare prenotazione degli abitanti tra lavoro, formazione, Esercito; assenza di duplicazione, saldi non negativi, gestione di posti e persone insufficienti.
- Controllare progressi durante pausa, salvataggio, reload, avanzamento di tempo e chiusura.

**Exit:** test e scenari che riproducono l'intero ciclo senza risorse o residenti creati/distrutti accidentalmente.

### P1-D47 — Matite, produzione e magazzino

- Verificare filiera attuale: **1 Legno lavorato + 1 Carbone → 4 Matite** su comando della Scuola, con **10 Cibo + 2 Matite per studente** come regola corrente; **solo 2 Matite minime sono vincolo confermato**, gli altri numeri restano candidati.
- Verificare capienze Municipio/Magazzino/Scuola, magazzino pieno, produzione atomica, rimborsi solo per matite realmente pagate e compatibilità delle iscrizioni pre-RC9.
- Preparare un'analisi di alternative: produzione manuale nella Scuola, produzione automatizzata più avanti o edificio produttivo dedicato. **Nessuna nuova filiera/edificio senza scelta esplicita**.

**Exit:** comportamento attuale affidabile e proposta comparativa di gameplay da approvare.

### P1-D48 — Tecnologie con benefici misurabili

- Inventariare tutte le tecnologie, prerequisiti (ricerca, edifici, progressione), costo, sblocco, effetti applicati a economia/UI e persistenza.
- Vietare acquisti di tecnologie senza un effetto definito; **Logistica/Navigazione restano indisponibili** fino a implementazione reale dei relativi benefici.
- Confermare accumulo e spesa libera dei punti ricerca, visualizzazione a 2 decimali e confronto basato sul valore non arrotondato.
- Verificare che i livelli Base/Grande/Avanzato rimangano edifici distinti, acquistabili direttamente oppure tramite upgrade secondo regole approvate.

**Exit:** tabella tecnologia→effetto→test di regressione, senza placeholder acquistabili.

### P1-D49 — Integrazione e compatibilità

- Scenari dal legno/carbone alla prima iscrizione, ricercatore attivo e tecnologia sbloccata; capacità e personale limitati.
- Salvataggi v14 precedenti e post-Matite, ripresa, identità delle selezioni e ciclo Nuova partita.
- UX compatta senza nuove ComboBox di tier e senza duplicare Costruisci nella scheda Scuola.

**Exit:** campagne realistiche riproducibili, suite verde, nessuna perdita compatibilità.

### P1-D50 — Gate scuola/tecnologie

- Rapporto di funzionalità, tabelle di regole **confermate vs candidate**, test integrati e note di migrazione eventuale.
- Approvazione del perimetro da misurare in E4a; niente bilanciamento anticipato.

## 5. Fase 3 — E4a: misurare prima di decidere

| Passo | Lavori | Output/gate |
| --- | --- | --- |
| **E4a-01 Baseline** | Definire seed, scenari, clock/step, input e schede di rilevazione ripetibili; registrare build e regole | Fixture e benchmark di riferimento senza cambiare parametri |
| **E4a-02 Progressione** | Misurare tempi/risorse per abitante, prime case, primo Pane sostenibile, primo studente e ricercatore, prime tecnologie e tier avanzati | Curve di tempi e costi, colli di bottiglia misurati |
| **E4a-03 Stress e stalli** | Bilanci multipli, acquisti +1/+5/+10, deficit materiali, magazzini saturi, assenza manodopera, razioni Cibo/Pane, training, combattenti, restart | Matrice di deadlock/limiti e riproduzione deterministica |
| **E4a-04 Rapporto** | Confrontare scenari, individuare costi o produzioni dominanti, proporre obiettivi di progressione senza assegnare nuovi numeri | Rapporto numerico e backlog di ipotesi da approvare |

**Misure minime:** tempo di progressione, popolazione/capienza, lavoratori liberi/occupati, produzione-consumo-saldo per risorsa, stoccaggio, deficit materiali, ricercatori/punti ricerca, coda militare, tempi di formazione, verifiche dopo Save/Load. Distinguere nettamente impossibilità strutturali da semplici attese; nessun obiettivo temporale viene inventato senza misurazioni.

**Exit:** scenari riproducibili e risultati prima di intervenire sui coefficienti.

## 6. Fase 4 — E4b: bilanciamento sottoposto ad approvazione

| Passo | Lavori | Criterio |
| --- | --- | --- |
| **E4b-01 Proposta** | Tabella valore attuale → alternativa A/B → impatto previsto, spiegazione | Revisione utente prima del codice |
| **E4b-02 Decisione** | Registrare scelte definitive in ADR, distinguere regole invarianti da numeri | Nessun coefficiente modificato senza decisione |
| **E4b-03 Implementazione** | Applicare solo valori approvati, migrare se necessario, testare progressioni | Test unit/integration e confronto benchmark |
| **E4b-04 Verifica** | Eseguire benchmark identici pre/post, controllare stalli e ritmo, eventuale secondo passaggio deliberato | Gate economico esplicito |

**Valori attuali candidati, non approvati definitivamente:** crescita prezzo `+15%` per edificio, fattori tier `1 / 2,6 / 6,5`, capacità `+50 / +150 / +500`, 120 s di formazione, 10 Cibo/iscrizione, 4 Matite/lotto, 0,1 punti/s per ricercatore. **Vincoli confermati:** 1/3/10 lavoratori ai tre livelli e almeno 2 Matite per studente. Il rapporto E4a decide cosa rivedere.

## 7. Fase 5 — E4c: sicurezza e attivazione v14

| Passo | Lavori | Gate |
| --- | --- | --- |
| **E4c-01 Inventario** | Schemi SQLite v2/v3, fixture storiche, inventario dei dati e incompatibilità | Nessun percorso di perdita dati non identificato |
| **E4c-02 Migrazione opt-in** | Copia v13→v14 con destinazione nuova, backup, convalida integrità, gestione conflitti | Originale intatto, nulla sovrascritto |
| **E4c-03 Rollback** | Prove di chiusura, errore I/O, crash/interruzione, schema non compatibile e ritorno a v13 | Recupero dimostrato e registrato |
| **E4c-04 Rilascio** | Valutare v14 come modalità normale solo dopo approvazione; QA, artefatti e istruzioni | Checkpoint finale, consentita disattivazione/rollback |

L'uso di database personali è escluso dai pacchetti. Non alterare il formato v13 in-place.

## 8. Fase 6 — Espansione, dopo v14 stabile

| Modulo | Ambito | Gate preliminare e di uscita |
| --- | --- | --- |
| **M5.4 Navale** | Cantiere, navi, equipaggi, materiali, slot di produzione, tecnologie con effetti | Definire costi/limiti dopo E4; nessun effetto fantasma |
| **M5.5 Esplorazione** | Esplorare fuori dalla colonia, individuare località e condizioni; distinguere **esplorazione** da **conquista** | Risultati deterministici nei casi approvati, senza conquista automatica |
| **M5.6 Conquista** | Requisiti di esercito/flotta, difesa, tempi, criteri di successo | Regole deliberate; niente perdite irreversibili inattese |
| **M5.7 Territori/Castello** | Nuove terre, benefici, logistica, Castello, condizioni di vittoria | Persistenza, progressione e vincita verificabili |
| **M6 Stabilizzazione estesa** | Collaudo completo, bilanciamento dell'espansione, usabilità, performance | Suite e benchmark estesi, nessun P0 |
| **M7 Distribuzione** | Build/installazione, documentazione utente, eventuale firma e aggiornamento | Release tracciabile con backup, rollback e licenza |

L'ordine e la struttura dei moduli sono proposti; il gameplay specifico richiederà ADR e approvazione prima del codice.

## 9. Registro decisioni e rischi da non perdere

| Tema | Stato alla RC27 | Quando decidere |
| --- | --- | --- |
| GUI v13, palette pixel art, tabella Risorse 7 colonne | **Vincolo confermato** | Non riaprire senza richiesta |
| Tre tier come edifici distinti, 1/3/10 lavoratori | **Vincolo confermato** | Non riaprire senza richiesta |
| Scuola costruita da Edifici, minimo 2 Matite/studente | **Vincolo confermato** | Non riaprire senza richiesta |
| Ricetta Matite e produzione manuale vs automatica | **Attuale, non bilanciata** | P1-D47 ed E4b |
| Costi progressivi e fattori tier | **Candidati** | E4a misurazione, E4b scelta |
| Tempo formazione e produzione punti ricerca | **Candidati** | E4a/E4b |
| Tecnologie Logistica/Navigazione | **Non acquistabili senza benefici** | P1-D48 / M5.4 |
| Migrazione v13→v14 | **Opt-in e non distruttiva** | E4c |
| Regole navale, esplorazione e conquista | **Da progettare** | M5.4–M5.6 |

**Rischi prioritari:** regressioni grafiche non coperte da xUnit; input o stoccaggi insufficienti che rendono impossibile una filiera; consumo/rimborso doppi nelle iscrizioni; discrepanza tra prezzi visualizzati e confronto economico; scritture inattese del salvataggio; tecnologie che sbloccano elementi senza effetto. Tutti richiedono test e prove riproducibili, non sole ispezioni statiche.

## 10. Metodo di lavoro e consegne

- **Ogni incremento di codice:** ZIP **integrale** (sorgenti, test, documentazione, changelog, roadmap, handoff, ADR quando necessario). Nessuna consegna di soli frammenti, niente DB personali, mantenere la cartella root ordinata.
- **Incremento solo documentale:** conservare l'ultima RC applicativa validata, non inventare una release o nuovi test; consegnare ZIP con progetto completo e documenti aggiornati, marcato «documentazione».
- **Test:** riportare sempre ultima suite confermata dall'utente, suite del candidato, verifiche statiche e prove visive non eseguite **separatamente**.
- **Ogni difetto:** descrivere comportamento atteso/osservato, come riprodurre, rischio regressione, test aggiunto e risultato Windows quando disponibile.
- **Ogni gate:** esplicitare evidenze di accettazione, decisioni ancora aperte e file da consultare nel passaggio di chat.

[Stato corrente](CURRENT_STATE.md) · [Handoff](NEW_CHAT_HANDOFF.md) · [Matrice UI](validation/G1_D39_P0_UI_MATRIX.md) · [Runbook G1-D42](validation/G1_D42_P0_RUNBOOK.md) · [ADR 0040](adr/0040-pianificazione-post-rc27-e-gate-sequenziali.md).
