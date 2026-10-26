# 0010 — Capacità di magazzino e conversione esplicita delle regole

- Stato: Accettato
- Data: 2026-10-27
- Ambito: secondo incremento M4, con parametri di scenario provvisori
- Origine: passo successivo dell'[handoff](../HANDOFF.md) dopo l'[ADR 0009](0009-civil-growth-and-save-generations.md)

## Contesto

Fino a `civil-growth-1` la capacità di ogni risorsa era un valore fisso dello scenario (1.000). Con la produzione di base a 1/s legno lavorato e pietra si riempiono in circa venti minuti di gioco e i produttori restano fermi senza alternative. L'ampliamento della capacità cambia il catalogo, quindi la versione delle regole: serve decidere anche cosa accade al checkpoint civile già salvato.

Scelte dell'utente del 2026-10-27: magazzino generico che aumenta tutte le risorse, senza personale; conversione esplicita del vecchio checkpoint conservando l'originale.

## Decisione

### Capacità derivata dagli edifici

`BuildingDefinition` acquisisce l'effetto opzionale `StoragePerBuilding`, un insieme di risorse validato dal catalogo come costi e ricette. `GameScenario.Capacity` resta la capacità **di base**; la capacità effettiva è base + Σ(edifici × effetto), calcolata da `GameSession.StorageCapacity`. Non viene salvata: deriva sempre dagli edifici dello snapshot, come i posti abitativi.

- Costruzione, previsione e avanzamento usano la stessa capacità effettiva; nell'avanzamento è costante perché gli edifici non cambiano durante una chiamata.
- La capacità è disponibile dal passo successivo alla costruzione, coerentemente con la regola dello spazio iniziale del passo (ADR 0006).
- Un acquisto che supererebbe il limite numerico fallisce con `NumericLimit` senza mutazioni. Il ripristino verifica le scorte rispetto alla capacità degli edifici salvati; un overflow è un dato invalido.
- Le scorte iniziali di uno scenario devono stare nella capacità degli edifici iniziali.

Il magazzino non ha ricetta né lavoratori: compare come struttura non produttiva, come la casa. Senza demolizione (Q-08) la capacità non può diminuire, quindi non esiste ancora il caso di scorte oltre il limite.

### Regole `civil-growth-2` e conversione

Il contenuto è nella [scheda dello scenario](../scenarios/CIVIL_GROWTH.md). Il formato snapshot/file resta v2: un nuovo tipo di edificio è una riga in `building_stock`.

- Le conversioni note sono funzioni esplicite in `Application/Scenarios`, applicate in `GameRuntime.TryRestore` solo per lo stesso scenario e solo verso le regole attive. `civil-growth-1 → civil-growth-2` aggiunge il magazzino con quantità zero e conserva tempo, popolazione, progresso, scorte ed edifici. Regole sconosciute o scenari diversi restano `IncompatibleRules`.
- Lo snapshot convertito passa da tutti i controlli di `GameSession.Restore`. Un checkpoint dichiarato v1 ma già contenente il magazzino è rifiutato come dato invalido.
- `PersistenceResult.UpgradedFromRules` comunica la conversione; il desktop la spiega al giocatore.
- Prima di sovrascrivere un checkpoint della stessa generazione ma con scenario o regole diversi, l'adattatore SQLite ne crea una copia con `VACUUM INTO` accanto al file: `<nome>.<regole>.backup.sqlite3`, poi `-2`, `-3`… Le copie esistenti non vengono mai sovrascritte; senza un nome libero il salvataggio fallisce. Generazioni di file diverse continuano a essere rifiutate senza copia.

## Alternative e conseguenze

Un magazzino con personale avrebbe aggiunto un vincolo di popolazione prima che esistano strumenti per i colli di bottiglia; magazzini per categoria richiedono categorie di risorse ancora non definite. L'effetto per risorsa consente comunque entrambe le varianti in futuro senza cambiare il Core.

Uno slot separato per `civil-growth-2` avrebbe lasciato la partita in corso non più giocabile dal default; ripartire da zero l'avrebbe persa. La conversione esplicita mantiene la partita e la copia di sicurezza ne conserva l'originale. La copia viene creata al primo salvataggio, non al caricamento, così un caricamento resta privo di effetti sul disco.

Le conversioni future devono restare funzioni dichiarate e verificate: non vengono dedotte confrontando cataloghi. Il nome della copia contiene la versione delle regole ripulita dai caratteri non sicuri per un percorso.

## Verifica

[Report M4](../validation/M4_CIVIL_GROWTH.md), sezione magazzino: produzione ferma a capacità piena e ripresa dopo la costruzione, ripristino con capacità degli edifici, limite numerico atomico, scorte iniziali, catalogo invalido, conversione con progresso a 59 s, rifiuti delle regole sconosciute, copia di sicurezza su SQLite reale, messaggio desktop e cronologia `storage` del simulatore.
