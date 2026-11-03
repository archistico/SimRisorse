> **Aggiornamento 3 novembre 2026 — v58-RC5, gate R3 ancora aperto.** La RC4 ha superato la fase xUnit fino ad arrivare ai benchmark, poi ha fallito nel confronto industriale che pretendeva erroneamente `civil-growth-9` mentre la partita attuale è `civil-growth-13`. La RC5 separa la simulazione storica v9 dalla verifica economica v13, senza alterare la logica del gioco. **RC5 da compilare ed eseguire su Windows; validazione completa non confermata.** Leggere [rapporto RC5](validation/V58_RC5_VALIDATOR_VERSION_DRIFT.md). Le sezioni RC4 sottostanti sono storiche.

# SimRisorse — stato corrente del ramo di riallineamento

**3 novembre 2026 · v58-RC4 in attesa di validazione Windows.**

**Gate Windows RC3:** compilazione riuscita, **299 test eseguiti, 296 superati, 3 falliti**. Uno dei quattro errori RC2 (ricetta Pane) è stato risolto; restano un'asserzione sui progressi, il rifiuto dei ruleset futuri e la visibilità dell'Esercito nello scenario storico. RC4 li affronta con modifiche mirate, **non ancora compilate in questo ambiente**. R3 resta aperto. [Report RC4](validation/V58_RC4_3_REMAINING_TESTS.md).

Baseline: sorgenti v57 (codice v55) con integrazione militare dal ramo v41. Gli ZIP originali non sono stati modificati. [Stato v57 storico](CURRENT_STATE_V57_ARCHIVE.md).

## Scenario attivo

- `civil-growth-13` è il nuovo **ruleset candidato**; `civil-growth-12` è mantenuto tramite `CreateTownHallForBenchmarks()` per la riproduzione storica.
- Il catalogo conserva **21 strutture** (20 obiettivo + Municipio fisso) e **16 risorse**. Mancano Cantiere e Castello per completare i 22 edifici-obiettivo.
- Restano esattamente le capacità v57: Municipio +1.000 per risorsa; Magazzino +500; edifici produttivi +50 ai propri output; case +10/+25 a Cibo e Pane; Caserma +20 Spade. Le risorse sono condivise; Pane non fa eccezione.
- Caserma del nuovo ruleset: **260 Legno lavorato + 200 Pietra**, prerequisito Forgia, **5 Spade per recluta**, **1 posto per Caserma**, **30 secondi per persona**. La coda e gli addestramenti in corso sono annullabili con rimborso atomico delle Spade se il deposito ha spazio. **Congedo disabilitato nella v13**, operativo nel ruleset v12 storico.
- Reclute e soldati restano residenti, occupano alloggi, consumano Cibo/Pane come prima e non possono essere assegnati a lavori civili.
- Scheda Esercito v57 preservata con progressi e descrizioni adattati al nuovo ruleset; conservati navigazione 8-bit, dettaglio capacità e tooltip dei magazzini.

## Persistenza e migrazione

- SQLite **user_version=2** e Snapshot **format=2** invariati. Il nuovo ruleset usa la tabella `military_state` storica e una tabella aggiuntiva `military_training` per i progressi individuali. La scrittura avviene nella stessa transazione dello stato civile.
- Le versioni da 1 a 12 percorrono conversioni dichiarate; il passo 12→13 conserva totale reclute/soldati, le risorse, le strutture, la popolazione, il tempo e il progresso civile. La quota temporale del gruppo (0–59 s) diventa quota corrispondente dell'addestramento individuale (0–29 s); i tirocinanti oltre i nuovi posti disponibili rientrano in coda **senza il proprio progresso precedente**. Politica esplicita ma da accettare definitivamente.
- Per il fork v41 salvato con etichetta `civil-growth-10` e **schema fisico alternativo**, la lettura identifica le colonne `queued_recruits` / `soldiers` e la tabella `military_training`; conserva l'avanzamento individuale e aggiunge un Municipio. La sovrascrittura convertita avviene solo dopo backup S1 dell'originale. Il formato del fork originale rimane leggibile.
- **Le partite reali dell'utente non vengono toccate durante i test.** Prima di usare il nuovo pacchetto su `%LOCALAPPDATA%\SimRisorse\civil-growth.sqlite3`, fare una copia esterna del DB. Validare con file temporanei prima.

## Stato reale delle verifiche

- RC2: Windows ha compilato e riportato 295/299 test superati.
- RC3: Windows ha compilato e riportato **296/299 test superati**, 3 falliti (Core/Application/Desktop); Infrastructure e test della ricetta Pane superati.
- RC4: introdotto confronto scalare del progresso individuale, controllo esplicito delle versioni sconosciute e sincronizzazione della visibilità Esercito all'apertura della finestra. Aggiunti assert diagnostici e test per la disponibilità della navigazione negli scenari militari.
- **R3 non concluso:** serve esecuzione di `\.\validate.cmd` su Windows, benchmark, smoke alle risoluzioni previste e prove su copie di DB. Non sono attribuiti test passati alla RC4.
- [Rapporto RC4](validation/V58_RC4_3_REMAINING_TESTS.md).

## Lavori futuri

1. R3: compilazione, test, simulazioni, save upgrade e smoke desktop.
2. M5.3c: simulazione/bilanciamento di arruolamento a 5 Spade e posti individuali.
3. G1: approvare dettagli missioni/isole e il contratto navale N-01…N-07 (ancora **proposte**).
4. M5.4: Cantiere, flotta, salvataggi e preflight (ruleset futuro candidato **civil-growth-14**).
5. M5.5 esplorazione; M5.6 conquista deterministica; M5.7 Castello; M6 vittoria/bilanciamento; M7 distribuzione.

Documenti: [report di integrazione](validation/V58_RECONCILIATION.md), [ADR](adr/0015-individual-military-training-branch-reconciliation.md), [roadmap attiva](ROADMAP.md).
