# RC62 — F1-D05e.2b.1-fix: test generation 6 e visibilità filiere v15

Data 05/11/2026. Ultimo checkpoint completamente validato: **RC60, 756 test**. Risultato Windows RC61: **761 test eseguiti, 760 riusciti e 1 fallito**, senza errori del compilatore C#. Il singolo fallimento `F1D05d2FoodIndustryPersistenceTests.SchemaFiveRoundTripPreservesEveryIntermediateMilliunitAndSixBuildingCounts`, linea 81, confronta `PRAGMA user_version` con 5 mentre lo store RC61 scrive correttamente generation 6.

## Modifiche al salvataggio

- Il test rinominato `SchemaSixRoundTripPreservesEveryIntermediateMilliunitAndSixBuildingCounts` aspetta `user_version=6` e `snapshot_version=6`. Tutte le asserzioni sulla conservazione degli stock intermedi al millesimo, sulle sei strutture, sulle scorte di città/Deposito e sugli altri stati rimangono operative.
- Anche i percorsi temporanei del test e della copia `.v15.backup.sqlite3` descrivono ora `generation6`. Nessuna modifica a `V15SqliteFoodSaveStore`, schema SQL, formati o motore.
- I database di sviluppo v15 gen. 5 restano incompatibili, rifiutati senza scrittura; un eventuale DB v15 già presente deve essere spostato/rinominato **a gioco chiuso**, mantenendo intatto `%LOCALAPPDATA%\SimRisorse\experiments\civil-growth-v14.sqlite3`.

## Modifiche all'avvio

`DesktopOptions.Parse([])` seleziona ora `UseV15=true`, nel percorso `%LOCALAPPDATA%\SimRisorse\experiments\civil-growth-v15.sqlite3`. Di conseguenza `run-game.cmd` avvia automaticamente la v15; `--v15` la seleziona esplicitamente. La v14 storica resta disponibile con `run-game.cmd --v14` o `--experimental-v14`, utilizzando il percorso v14 preesistente. `--legacy-v13`, `--scenario` e `--smoke-test` conservano i percorsi precedenti. Il parser continua a rifiutare conflitti fra modalità e percorsi riservati. Non c'è migrazione automatica né cambio della GUI v14.

## Individuazione delle sei strutture alimentari

In `V15PreviewWindow.axaml` la lista **Edifici** ora usa `VisibleBuildings`; il pulsante **Filiere alimentari (6)** mostra solo `stable`, `hunter`, `butcher`, `garden`, `orchard`, `food-depot` con le etichette italiane Stalla, Cacciatore, Macellaio, Orto, Frutteto e Deposito alimentare. Il pulsante diventa **Tutti gli edifici** per ripristinare il catalogo generale. Non cambia il set reale `Buildings`, la selezionabilità degli edifici bloccati, i costi, il rendering pixel art, la struttura delle sette colonne Risorse o gli inventari. Il filtro usa le stesse istanze delle righe e resta stabile agli aggiornamenti del clock.

## Limiti e roadmap

L'interfaccia v15 resta una schermata separata ma ora è quella di avvio normale; in seguito si potrà consolidare il codice grafico comune senza stravolgere il layout storico. **F1-D05e.2b.2** deve ancora sostituire i comandi civici v14 ereditati e rimuovere lo slot `food-store` residuo in memoria. Poi E4a benchmark, E4b costi quasi esponenziali, Zecca con minimo 10 addetti per unità. Nessuna formula economica cambia in RC62.
