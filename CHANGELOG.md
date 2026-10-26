# Changelog

Questo file registra le modifiche realizzate. Le attività previste appartengono alla [roadmap](docs/ROADMAP.md). Non è stata ancora pubblicata una versione della nuova applicazione.

## [Unreleased]

### 2026-10-27 — Script di pulizia

- Aggiunti `clean.cmd` e `scripts/clean.ps1`: cancellano `bin` e `obj` di ogni progetto del repository, con `-DryRun` per l'anteprima. Esclusi `.git`, `.local-cache` e collegamenti.

### 2026-10-27 — M4: analisi di raggiungibilità del catalogo

- Aggiunto `CatalogReachability.Analyze` nel Core: controllo statico che ogni struttura sia costruibile (prerequisiti, costi pagabili con risorse rinnovabili o con la dotazione iniziale, capacità di base o ampliabile) e che ogni ricetta abbia input rinnovabili ed entri nella capacità. Ignora tempo, persone e contesa.
- Simulatore: `--check-catalog [--scenario …]`, con codice di uscita 1 se uno sblocco è impossibile. La validazione lo esegue per entrambi gli scenari e salva i report.
- Test sugli scenari distribuiti (completi) e sulla catena delle conversioni di `civil-growth`. **140 test superati**; regole e salvataggi invariati.

### 2026-10-27 — M4: filiera del pane e regole `civil-growth-3`

- Aggiunti grano, acqua e farina e quattro strutture: fattoria (1 grano/s), pozzo (1 acqua/s), mulino (2 grano → 1 farina) e forno (1 farina + 1 acqua → 3 cibo), con prerequisiti mulino→fattoria e forno→mulino+pozzo. Valori provvisori: il pane affianca il pesce come fonte di cibo, più efficiente per lavoratore ma più costosa.
- Nuove regole `civil-growth-3`. Le conversioni sono dichiarate in `CivilGrowthScenario.Changes` e si concatenano: i checkpoint `civil-growth-1` e `civil-growth-2` vengono caricati con le nuove strutture a zero e l'originale viene copiato come prima.
- La tabella della vista Risorse ora scorre, con intestazione e nota fisse: le otto risorse non stavano nella finestra compatta.
- Simulatore: cronologia `bread`, inclusa nella validazione. Verificati **132 test**, nove simulazioni, otto render e ciclo nativo Windows. Aggiornati scenario, formato, report M4, roadmap e handoff.

### 2026-10-27 — M4: magazzino espandibile e regole `civil-growth-2`

- Aggiunto il Magazzino (100 legno lavorato + 60 pietra, senza personale): +500 capacità per ognuna delle cinque risorse. Valori provvisori scelti dall'utente.
- Introdotto l'effetto `StoragePerBuilding` nelle definizioni: la capacità effettiva deriva dagli edifici e viene usata da costruzione, previsione, avanzamento, ripristino e vista Risorse. Limite numerico atomico.
- Nuove regole `civil-growth-2`. I checkpoint `civil-growth-1` vengono convertiti al caricamento con zero magazzini e il desktop lo spiega; regole sconosciute restano rifiutate.
- L'adattatore SQLite copia il checkpoint in `<nome>.<regole>.backup.sqlite3` prima di sostituirlo con scenario o regole diversi, senza mai sovrascrivere copie esistenti. Formato file/snapshot invariato (v2).
- UI: descrizione, effetto e stato del magazzino. Simulatore: cronologia `storage`, inclusa nella validazione.
- Verificati **128 test**, build senza avvisi, otto simulazioni, otto render e ciclo nativo Windows. Aggiunto l'[ADR 0010](docs/adr/0010-storage-capacity-and-rules-conversion.md); aggiornati scenario, formato, report M4, roadmap e handoff.

### 2026-10-26 — M4: case, crescita e prima filiera alimentare

- Aggiunto lo scenario giocabile `civil-growth-1`, con casa piccola, pescatore e deposito che trasforma pesce in cibo, oltre ai tre produttori di base.
- Introdotti capacità abitativa, consumo civile esatto e crescita di persone intere con accumulo dei secondi validi; fame, pausa e case piene sospendono gli arrivi senza perdere il progresso.
- Condivise prenotazioni alimentari e ricette tra forecast e avanzamento. La previsione delle risorse include il consumo degli abitanti.
- Aggiunti posti abitativi, stato della crescita e fabbisogno nell'interfaccia; distinte strutture abitative e produttive e saldi negativi delle risorse.
- Creato snapshot/file SQLite v2 per la crescita parziale. Il nuovo default usa `civil-growth.sqlite3`; Foundation v1 resta disponibile con `--scenario economy-foundation` e non viene convertito o sovrascritto automaticamente.
- "Nuova partita" mantiene lo scenario corrente. Aggiunti argomenti del simulatore e cronologie `bootstrap` e `recovery`.
- Verificati **117 test**, build senza avvisi, sette simulazioni fino a 32 ore, otto render e ciclo nativo Windows per entrambi gli scenari, inclusa la ripresa a 59 secondi con un solo arrivo successivo.
- Aggiornati scenario, ADR 0009, formato dei salvataggi, roadmap e handoff. Magazzini espandibili e ulteriori filiere civili restano da implementare in M4; valori e durata del gioco completo non sono ancora bilanciati.

### 2026-10-26 — Avalonia 12 e preparazione dello scenario M4

- Aggiornata la soluzione definitiva ad Avalonia **12.1.4**, con lock file coerenti; la prova storica M1 resta separata.
- Migrati i test Desktop a xUnit 3.2.2, configurato HarfBuzz per il rendering headless e aggiornata l'esportazione PNG. Timer associati esplicitamente al dispatcher della finestra.
- Aggiornato Microsoft.Data.Sqlite a **10.0.14**, che risolve SQLitePCLRaw 2.1.13 e SQLite nativo 3.53.3, dopo aver rilevato l'advisory della precedente libreria nativa.
- Aggiunta una fixture di checkpoint M3 e verificato il ripristino esatto con il nuovo provider, senza modifiche al formato v1 o alle regole economiche.
- Build Release senza errori o avvisi; **83 test superati**, quattro render, simulazioni e ciclo nativo Windows completati. Il riepilogo di validazione registra anche le versioni dei pacchetti risolte.
- Confrontate 63 coppie pacchetto/versione con il feed delle vulnerabilità NuGet: nessuna corrispondenza dopo l'aggiornamento. L'audit CLI resta impedito dall'errore TLS dell'ambiente; dettagli nel [report](docs/validation/AVALONIA_12.md).
- Registrato l'[ADR 0008](docs/adr/0008-avalonia-12.md) e preparata una [scheda M4](docs/scenarios/CIVIL_GROWTH_DRAFT.md) con valori candidati per abitazioni, crescita e alimentazione, ordine del passo e prove richieste. Queste nuove regole non sono ancora implementate.

### 2026-10-26 — M3: prima partita desktop salvabile

#### Aggiunto

- Desktop Avalonia/MVVM e Infrastructure collegati alla soluzione definitiva, con quattro progetti di test complessivi e nessun riferimento ai prototipi.
- Viste Edifici e Risorse, costi e ricette, assegnazioni e costruzioni da 1/5/10 unità, produzione/consumo/saldo previsti al secondo e capacità visibili.
- Port applicativo di persistenza, salvataggi fuori dal dispatcher e controllo di revisione contro caricamenti tardivi.
- Formato SQLite v1 con metadati, quantità in millesimi, edifici e assegnazioni in una transazione atomica.
- Caricamento iniziale in pausa, checkpoint ogni minuto, salvataggio manuale e alla chiusura; conservazione della sessione se la scrittura fallisce.
- Protezione di file estranei e versioni future, conferma di nuova partita e uscita senza salvataggio solo su scelta esplicita dopo errore.
- Orologio monotono, pausa alla minimizzazione e gestione delle notifiche di sospensione/ripresa Windows, con fallback per interruzioni lunghe.
- Comando `run-game.cmd`, pubblicazione Windows nel validatore, documentazione del formato e ADR 0007.

#### Verificato

- Build Release senza errori o avvisi; **82 test superati** (36 Core, 18 Application, 12 Infrastructure, 16 Desktop).
- Quattro render a 1240×850 e 1040×740; simulazioni di 60 secondi, 8 e 32 ore.
- Pacchetto nativo Windows: salvataggio, nuova sessione, ricaricamento, sospensione simulata e chiusura completati con codice zero.
- Comando di avvio interattivo verificato in modalità smoke, senza usare il salvataggio personale.

Baseline locale Avalonia 11.3.18 e Microsoft.Data.Sqlite 10.0.9. L'audit online è stato ritentato e non completato per errore TLS. Popolazione dinamica, alimentazione, catalogo completo, militari, isole e bilanciamento restano nelle milestone successive.

### 2026-10-26 — Nuovi riferimenti di risorse e filiere

- Analizzate due schermate delle risorse e tre diagrammi di materiali, alimentazione, monete, esercito, colonie e strutture decorative.
- Aggiornato l'inventario a nove immagini e documentati i collegamenti visibili, le ambiguità e le conseguenze sulle milestone successive.
- Distinte quantità, capacità e variazioni senza attribuire unità temporali non mostrate; mantenuti separati abitanti, materiali, missioni e mezzi.
- Aggiornati game design e handoff. Nessuna modifica al codice, ai parametri dello scenario o alle immagini.

### 2026-10-26 — M2: motore economico e casi d'uso

#### Aggiunto

- Soluzione definitiva `SimRisorse.slnx` con Core, Application, due progetti di test e strumento Simulation, senza riferimenti a bozza o esperimento.
- Catalogo immutabile con identificatori stabili, validazione di riferimenti e cicli dei prerequisiti.
- Risorse esatte in millesimi su `long`, capacità, acquisti multipli atomici e assegnazioni intere dei lavoratori.
- Pianificatore unico per produzione e previsioni, con prenotazioni di materiali e spazio per priorità esplicita.
- Tempo di gioco esplicito, pausa, residui inferiori al secondo e snapshot immutabili validati.
- Funzionalità verticali per nuova partita, costruzione, assegnazione, rilascio, pausa, avanzamento e lettura dello stato.
- Coordinamento delle mutazioni e delle proiezioni per impedire doppie spese o assegnazioni concorrenti.
- Scenario economico di riferimento, ADR 0006 e report M2.
- Configurazione SDK, build, stile e pacchetti; lock file e comando `validate.cmd`.

#### Verificato

- Build Release con zero errori e avvisi; 45 test superati (36 Core, 9 Application e architettura).
- Simulazioni senza finestra di 60 secondi, 8 e 32 ore, con tempo e quantità entro i limiti attesi.
- Configurazioni storiche della bozza e pacchetti dell'esperimento rimasti separati.

L'audit online delle dipendenze è stato tentato, ma non completato a causa di un errore TLS dell'ambiente. La nuova UI, l'adattatore SQLite definitivo e il bilanciamento completo restano lavoro successivo.

### 2026-10-26 — M1: scenario ridotto e prova desktop

#### Aggiunto

- Scheda del primo scenario economico: tre strutture, otto abitanti, acquisti e assegnazioni manuali, ricette e pausa.
- Prova eseguibile isolata in `experiments/stack-probe/`, con modello indipendente dalla UI, finestra Avalonia/MVVM e adattatore SQLite.
- Salvataggio transazionale, caricamento validato e ripresa in pausa, compreso il tempo residuo inferiore al secondo.
- 23 test per economia, tempo, conservazione dei lavoratori, persistenza e interfaccia headless con rendering Skia.
- Script riproducibile di ripristino, build, test, pubblicazione e avvio nativo di verifica; comando per aprire la prova interattiva.
- Versioni centralizzate e lock file limitati all'esperimento.
- Report M1 con evidenze e limiti delle verifiche.

#### Verificato e deciso

- Build Release con zero errori e avvisi; tutti i 23 test superati; pacchetto framework-dependent avviato su Windows.
- Rendering a 1180×790 e 1020×720, con comandi principali visibili; cronometro oltre 24 ore.
- Adozione di C#/.NET 10, Avalonia/MVVM e SQLite per la prima implementazione desktop, registrata nell'ADR 0005.

La prova usa pacchetti già disponibili offline: non certifica versioni più recenti, sicurezza delle dipendenze, altri sistemi operativi o bilanciamento del gioco completo. Il Core definitivo resta M2.

### 2026-10-26 — Impostazione del progetto

#### Aggiunto

- README con descrizione, stato reale del progetto e percorsi di lettura.
- Game design con scelte confermate, proposte e registro delle questioni aperte.
- Architettura prevista, convenzioni di sviluppo e criteri di verifica.
- Registro ADR con cinque decisioni/proposte e modello per quelle successive.
- Roadmap per milestone e handoff per la ripresa del lavoro.
- Indice e analisi dei quattro screenshot forniti come riferimento.
- README del prototipo con limiti noti e istruzioni per aprirlo.

#### Riorganizzato

- Soluzione, sorgenti e test Windows Forms trasferiti in `legacy/winforms/`, senza modifiche al loro contenuto e mantenendo i riferimenti relativi.
- Immagini trasferite da `screnshot/` a `docs/references/screenshots/`, senza rinominarle.
- Cache Visual Studio, output `bin/` e `obj/`, preferenze locali del progetto e cartelle vuote di analisi raccolti in `.local-cache/`. La pulizia è reversibile: questi file non sono stati cancellati.

#### Configurazione

- Aggiunte esclusioni Git mirate per cache locali, dati di gioco locali e file ausiliari SQLite.

### Limiti della modifica

- Nessuna modifica alle meccaniche della bozza.
- Nessuna migrazione del runtime, nuova applicazione Avalonia o persistenza SQLite implementata.
- Nessuna promessa di completamento in otto ore verificata attraverso simulazioni.
