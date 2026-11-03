# SimRisorse — Roadmap operativa RC16 / G1-D31

**Baseline validata su Windows:** **RC15 / G1-D30, 473 test attesi dichiarati superati dall’utente il 03/11/2026** (TRX non allegato). **Candidata corrente:** RC16 / G1-D31, 476 test attesi, non eseguiti nell’ambiente di confezionamento. **Gate grafico P0 ancora aperto.**

**Principio approvato:** consolidare e validare la GUI prima di nuove funzioni; analizzare l'economia prima di modificare i coefficienti; avviare navale/esplorazione soltanto dopo il consolidamento della campagna. La v13 resta una baseline intangibile quanto a funzionalità e salvataggi.

## Milestone e ordine vincolante

| Fase | Identificativo | Priorità | Obiettivo | Evidenze/gate per chiudere |
| --- | --- | --- | --- | --- |
| **1 — Audit e chiusura GUI** | **G1-D29 → D30+** | **P0** | Inventariare e correggere le differenze v13/v14 in Risorse, Edifici, Esercito, Popolazione, Scuola, Ricerca, salvare/caricare e stile. | Matrice **UI-01…UI-22** esaminata e classificata, 1040×740 e 1240×850 controllate su Windows, suite xUnit verde, nessuna regressione P0 irrisolta. |
| **2 — Rifinitura Scuola/tecnologie** | **G1-Dxx** | P1 | Completare coerenza UX e benefici effettivi delle funzioni civili già introdotte; ridurre duplicazioni GUI solo quando esistono test di equivalenza. | Tutte le azioni sono spiegate, non duplicano residenti; ricerche senza effetto non acquistabili; regole di Matite e Scuola riproducibili; v13 invariata. |
| **3 — Audit economico misurato** | **E4a** | P2 | Benchmark su progressione (case, Cibo/Pane, produttori Base/Grande/Avanzato, storage, militari, Matite, ricerca). | Report con tempi, colli di bottiglia, deadlock, consumi/saldi, casi salva/ripristina e scenari riproducibili; **nessun coefficiente inventato**. |
| **4 — Bilanciamento deliberato** | **E4b** | P3 | Proporre e applicare coefficienti produttivi, prezzi, capienze, tempi e costi tecnologici. | Decisioni dell'utente esplicite, ADR economico aggiornato, benchmark pre/post e suite completa verde. |
| **5 — Consolidamento campagna v14** | **E4c** | P4 | Migrazione v13→v14 opt-in, backup, rollback, validazione SQLite e passaggio facoltativo a modalità normale. | Migrazione non distruttiva comprovata su fixture anonimizzate; recupero da errori e salvataggi storici verificato; approvazione rilascio. |
| **6 — Espansione del gioco** | **M5.4 → M7** | P5 | Navi/cantiere, esplorazione separata dalla conquista, territori/Castello, vittoria, rifiniture e distribuzione. | Gate funzionali e numerici specifici prima di ciascun modulo; nessuna perdita irreversibile non deliberata. |

**Stop rule:** un gate fallito blocca il passaggio alla fase successiva; un test automatico verde **non** costituisce approvazione estetica o dei prezzi del gameplay.

## Dettaglio fase 1: G1-D29 → D30+

### D29 — Audit con prove e prime correzioni locali

1. Congelare RC12 come base, registrando l'esito Windows riferito dall'utente. Non aggiornare i salvataggi v13/v14.
2. Confrontare `MainWindow.axaml`/`MainViewModel.cs` con `V14PreviewWindow.axaml`/`V14PreviewViewModel*.cs` e test headless; conservare una [matrice d'audit osservabile](validation/G1_D29_AUDIT_GUI_V13_V14.md).
3. Classificare le 22 voci UI-01…UI-22 in **copertura automatica**, **evidenza solo statica**, **controllo visivo ancora aperto**, **difetto identificato**.
4. Correggere **solo** difetti circoscritti e dimostrabili con test. In RC13: capacità Matite nel tooltip, realmente scomposta per edifici posseduti e spazio libero; nessun redesign, niente nuove regole.
5. Eseguire controlli statici locali, chiedere conferma della suite Windows per la nuova candidata; non dichiarare green test che non sono stati eseguiti. **Storico:** RC13 fallì la compilazione per CS0136; RC14 ha corretto i nomi locali ed è stata dichiarata validata dall’utente (470 test). D30 è avviato con RC15 candidata.

**Uscita D29:** roadmap aggiornata + rapporto di audit iniziale + correzioni e test circoscritti + ZIP integrale. D29 **non** chiude P0 grafico.

### D30+ — Chiusura P0 visiva e funzionale

**Avanzamento D30 / RC15, ora con test dichiarati passati:** corretti due scostamenti documentati: il dettaglio Edifici indica ora la capienza Matite per Municipio/Magazzino/Scuola e la conferma di nuova partita blocca Pausa/Riprendi come v13. Tre nuovi test Desktop (473 attesi). Non è un gate P0 completato: restano screenshot e confronto a due risoluzioni. [Rapporto](design/G1_D30_RC15_PARITA_DETTAGLI_E_CONFERMA.md) · [Piano test](validation/G1_D30_RC15_TEST_PLAN.md).

**Avanzamento D31 / RC16:** rilevata la possibilità di modificare la colonia durante la conferma «Nuova partita?». Aggiunta una guardia comune a costruzione, personale, Esercito, Scuola, Matite e acquisti ricerca; +3 test Desktop, 476 attesi. Il layout resta invariato. Il flusso Salva/Carica durante la conferma è un controllo aperto UI-20/21, non una funzione convalidata. [Rapporto](design/G1_D31_RC16_GUARDIA_CONFERMA_NUOVA_PARTITA.md) · [Piano](validation/G1_D31_RC16_TEST_PLAN.md).

- **Risorse:** sette colonne esatte, ordinamento italiano, quantità arrotondate, tooltip di capacità reale (compreso Pane e Matite), produzione/consumo/saldo, riempimento `hh:mm:ss`, scrollbar non sovrapposta.
- **Edifici:** Base/Grande/Avanzato elencati separatamente; bloccati grigi ma selezionabili; costo/prerequisiti/diagnostica; `Costruisci +1/+5/+10` sulla prima riga e `Lavoratore −1/+1/+5/+10` sulla seconda; hover ocra immobile e disabled grigio; upgrade distinto e lotti cumulativi.
- **Esercito:** Caserme, Spade, reclute in attesa e in corso, progresso individuale, arruola/annulla, nessun congedo improprio.
- **Scuola e Albero:** formare e annullare studenti, progressi e ricercatori, 2 Matite a iscrizione; ricerca volontaria con saldo e costi a due decimali, nessun acquisto senza punti o benefici.
- **Popolazione e ciclo partita:** crescita e razioni Cibo/Pane, pausa/tempo, selezione persistente dopo tick, nuova partita con conferma, salvataggi separati, DB incompatibili senza sovrascritture né finestre bloccate.
- Confronto visivo **v13/v14 a 1040×740 e 1240×850** e documentazione degli scostamenti autorizzati (nuove funzioni Scuola e tecnologia).

## Fase 2 — Tecnologie e Scuola: decisioni rinviate

- La Scuola rimane un edificio selezionabile/costruibile **solo da Edifici**. La schermata Scuola contiene iscrizioni, code/progresso, ricercatori e laboratorio Matite.
- Ogni iscrizione richiede **almeno 2 Matite**; la ricetta attuale `1 Legno lavorato + 1 Carbone → 4 Matite` è **candidata**. Valutare se introdurre in futuro una produzione automatica solo dopo una decisione di gameplay.
- Verificare capacità del Municipio/Magazzino/Scuola senza eccezioni nella tabella Risorse; niente doppie prenotazioni di residenti. 
- Valutare i benefici effettivi di ogni tecnologia; Logistica/Navigazione restano indisponibili finché senza effetti.
- Refactoring delle viste condivise solo con prove di equivalenza e nessuna alterazione grafica inattesa della v13.

## E4a, E4b, E4c — vincoli di progettazione

**E4a:** misurare la progressione reale dal nuovo insediamento al primo ricercatore, alla casa avanzata, alla produzione sostenibile di Pane e alla disponibilità militare. Riprodurre l'economia in avanzamenti temporali diversi, lotti e restart; individuare storage deadlock, dipendenze circolari e manodopera insufficiente.

**E4b:** valori **candidati, non approvati**: costi +15%/edificio, moltiplicatori Base/Grande/Avanzato `1 / 2,6 / 6,5`, capacità produttori `+50 / +150 / +500`, formazione 120 s, costo 10 Cibo, Matite 4/lotto, ricerca 0,1 punti/s. **Confermati**: livelli distinti 1/3/10 posti e almeno 2 Matite per studente. Chiedere una decisione esplicita prima di cambiare i coefficienti.

**E4c:** backup prima di migrare; copia v13→v14 con percorso nuovo, never overwrite; rollback dimostrato; compatibilità SQLite v2/v3 e dati storici verificata. Attivazione ordinaria solo con approvazione.

## Espansione M5.4–M7

- M5.4: Cantiere, Navi, equipaggi, costi e tecnologie navali con **effetti reali**.
- M5.5: Esplorazione **distinta** dalla conquista; regole e costi deliberati, risultati deterministici dove approvati.
- M5.6/M5.7: conquista, territori, Castello e condizioni di vittoria; niente penalità irreversibili introdotte tacitamente.
- M6–M7: usabilità finale, validazione, pacchetto di distribuzione.

## Regole immutabili di consegna

- Per ogni codice: **ZIP integrale** con sorgenti, test, README, changelog, roadmap, handoff e ADR se pertinenti; niente soli frammenti.
- Test automatici confermati dall'utente ≠ audit grafico; distinguere sempre verifiche eseguite e pendenti.
- Preservare GUI e SQLite v13, mantenere separati i salvataggi sperimentali; non incorporare database personali.
- Tenere root compatta e versioni precedenti in `docs/history/`.

[Stato corrente](CURRENT_STATE.md) · [Matrice originale](validation/G1_D19_MATRICE_PARITA_GUI.md) · [Audit D29](validation/G1_D29_AUDIT_GUI_V13_V14.md) · [ADR 0038](adr/0038-roadmap-gate-gui-e-audit-matite.md) · [Handoff](HANDOFF.md).
