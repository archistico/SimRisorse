# Salvataggi desktop

Stato: implementato per Foundation v1 e per Civil v2, con conversione esplicita delle regole civili e copia di sicurezza. Non è compatibile automaticamente con l'esperimento M1, che usa un file e un modello differenti.

## Percorso e comportamento

Il nuovo default è `LocalApplicationData/SimRisorse/civil-growth.sqlite3`. Su Windows corrisponde normalmente a `%LOCALAPPDATA%\SimRisorse\civil-growth.sqlite3`.

Il vecchio scenario conserva `LocalApplicationData/SimRisorse/game.sqlite3` e si apre con `run-game.cmd --scenario economy-foundation`. Non viene convertito, sostituito o applicato alle nuove regole durante l'avvio del civile.

- Un singolo slot per ciascuno dei due scenari, ripreso automaticamente in pausa all'avvio dello scenario scelto.
- Salvataggio manuale con "Salva", periodico ogni minuto e alla chiusura ordinata.
- Nessun avanzamento per il tempo trascorso a programma chiuso.
- La creazione di una nuova partita richiede conferma; mantiene lo scenario corrente e sostituisce la sessione in memoria, poi il nuovo checkpoint viene scritto ai successivi salvataggi.
- Un caricamento non valido lascia la partita corrente in pausa.
- Un avvio con file non caricabile sospende l'autosalvataggio della sessione iniziale di ripiego, per non perdere il file esistente.
- Se il salvataggio in chiusura fallisce, la finestra rimane aperta; l'uscita senza salvare è una scelta esplicita del giocatore.

Il timer di autosalvataggio non fa avanzare la simulazione. Può salvare anche scelte prese in pausa; salta operazioni già in corso e conferme di nuova partita aperte.

Per sviluppo e verifiche si può scegliere un altro file:

```powershell
.\run-game.cmd --save-path "E:\percorso\partita.sqlite3"
```

La modalità `--smoke-test` usa sempre un percorso di prova, fornito dal validatore oppure generato nella cartella temporanea. Non usa il salvataggio personale predefinito.

## Identità del formato

| Identificatore | Foundation | Civil Growth |
| --- | --- | --- |
| `PRAGMA application_id` | `0x53494D52` | `0x53494D52` |
| `PRAGMA user_version` | `1` | `2` |
| Versione del contratto snapshot | `1` | `2` |
| Scenario | `economy-foundation` | `civil-growth` |
| Regole | `economy-foundation-1` | `civil-growth-3` (converte `civil-growth-1` e `-2`) |

La versione del file è distinta dalla versione del contratto e dal bilanciamento. Il loader rifiuta file estranei, versioni non supportate e regole che non corrispondono allo scenario attivo, salvo le conversioni esplicite elencate sotto. Non sono presenti migrazioni di formato o fingerprint del catalogo.

## Conversioni di regole e copie di sicurezza

Le conversioni dichiarate sono elencate in `CivilGrowthScenario.Changes`: `civil-growth-1 → civil-growth-2` aggiunge il magazzino, `civil-growth-2 → civil-growth-3` fattoria, pozzo, mulino e forno, sempre con quantità zero e mantenendo il resto dello stato. Si applicano in sequenza. Formato file e snapshot restano v2, perché ogni nuovo edificio è una riga in `building_stock` e le nuove risorse con scorta zero non hanno righe. La capacità delle risorse non è salvata: deriva dagli edifici. Decisione nell'[ADR 0010](adr/0010-storage-capacity-and-rules-conversion.md).

Il caricamento converte solo in memoria. Il primo salvataggio che sostituirebbe un checkpoint della stessa generazione con scenario o regole diversi copia prima il file con `VACUUM INTO` in `<nome>.<regole>.backup.sqlite3`, nella stessa cartella (per esempio `civil-growth.civil-growth-1.backup.sqlite3`). Se il nome esiste si usa `-2`, `-3`…: le copie non vengono sovrascritte e, senza un nome libero, il salvataggio fallisce. I caratteri delle regole non sicuri per un percorso diventano `_`. Le copie si riaprono con `run-game.cmd --save-path`, con un programma che supporti quelle regole.

Nelle due generazioni implementate, versione del file e dello snapshot devono corrispondere. Il writer rifiuta anche la sovrascrittura tra v1 e v2: per generazioni diverse si usano percorsi separati. Decisione nell'[ADR 0009](adr/0009-civil-growth-and-save-generations.md).

## Schema SQLite v1

Tre tabelle `STRICT`, con quantità intere e controlli di non negatività:

| Tabella | Colonne principali | Responsabilità |
| --- | --- | --- |
| `game_save` | slot, snapshot_version, scenario_id, rules_version, elapsed_ticks, population | Identità e avanzamento |
| `resource_stock` | slot, resource_id, milliunits | Scorte esatte in millesimi |
| `building_stock` | slot, building_id, count, workers | Edifici e assegnazioni |

`slot` identifica il checkpoint `1`. Chiavi composte impediscono duplicati di risorse o edifici; i riferimenti allo slot sono vincoli esterni. Le scorte nulle possono essere omesse e vengono ricostruite come zero. Lo snapshot contiene invece tutti i tipi di edificio, inclusi quelli con quantità zero.

`elapsed_ticks` conserva unità di 100 ns, compreso il residuo inferiore al secondo. Non viene salvato un timestamp da utilizzare per produzione offline.

## Estensione SQLite v2

Le tre tabelle precedenti restano presenti. Il file v2 aggiunge una tabella `STRICT`:

| Tabella | Colonne | Responsabilità |
| --- | --- | --- |
| `population_state` | slot, growth_seconds | Secondi validi verso il prossimo abitante |

`slot=1` è chiave primaria e riferimento a `game_save`. `growth_seconds` è un intero non negativo; il Core verifica che sia inferiore all'intervallo delle regole attive (60 per Civil). Una riga mancante non significa progresso zero: il checkpoint è incompleto e viene rifiutato. Lo snapshot v2 richiede `GrowthProgressSeconds`; lo snapshot v1 non deve contenerlo.

`game_save.population` conserva il numero effettivo delle persone, non soltanto la dotazione iniziale. Il Core verifica anche capacità abitativa, popolazione iniziale minima dello scenario senza decessi e assegnazioni. Il nuovo abitante, i relativi consumi e il progresso residuo non vengono ricalcolati dal tempo totale.

Il formato limita a 10.000 le righe di risorse e quelle di edifici per checkpoint. Questo è un limite tecnico del primo adattatore, non un numero di strutture costruibili: il campo `count` aggrega molte costruzioni dello stesso tipo.

## Atomicità e controlli

Metadati, crescita v2, rimozione delle vecchie righe e inserimento del nuovo stato fanno parte di una sola transazione. Un errore nell'ultima fase annulla anche le modifiche precedenti. Il caricamento legge i gruppi di dati nella stessa transazione differita, così non mescola checkpoint diversi.

L'adattatore controlla identità del file, forma dei dati, limiti e quantità. Il caso d'uso applicativo passa poi lo snapshot al Core per verificare catalogo, popolazione, capacità e assegnazioni prima della sostituzione. Un oggetto costruito da chiamanti esterni del port deve comunque rispettare gli invarianti di dominio: il writer non conosce tutte le regole dello scenario.

I database estranei e le versioni di file o snapshot future non sono sovrascritti. Per lavorare con uno stato incompatibile mantenendo l'originale si usa un percorso differente; non si aggiorna silenziosamente una vecchia partita alle regole correnti.

## Concorrenza e gestione degli errori

`SaveGameHandler` cattura uno snapshot coerente sotto il coordinatore, poi svolge I/O fuori dal lock. Un gate distinto serializza salvataggi e caricamenti. Il salvataggio cattura lo stato dopo l'attesa del gate.

`LoadGameHandler` mette in pausa, registra una revisione e carica fuori dal lock. Al completamento sostituisce la sessione solo se nessun altro comando o avvio ne ha invalidato la revisione.

Il port espone errori applicativi: file assente, formato non supportato, regole incompatibili, dati invalidi, indisponibilità, operazione superata o annullata. Il driver SQLite non è esposto nei ViewModel. L'annullamento non interrompe necessariamente una scrittura sincrona già iniziata.

## Verifiche e limiti

Round trip, precisione, ricaricamento in pausa, rifiuto di versioni future e database estranei, rollback mediante trigger ed errori di chiusura sono verificati nel [report M3](validation/M3_DESKTOP.md). Ripresa esatta della crescita, rollback v2, stato parziale corrotto e protezione tra generazioni sono nel [report M4](validation/M4_CIVIL_GROWTH.md). La fixture M3 rimane leggibile in v1.

Non sono ancora implementati più slot per scenario, selezione grafica degli scenari, gestione delle copie dalla UI, migrazioni di formato, conversioni non dichiarate in `CivilGrowthScenario.Changes` o validazione competitiva per una classifica pubblica. Un arresto forzato può perdere il progresso successivo all'ultimo checkpoint; non è stato simulato uno spegnimento elettrico.
