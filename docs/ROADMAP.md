# Roadmap unica — SimRisorse RC105 (hotfix test Zecca)

**Siamo qui:** RC105 candidata, **1360 test attesi**. Ultima versione completamente confermata RC95 (**1245 test**). Primo tentativo RC105: 2 errori compilazione (`CS0119`, `xUnit2013`) corretti; secondo tentativo Windows: **build riuscita, 1359/1360 test**, 1 errore nella fixture RC98 per mancato aggiornamento della capacità dopo rimozione Fonderia. Secondo hotfix corregge soltanto il test; **nuovo gate F1 ancora da verificare**.

## P0 — Validazione e stabilità

Usare solo ` .\validate.cmd ` su Windows; confermare **1360/1360**, zero errori e gate F1 completo, inclusi screenshot. Il report precedente **1359/1360** non valida il secondo hotfix. La pulizia `.local-cache` deve conservare l'ultimo run, anche se fallito. Consegnare sempre ZIP completo senza manifest SHA-256.

## P1 — Salvataggio unico e cantieri

**Fatto:** preflight RC104 + prototipo SQLite generation 7 RC105 (tabelle città e deposito nello stesso file, ricevute e identità; nessuna scrittura giocabile). **Da fare:** autorità incorporata nel file, sessioni e revisioni, transazioni città+cantieri+rate, recupero e backup dopo crash, migrazione non distruttiva dal generation 6 e dal composito. Attivazione controllata nei comandi solo quando tutto è verificato. [ADR 0116](adr/0116-prototipo-sqlite-generation7-citta-e-cantieri.md).

## P2 — Bilanciamento economico

Provare con il clock vero, senza risorse sintetiche: Scuola → Estrazione I → Carbone → filiere → Zecca, seconda Zecca, lavoratori, capienza e rate. Valutare curva A opt-in, senza cambiare prezzi predefiniti in anticipo.

## P3 — Gameplay e GUI

Consolidare filiere, ricerca, abitazioni, popolazione, spedizioni, interfaccia e distribuzione. Conservare la GUI pixel art canonica e i contratti già approvati.

## P4 — Punteggio e classifica locale

**Dopo** bilanciamento e gameplay, definire condizioni di fine partita e formula punti **versionata e verificabile**, poi classifica locale delle partite. [Piano](design/PUNTEGGIO_CLASSIFICA.md).

## P5 — Ultima fase: classifica online via API

Solo alla fine, estendere la classifica locale con API, privacy e validazione lato server dei risultati. Mai accettare punteggi arbitrari del client.
