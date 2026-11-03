> **Aggiornamento v58-RC5 (3 novembre 2026):** il gate R3 si è spostato oltre i test xUnit e si ferma sul validatore delle simulazioni; nella RC4 quest'ultimo confrontava la v13 con attese v9. RC5 congela i percorsi storici su v9 e aggiunge sonde economiche v13. **R3 ancora aperto:** servono `validate.cmd`, prove SQLite su copie e smoke Windows. Nessuna milestone navale implementata.

# SimRisorse — roadmap attiva dopo il recupero del ramo v41

**3 novembre 2026 · v58-RC4 in attesa di rivalidazione Windows (RC3: 296/299 test, 3 falliti).** Versione software originaria consolidata: v57 (codice v55). [Roadmap v57 storica](ROADMAP_V57_ARCHIVE.md).

| Fase | Obiettivo | Stato reale | Criterio di chiusura |
| --- | --- | --- | --- |
| **R0** | Confrontare v41 alterato e v57, individuare funzioni mancanti e collisioni di schema | **Completato (statico)** | Audit con archivio e hash |
| **R1** | Integrare addestramento individuale su v57, conservando il Municipio e la UI più recente | **Implementato; RC3 compilata, RC4 non verificata** | Gate R3 + approvazione politica di migrazione |
| **R2** | Migrazione v10–v12→v13 e fork v41 con schema diverso; backup S1 | **Implementato; test SQLite RC3 superati, RC4 da verificare** | Test con copie DB reali e sintetiche, backup integro |
| **R3** | Build, test xUnit, benchmark, smoke Windows | **Pendente, bloccante (RC3: 3 test falliti)** | `validate.cmd` verde, UI 1040×740 e 1240×850 |
| **M5.3c** | Misure con 5 Spade, 1 posto/Caserma e tempi individuali | **Da eseguire** | Scenari 8 ore + deficit alimentare e popolazione |
| **G1** | Finalizzare esplorazione, conquista e flotta: scelta isole, soldati, viaggi, rientri | **In discussione** | Regole approvate con ADR |
| **M5.4** | Cantiere e navi persistenti (proposta: `civil-growth-14`) | **Non implementato** | Costo, ID, capacità, salvataggi, UI, preflight |
| **M5.5** | Esplorazione separata, viaggi con tempo simulato | **Non implementato** | Stati deterministici, no doppi esiti, restore corretto |
| **M5.6** | Conquista deterministica e almeno un'isola obbligatoria | **Non implementato** | Requisiti verificabili, sblocco salvabile |
| **M5.7** | Castello, ultimo edificio-obiettivo | **Non implementato** | Richiede conquista, catalogo 22 obiettivi |
| **M6** | Vittoria, risultati, bilanciamento e percorso automatico | **Non implementato** | Campagna completabile; 8 h / 32+ h sono obiettivi |
| **M7** | Rifinitura, salvataggi, UX, build distribuibile | **Non implementato** | Release documentata e verificata |

[Rapporto dei tre errori residui e interventi RC4](validation/V58_RC4_3_REMAINING_TESTS.md).

## Gate immediato: R3

1. Non toccare i salvataggi personali. Estrarre lo ZIP del candidato v58-RC4 in una directory nuova e vuota.
2. Installare .NET SDK 10 e ripristinare i pacchetti da `packages.lock.json`, se necessario; eseguire `.\validate.cmd` dalla root in PowerShell/cmd. Conservare il log.
3. Controllare regressioni di Municipio, capacità di Pane e Magazzino, produzione, popolazione, schermata Risorse/Esercito; confermare capacità Caserma e nuove azioni.
4. Con un DB di **test**, provare almeno: vecchio v10 gruppo a metà, v12 a metà, schema alternativo v41 a metà, v13 a metà. Salvare e riaprire più volte; controllare backup S1, integrità e nessun incremento artificiale di abitanti, Spade o unità.
5. Solo dopo R3, decidere e programmare il primo blocco di M5.4. La proposta navale N-01…N-07 in `docs/design/M5_4_DECISION_GATE_V55.md` **non è ancora approvata**.

## Regole già concordate per l'orientamento delle spedizioni

- Esplorazione distinta dalla conquista.
- Requisiti di conquista deterministici, senza lotteria del successo.
- Nessuna perdita irreversibile iniziale di soldati o navi.
- Numeri, isole, possibilità di missioni parallele, tempi andata/ritorno e destinazione delle truppe dopo la conquista **restano aperti**.

**Principio di sviluppo:** una nuova meccanica diventa definitiva con proposta, approvazione, codice, test, documentazione e pacchetto completo; un test non eseguito non conta come superato.
