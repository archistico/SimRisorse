# Salvataggi della prima versione desktop

Stato: implementato in M3. Non è compatibile automaticamente con l'esperimento M1, che usa un file e un modello differenti.

## Percorso e comportamento

Il file predefinito è `LocalApplicationData/SimRisorse/game.sqlite3`. Su Windows corrisponde normalmente a `%LOCALAPPDATA%\SimRisorse\game.sqlite3`.

- Un singolo slot, ripreso automaticamente in pausa all'avvio.
- Salvataggio manuale con "Salva", periodico ogni minuto e alla chiusura ordinata.
- Nessun avanzamento per il tempo trascorso a programma chiuso.
- La creazione di una nuova partita richiede conferma; sostituisce la sessione in memoria, poi il nuovo checkpoint viene scritto ai successivi salvataggi.
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

| Identificatore | Valore attuale |
| --- | --- |
| `PRAGMA application_id` | `0x53494D52` |
| `PRAGMA user_version` | `1` |
| Versione del contratto snapshot | `1` |
| Scenario | `economy-foundation` |
| Regole | `economy-foundation-1` |

La versione del file è distinta dalla versione del contratto e dal bilanciamento. Il loader rifiuta file estranei, versioni non supportate e regole che non corrispondono allo scenario attivo. Non sono presenti migrazioni automatiche o fingerprint del catalogo.

## Schema SQLite v1

Tre tabelle `STRICT`, con quantità intere e controlli di non negatività:

| Tabella | Colonne principali | Responsabilità |
| --- | --- | --- |
| `game_save` | slot, snapshot_version, scenario_id, rules_version, elapsed_ticks, population | Identità e avanzamento |
| `resource_stock` | slot, resource_id, milliunits | Scorte esatte in millesimi |
| `building_stock` | slot, building_id, count, workers | Edifici e assegnazioni |

`slot` identifica il checkpoint `1`. Chiavi composte impediscono duplicati di risorse o edifici; i riferimenti allo slot sono vincoli esterni. Le scorte nulle possono essere omesse e vengono ricostruite come zero. Lo snapshot contiene invece tutti i tipi di edificio, inclusi quelli con quantità zero.

`elapsed_ticks` conserva unità di 100 ns, compreso il residuo inferiore al secondo. Non viene salvato un timestamp da utilizzare per produzione offline.

Il formato limita a 10.000 le righe di risorse e quelle di edifici per checkpoint. Questo è un limite tecnico del primo adattatore, non un numero di strutture costruibili: il campo `count` aggrega molte costruzioni dello stesso tipo.

## Atomicità e controlli

Metadati, rimozione delle vecchie righe e inserimento del nuovo stato fanno parte di una sola transazione. Un errore nell'ultima fase annulla anche le modifiche precedenti. Il caricamento legge i gruppi di dati nella stessa transazione differita, così non mescola checkpoint diversi.

L'adattatore controlla identità del file, forma dei dati, limiti e quantità. Il caso d'uso applicativo passa poi lo snapshot al Core per verificare catalogo, popolazione, capacità e assegnazioni prima della sostituzione. Un oggetto costruito da chiamanti esterni del port deve comunque rispettare gli invarianti di dominio: il writer non conosce tutte le regole dello scenario.

I database estranei e le versioni di file o snapshot future non sono sovrascritti. Per lavorare con uno stato incompatibile mantenendo l'originale si usa un percorso differente; non si aggiorna silenziosamente una vecchia partita alle regole correnti.

## Concorrenza e gestione degli errori

`SaveGameHandler` cattura uno snapshot coerente sotto il coordinatore, poi svolge I/O fuori dal lock. Un gate distinto serializza salvataggi e caricamenti. Il salvataggio cattura lo stato dopo l'attesa del gate.

`LoadGameHandler` mette in pausa, registra una revisione e carica fuori dal lock. Al completamento sostituisce la sessione solo se nessun altro comando o avvio ne ha invalidato la revisione.

Il port espone errori applicativi: file assente, formato non supportato, regole incompatibili, dati invalidi, indisponibilità, operazione superata o annullata. Il driver SQLite non è esposto nei ViewModel. L'annullamento non interrompe necessariamente una scrittura sincrona già iniziata.

## Verifiche e limiti

Round trip, precisione, ricaricamento in pausa, rifiuto di versioni future e database estranei, rollback mediante trigger ed errori di chiusura sono verificati nel [report M3](validation/M3_DESKTOP.md).

Non sono ancora implementati salvataggi multipli, backup gestiti dalla UI, migrazioni di scenario o validazione competitiva per una classifica pubblica. Un arresto forzato può perdere il progresso successivo all'ultimo checkpoint; non è stato simulato uno spegnimento elettrico.
