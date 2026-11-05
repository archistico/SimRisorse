# Piano test RC62 — test SQLite generation 6 e avvio v15

## Dati di partenza

- RC60: 756/756 confermati dall'utente.
- RC61: compilazione C# riuscita, 760/761 test riusciti; unico fallimento Infrastructure linea 81: expected 5 / actual 6 per `PRAGMA user_version`.
- RC62: correzione del test e **1 test Desktop aggiuntivo** per il filtro delle sei strutture; **762 test attesi**. Gli altri test dell'avvio sono aggiornati al nuovo comportamento. SDK .NET 10 non disponibile nell'ambiente di confezionamento; non dichiarare la suite superata.

## Comandi Windows

```powershell
.\validate.cmd
.\run-game.cmd                  # v15 predefinita: Macellaio e altre filiere visibili in Edifici
.\run-game.cmd --v15            # v15 esplicita
.\run-game.cmd --v14            # vecchia GUI e sua partita separata
```

## Verifiche automatiche

1. `F1D05d2FoodIndustryPersistenceTests.SchemaSixRoundTripPreservesEveryIntermediateMilliunitAndSixBuildingCounts`: `user_version=6`, `snapshot_version=6`, sei strutture e inventari conservati integralmente.
2. `V15DesktopIntegrationTests.DefaultV15AndExplicitV14UseDistinctReservedSlots`: default v15, flag espliciti v15/v14, protezione incrociata dei percorsi, incompatibilità modalità.
3. `V14PreviewTests.ExplicitV14LaunchPreservesTheExistingV14SaveSlot` e `DesktopTests.ScenariosHaveDistinctPersonalSlotsAndSmokeTestsUsePrivatePaths`: v14 e legacy mantengono i rispettivi percorsi anche dopo il nuovo default.
4. `V15DesktopIntegrationTests.FoodIndustryFilterShowsAllSixSitesAndCanRestoreFullCatalog` (**nuovo**): premere filtro, verificare esattamente le sei strutture, ripetere `Refresh()`, ripristinare l'intero catalogo senza mutare lo stato della città.
5. Suite cumulativa di 762 test, incluse economia, capacità, ricerca, salvataggio/ripresa, GUI a sette colonne e stress combinato.

## Verifiche GUI manuali

- Con un DB v15 gen. 6 nuovo, l'avvio senza argomenti apre la GUI v15 e il menu Edifici contiene il pulsante **Filiere alimentari (6)**. Selezionarlo, controllare il Macellaio e i prerequisiti; tornare a **Tutti gli edifici**.
- Con `--v14`, verificare che la vecchia partita e la tabella Risorse precedente rimangano identiche; non aprire la v14 attraverso un percorso v15.
- Con un DB v15 gen. 5, verificare il messaggio di blocco, senza sovrascrittura. Per partire con una nuova gen. 6 rinominare il solo file v15 prima dell'avvio; nessuna operazione sul salvataggio v14.
- Verificare che il selettore di ricerca e l'esercito siano raggiungibili, e che le costruzioni bloccate non presentino l'azione `+1`.
- Se desiderato, `dotnet run --project tools/SimRisorse.Simulation -c Release --no-restore -- --audit-v15 --seconds 120 --profile combined` per stress dopo il gate xUnit.

## Esito

Passaggio di gate richiesto: compilazione e **762/762 xUnit** su Windows. Audit statico XML, JSON, Markdown, DDL SQLite, ZIP/SHA-256 eseguibile in fase di confezionamento ma non sostituisce dotnet.
