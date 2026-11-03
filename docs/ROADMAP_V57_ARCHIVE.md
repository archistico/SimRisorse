# SimRisorse — roadmap operativa

**28 ottobre 2026 · checkpoint documentale v57, codice v55.** Solo questa roadmap descrive le attività ancora da svolgere; la [roadmap cronologica fino alla v56](ROADMAP_ARCHIVE_V56.md) contiene tutti i resoconti precedenti. L'ordine delle priorità deriva dallo [stato corrente](CURRENT_STATE.md) e dalle [decisioni confermate](AUTHORITY_REGISTRY.md).

## A. Milestone e stato reale

| Milestone | Obiettivo | Stato al checkpoint |
| --- | --- | --- |
| **M0–M3** | Fondazioni, motore, desktop e salvataggi | **Completate nei gate storici**; preservare regressioni |
| **M4** | Crescita civile, Cibo/Pane, abitazioni, magazzini, benchmark, UI retro | **Implementata** e già verificata nei gate storici |
| **M5.1** | Ferro, Carbone, Oro, filiere industriali e prove scala | **Implementata**; misure dettagliate successive da recuperare dai benchmark |
| **M5.2** | Forgia, Spade, Zecca, Monete | **Implementata**, con regole A approvate |
| **M5.3** | Caserma, reclute, addestramento e pagina Esercito | **Implementata nel codice**, validazione completa della v55/v57 ancora pendente |
| **M5.4** | Cantiere, navi, flotta salvabile, preflight | **Solo proposta N-01…N-07**; non approvata/non implementata |
| **M5.5** | Esplorazione e missioni temporizzate | Da definire e implementare dopo M5.4 |
| **M5.6** | Conquiste di isole e sblocchi | Da definire e implementare dopo M5.5 |
| **M5.7** | Castello e progressione completa | Da definire e implementare dopo M5.6 |
| **M6** | Vittoria, percorsi riproducibili, bilanciamento e risultati | Da iniziare dopo M5 |
| **M7** | Rifinitura, qualità, distribuzione | Da iniziare dopo M6 |

Il ruleset giocabile è **`civil-growth-12`**, con **21 strutture a catalogo**: **20 dei 22 edifici-obiettivo** e **1 Municipio aggiuntivo fisso**. Restano Cantiere e Castello; **16 risorse economiche**. Le navi future sarebbero entità, non una nuova risorsa. Non introdurre un nuovo `civil-growth-13` senza una modifica approvata del gameplay e una migrazione esplicita.

## B. Primo gate da eseguire — v55/v57

1. Su Windows e nella root estratta del pacchetto, eseguire `.\validate.cmd` con .NET SDK 10 e dipendenze ripristinate. Sono attesi **287 casi xUnit**, più cataloghi, simulazioni/benchmark, pubblicazione e smoke nativi. **Non eseguiti qui per SDK assente**.
2. Inviare il risultato completo o almeno il **primo errore** con messaggio, progetto, file, riga e nome del test. Non passare a nuove regole se il gate fallisce.
3. Controllare visivamente **1040×740** e **1240×850**: scrollbar non sovrapposte, selezione ocra, tooltip Capacità, dettaglio Costi/Produzione/Consumi/Capacità, dotazione e pulsanti del Municipio, Esercito.
4. Con salvataggi di **test**: convertire uno snapshot `civil-growth-11`, verificare il backup S1, la seconda apertura e l'assenza di dotazioni duplicate. Nessuna operazione sulle partite personali.

Check e limiti del gate: [rapporto statico v55](validation/V55_STATIC_AUDIT.md), [documentazione del Municipio](validation/V54_TOWN_HALL_DISTRIBUTED_STORAGE.md) e [guida al formato](SAVE_FORMAT.md).

## C. M5.4 — solo dopo gate positivo e decisione navale

La [scheda decisionale N-01…N-07](design/M5_4_DECISION_GATE_V55.md) propone, **senza approvazione attuale**, navi con ID persistenti; Cantiere 320 Tavole/220 Pietra; nave 160 Tavole/120 Monete; 5 soldati per nave; riutilizzo, acquisto immediato e nessuna flotta mercantile. Sono numeri teorici, **non** un benchmark della partita iniziale.

| Fase | Dopo approvazione esplicita | Criterio di uscita |
| --- | --- | --- |
| **M5.4a** | Cantiere/catalogo `civil-growth-13`, acquisto nave nel Core e handler applicativi | Costi, precondizioni, ID univoci e acquisto atomico testati; nessun saldo negativo |
| **M5.4b** | Stato navale nel checkpoint, conversione S1 v12→v13, tab Flotta | Backup e vecchi salvataggi conservati, nessuna nave duplicata, GUI verificata a due risoluzioni |
| **M5.4c** | Preflight di trasporto senza missioni | Soldati disponibili e posti di carico contati senza effetti collaterali |

**Non implementare** una missione, la perdita di una nave o un nuovo tipo mercantile durante M5.4 senza decisione successiva. Lo schema SQLite resta v2 finché non serva davvero una modifica dichiarata; qualunque nuova tabella/stato deve salvare atomicamente con esercito e scorte.

## D. M5.5–M5.7 — isole e Castello

- **M5.5, esplorazione/spedizioni:** approvare durata, requisiti, partecipanti, costo, successo/fallimento, ripresa da salvataggio e garanzia *applicazione una sola volta*. Stesso tempo di simulazione dell'economia, non tempo reale a programma chiuso.
- **M5.6, conquista:** almeno **una conquista obbligatoria** è una decisione già acquisita; isola concreta, eventuali ricompense, forza minima, rischi e sblocco vanno ancora specificati. Non introdurre cicli in cui la prima nave richiede risorse ottenibili solo con la conquista stessa.
- **M5.7, Castello:** ultimo dei **22 edifici-obiettivo**, disponibile dopo la conquista. Testare tutti i casi negativi e la conservazione del progresso nei checkpoint.

## E. M6 — vittoria e bilanciamento

1. Applicare la condizione di possesso contemporaneo di tutti i **22 tipi** con Castello ultimo. **Non** contare il Municipio extra come un ventitreesimo obbligo.
2. Costruire una strategia automatica dalla colonia iniziale alla vittoria, riproducibile e senza risorse preconcesse artificialmente.
3. Misurare la durata di una buona strategia rispetto all'obiettivo **~8 ore di gioco**; confrontare strategie plausibili più lente fino a **32 ore o più**. Non presentare tali valori come già raggiunti.
4. Bilanciare colli di bottiglia, persone, razioni, costi navali/militari, recupero dai blocchi e possibilità di completamento.
5. Definire risultati/classifica e possibile verifica anti-manomissione dei checkpoint, senza server esterni salvo decisione dell'utente.

## F. M7 — qualità e distribuzione

- UI Avalonia stabile su differenti DPI, dimensioni, tastiera, contrasto e selezione. Pixel art 8-bit originale e leggibilità confermata.
- Documentazione utente, help dei messaggi di blocco, prove native Windows e, se pertinente, Linux/macOS.
- Build riproducibili, dipendenze bloccate, diagnostica di errori e salvaguardia delle partite; licenza AGPL v3 preservata.

## G. Modalità di collaborazione e rischi

- Ogni nuova meccanica richiede **proposta → approvazione → implementazione → test → gate**. «Continua» non è approvazione di costi navali o regole di conquista.
- Conservare la separazione Core/Application/Infrastructure/Desktop, gli scenari storici congelati e la migrazione S1 con backup. Non modificare il formato dei salvataggi senza controllo di compatibilità.
- Nei messaggi di avanzamento riportare sempre **stato/versione, test confermati, modifiche, prossimo blocco della roadmap, attività pendenti e decisioni aperte**.
- Decisioni rinviate: **Carne** fuori dalla prima campagna; **demolizione** e relativi rimborsi non approvati; criteri esatti della classifica e bilanciamento globale ancora aperti.

Per riprendere da zero il contesto senza ripetere il lavoro consultare [handoff operativo](HANDOFF.md), [passaggio completo](NEW_CHAT_HANDOFF.md) e [registro delle fonti](AUTHORITY_REGISTRY.md). Lo [storico della roadmap](ROADMAP_ARCHIVE_V56.md) conserva tutte le fasi precedenti.
