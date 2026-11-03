# V60-E3b2-RC2 — correzioni e pulizia del progetto

**3 novembre 2026 · versione candidata · da validare su Windows .NET 10**

## Risultato del gate precedente

L'utente ha eseguito `validate.cmd` sulla E3b2-RC1: **377 test, 372 passati, 5 falliti**. Tutti e cinque appartengono a `V14SqliteSaveStoreTests`. Build degli assembly completata; esecuzione di `validate.cmd` arrestata dalla suite test. Le prove successive (benchmark, smoke, publish) **non risultano completate da quel run**.

## Diagnosi dei cinque fallimenti e correzioni

- `V14RoundTripUsesIndependentGenerationAndOldStoreRefusesIt`
- `NonemptySchoolProgressAndResearchUnlocksRoundTrip`
- `MilitaryIndividualTrainingProgressRoundTrips`
- `OverwriteKeepsConsistentS1BackupOfPreviousCheckpoint`

I quattro test sopra confrontavano `OtherBuildings` come una sequenza ordinata. Nel formato SQLite `building_stock` è un insieme di righe con chiave `(slot,building_id)` e viene letto per `building_id` alfabetico, mentre lo snapshot v13 originale usa l'ordine del catalogo. Il contenuto coincideva: il test attribuiva una falsa differenza all'ordine. Ora la funzione `Equivalent()` ordina le collezioni con chiave (`OtherBuildings`, `ProductiveFamilies`, `PurchaseCounters`, `UnlockedTechnologyIds`) per la rispettiva chiave, preservando il controllo di **tutti i record e valori**, e continua a confrontare le code di avanzamento in ordine reale. Nessuna modifica allo store produttivo.

- `MissingTierAndMissingTrainingPositionAreRejected`

Il test aveva uno studente con una sola posizione di formazione (`position=0`), ma eseguiva `DELETE` di `position=1`: 0 righe eliminate, nessuna corruzione, nessuna eccezione. Ora prepara **due studenti nelle posizioni 0 e 1** e rimuove esplicitamente `position=0`: rimane la posizione `1` senza la `0`, ossia una lacuna rilevabile. Verifica con `Assert.Equal(1, ...)` che le operazioni SQL di corruzione abbiano effettivamente modificato una riga. Il caricamento deve respingere sia la mancanza di un livello sia una posizione interna mancante.

**Limite noto da affrontare prima dell’attivazione dei salvataggi v3:** la rimozione della *sola posizione finale* può risultare indistinguibile da una coda più corta perché la tabella `progression_school_state` non registra un numero atteso degli studenti in formazione. Questo test controlla la contiguità, non garantisce il riconoscimento di qualsiasi troncamento. Nella fase successiva valuteremo la memorizzazione e verifica esplicita del numero di posizioni, con adeguata versione di schema.

## Organizzazione repository

Sono stati spostati **25 file** dalla radice nelle cartelle:

- [`docs/history/checkpoints/`](../history/checkpoints/README.md) — da `G1_CHECKPOINT.md` fino a G1-D9.
- [`docs/history/integrity/`](../history/integrity/README.md) — manifest `G1_*_SOURCE_INTEGRITY.json`.
- [`docs/history/releases/`](../history/releases/README.md) — rapporti `V58_*`, `V59_*`, `V60_*`.

L'originale `00_START_HERE.md` è in `docs/history/START_HERE_BEFORE_RC2.md`, mentre la guida attuale è [`docs/START_HERE.md`](../START_HERE.md). Le vecchie versioni del README e dell'indice documentale sono state archiviate. Sono stati rivisti i collegamenti Markdown dei documenti spostati.

La radice mantiene soltanto `README.md`, `CHANGELOG.md`, `LICENSE.txt`, progetto, configurazioni e script di avvio/manutenzione. I sorgenti precedenti, compreso `V14SqliteSaveStore.cs`, **non cambiano**.

## Validazione

Controlli locali: confronto con ZIP RC1, inventario del nuovo archivio, correttezza delle modifiche mirate, riferimenti documentali, eventuali controlli SQLite in Python. Non è disponibile .NET SDK 10 nel container: non dichiarare la suite RC2 superata prima del test Windows.

Da PowerShell, nella directory radice della **RC2** estratta in cartella nuova:

```powershell
.\validate.cmd
```

Obiettivo: **377 test passati su 377**, quindi successivi gate di simulazione e packaging. Se i nuovi test totali differiscono, verificare versione estratta e log effettivo.

## Roadmap

- v60-E3b-RC1: validata su Windows, prima della nuova implementazione Infrastructure.
- v60-E3b2-RC2: test SQLite corretti, pulizia documentale, **da validare**.
- E3b/3: sessione avanzata isolata e gestione degli stati v14, soltanto dopo il gate verde.
- E3b/4: GUI ricerca e costruzione livelli; E4: attivazione simultanea dei prezzi progressivi, scuola e upgrade, benchmark anti-softlock.
