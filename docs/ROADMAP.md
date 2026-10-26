# Roadmap

Aggiornata al 2026-10-26. Le milestone hanno dipendenze e criteri di uscita, senza stime di calendario non ancora fondate. Il [changelog](../CHANGELOG.md) registra solo il lavoro realizzato.

| Milestone | Obiettivo | Stato | Dipende da |
| --- | --- | --- | --- |
| M0 | Ordinamento e documentazione iniziale | Completata il 2026-10-26 | — |
| M1 | Specifica minima e validazione dello stack | Completata il 2026-10-26 | M0 |
| M2 | Motore economico verificabile senza UI | Completata il 2026-10-26 | M1, regole minime definite |
| M3 | Prima partita desktop salvabile | Completata il 2026-10-26 | M2, stack confermato |
| M4 | Popolazione dinamica e progressione civile | In corso; case, crescita e prima filiera completate | M3 |
| M5 | Soldati, navigazione e isole | Da avviare | M4, regole militari definite |
| M6 | Scenario completo, risultati e bilanciamento | Da avviare | M5, condizione di vittoria definita |
| M7 | Consolidamento e distribuzione | Da avviare | M6 |

## M0 — Ordinamento e documentazione iniziale

- [x] Conservare soluzione, sorgenti e test della bozza in `legacy/winforms/`.
- [x] Organizzare i quattro screenshot in `docs/references/screenshots/`.
- [x] Raccogliere cache e output locali in un percorso ignorato da Git.
- [x] Creare README, game design, architettura, ADR, roadmap, changelog e handoff.
- [x] Verificare integrità dei file spostati, riferimenti della soluzione e collegamenti della documentazione.

Criterio di uscita: documentazione navigabile, nessuna perdita di sorgenti o immagini e distinzione esplicita tra implementato, accettato e proposto.

Evidenze: integrità SHA-256 di 30 file preservati, riferimenti validi per i due progetti legacy, 16 documenti Markdown con 58 collegamenti locali validi, esclusioni delle cache e controllo delle differenze senza errori. Dettagli nell'[handoff](HANDOFF.md).

## M1 — Specifica minima e validazione dello stack

Definire un piccolo scenario economico, sufficiente a provare acquisto, assegnazioni e consumo di risorse.

- Risolvere Q-03, Q-06 e Q-07 del [game design](GAME_DESIGN.md) per il solo scenario minimo.
- Definire dotazione iniziale, unità delle quantità, accumuli frazionari, ordine di avanzamento e politica di contesa degli input.
- Scegliere velocità iniziale e politica degli ordini in pausa per la prova; distinguere queste scelte dall'automatismo alla minimizzazione, se rinviato.
- Eseguire la prova tecnica descritta nell'[ADR 0005](adr/0005-desktop-stack-candidate.md), selezionando i primi sistemi operativi obiettivo.
- Aggiornare o sostituire l'ADR prima di generare la soluzione definitiva.

Criterio di uscita: una scheda di scenario con valori concreti, esiti attesi e decisioni sufficienti a implementare M2; evidenze della prova tecnica e stato dello stack aggiornato. Le domande su isole e combattimenti non bloccano questa milestone.

Completata con [scenario ridotto](scenarios/ECONOMY_PROBE.md), [prova eseguibile](../experiments/stack-probe/README.md) e [report delle verifiche](validation/M1_STACK_PROBE.md): 23 test superati, build Release senza avvisi e pubblicazione avviata su Windows. Stack adottato nell'ADR 0005; regole numeriche limitate alla prova. Linux/macOS e audit online non verificati.

## M2 — Motore economico senza interfaccia

- Creare i progetti necessari della nuova soluzione, senza riferimenti al prototipo.
- Selezionare versioni supportate di SDK e pacchetti, distinguendole da quelle dell'esperimento offline, ed eseguire l'audit delle dipendenze quando la sorgente online è disponibile.
- Implementare stato della partita, risorse, capacità, piccolo catalogo e popolazione iniziale fissa.
- Realizzare costruzione, assegnazione e rimozione dei lavoratori, produzione con input e avanzamento temporale esplicito.
- Rendere pausa e ripresa operazioni coerenti, indipendenti dal timer grafico.
- Verificare scenari a scorte insufficienti, personale insufficiente, magazzino pieno e acquisto non riuscito.

Criterio di uscita: una partita economica eseguibile senza finestre, con risultati ripetibili per la stessa cronologia. Nessun output gratuito o conteggio negativo; nessuna doppia assegnazione di persone.

Completata con [soluzione definitiva](../SimRisorse.slnx), Core senza dipendenze NuGet, casi d'uso verticali e simulatore da terminale. [Verifiche M2](validation/M2_CORE.md): 45 test superati, build senza avvisi e simulazioni di 60 secondi, 8 e 32 ore. Precisione e prenotazioni formalizzate nell'[ADR 0006](adr/0006-economic-step-and-resource-precision.md). Audit online tentato ma non concluso per errore TLS; da ripetere in un ambiente con accesso NuGet funzionante.

## M3 — Prima partita desktop salvabile

- Collegare l'interfaccia scelta a uno stato di lettura coerente del motore.
- Mostrare elenco, dettaglio, quantità, personale, flussi netti, tempo e motivi di blocco.
- Offrire nuova partita, pausa, ripresa, costruzione e assegnazione dei lavoratori.
- Implementare il primo adattatore di salvataggio, formato versionato e gestione degli errori.
- Salvare e riprendere esattamente lo stato, senza avanzamento a gioco chiuso.

Criterio di uscita: eseguire una sessione, fermarla, chiudere, riaprire e continuare senza perdita di scorte, tempo o assegnazioni. L'interfaccia rimane utilizzabile mentre il gioco avanza.

Completata con desktop Avalonia collegato al Core, proiezioni di flusso, ordini multipli e checkpoint SQLite manuali, periodici e in chiusura. [Verifiche M3](validation/M3_DESKTOP.md): 82 test superati, quattro render, pubblicazione Windows e ciclo nativo di salvataggio/ripresa/sospensione simulata/chiusura. L'audit online è stato ritentato ma resta non concluso per TLS; altri sistemi operativi e migrazioni sono ancora da verificare.

## M4 — Popolazione dinamica e progressione civile

Primo incremento completato: [scenario Civil Growth](scenarios/CIVIL_GROWTH.md), con abitazioni, crescita intera, pescatore, pesce→cibo e carenze recuperabili. Parametri provvisori adottati per questo scenario. [Verifiche](validation/M4_CIVIL_GROWTH.md): 117 test, due cronologie di sviluppo/recupero, simulazioni fino a 32 ore, otto render e ripresa nativa della crescita parziale. Snapshot/file v2 e vecchio scenario v1 separati nell'[ADR 0009](adr/0009-civil-growth-and-save-generations.md).

Resta da completare l'ampliamento civile: magazzini espandibili e ulteriori filiere, a partire da grano/mulino/pozzo/forno. Demolizione e rimborsi richiedono ancora Q-08. Il primo incremento non chiude M4 né il bilanciamento del gioco completo.

- Risolvere le regole di abitazioni, crescita, cibo e carenze (Q-05).
- Ampliare ricette e prerequisiti, verificando che la dotazione iniziale consenta di avviare tutte le filiere necessarie.
- Definire e, se confermata, introdurre la demolizione con effetti coerenti su capacità e persone (Q-08).
- Aggiungere operazioni multiple e strumenti per individuare i colli di bottiglia.

Criterio di uscita: uno scenario civile con più fasi e possibilità misurabili di recuperare dagli errori; nessuno sblocco irraggiungibile per un errore del catalogo.

## M5 — Soldati, navigazione e isole

- Risolvere Q-09 e precisare il ruolo delle conquiste nella progressione (Q-02).
- Definire passaggio da popolazione civile a reclute e soldati, se confermato il bacino condiviso.
- Implementare addestramento, contingenti, strutture militari e mezzi strettamente necessari.
- Implementare missioni con requisiti, durata, esito e ricompense espliciti.
- Conservare missioni e addestramenti nei salvataggi, inclusi gli avanzamenti parziali.

Criterio di uscita: partire dall'economia civile, preparare una spedizione, sospenderla e riprenderla, completarla e applicarne gli effetti una sola volta.

## M6 — Scenario completo, risultati e bilanciamento

- Fissare la condizione esatta di vittoria (Q-01) e le regole dei risultati (Q-10).
- Eseguire lo scenario completo con uno strumento automatico e cronologie riproducibili.
- Confrontare strategie efficienti e meno efficaci; misurare attese, inattività e carenze.
- Puntare a circa otto ore di gioco per la strategia efficiente di riferimento, verificando anche percorrenze da 32 ore o più.
- Raccogliere prove con giocatori per verificare ritmo e comprensibilità.
- Implementare lo storico dei risultati e la classifica secondo il perimetro deciso.

Criterio di uscita: completamento dello scenario dimostrato, report di bilanciamento riproducibile e risultati legati alle versioni delle regole. Le otto ore diventano una misura documentata, non una dichiarazione iniziale.

Una classifica pubblica, se scelta, richiede una specifica della verifica dei risultati e del trattamento di salvataggi e partite modificate prima della pubblicazione. Non è già inclusa come servizio server implementato.

## M7 — Consolidamento e distribuzione

- Verificare compatibilità dei salvataggi, errori di lettura e scrittura e migrazioni supportate.
- Misurare prestazioni con quantità elevate di strutture e partite lunghe.
- Verificare navigazione da tastiera, leggibilità, ridimensionamento e spiegazione degli stati.
- Eseguire prove sui sistemi operativi scelti; dichiarare quelli non verificati.
- Preparare pacchetti, istruzioni di avvio e note della prima release.

Criterio di uscita: pacchetti provati sulle piattaforme dichiarate e documentazione allineata al comportamento reale.

## Regola di avanzamento

Una milestone si completa con evidenze dei criteri di uscita, non con la sola creazione delle cartelle. Ogni passaggio aggiorna [handoff](HANDOFF.md), [changelog](../CHANGELOG.md) e ADR interessati.
