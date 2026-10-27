# Convenzioni di sviluppo

Queste convenzioni guidano la nuova implementazione. La bozza in `legacy/` conserva il proprio stile storico e non deve essere riformattata incidentalmente.

## Prima di lavorare

1. Leggere [handoff](HANDOFF.md), [roadmap](ROADMAP.md) e ADR pertinenti.
2. Controllare lo stato Git e preservare le modifiche già presenti.
3. Distinguere requisiti confermati, scelte tecniche accettate e proposte ancora aperte.
4. Se una regola di gioco manca, definirla prima dell'algoritmo che la applica. Le scelte tecniche locali reversibili non richiedono un ADR per ogni classe.

## Codice e confini

- Documentazione di progetto in italiano; nuovi identificatori di codice in inglese, coerenti con i nomi dei progetti.
- Nomi che descrivono azioni e concetti di gioco; evitare contenitori generici senza una responsabilità chiara.
- Stato modificato attraverso operazioni che proteggono gli invarianti; le viste leggono dati coerenti e non modificano collezioni interne del motore.
- Interfacce nei punti di sostituzione reali, come persistenza e caricamento dei contenuti. Non introdurre repository generici o wrapper per ogni classe.
- Validare gli input esterni al confine e applicare nel Core le regole che devono valere per ogni chiamante.
- Rendere espliciti unità, precisione, arrotondamenti, ordine delle operazioni e limiti numerici.
- Non aggiungere accesso al disco, SQL, tipi grafici o letture dell'orologio nel Core.
- Non mantenere codice della nuova applicazione dipendente dalla soluzione legacy.

M2 ha introdotto `SimRisorse.slnx`, `global.json`, `.editorconfig`, proprietà di build e versioni centrali alla radice. Le proprietà del prodotto si applicano ai progetti denominati `SimRisorse.*`; la bozza conserva target e gestione dei pacchetti storici. La prova M1 mantiene le proprie configurazioni locali. La CI resta da creare.

Core, Application e Simulation usano solo il framework .NET 10 e i riferimenti interni. Infrastructure aggiunge il provider SQLite; Desktop e i suoi test usano Avalonia. Le versioni sono centralizzate e fissate con lock file; l'SDK consente feature band .NET 10 successive senza prerelease. Per ogni aggiornamento verificare compatibilità e patch del runtime, non dedurle dalla sola compilazione.

Baseline corrente: Avalonia 12.1.4 e Microsoft.Data.Sqlite 10.0.14. I test Desktop usano xUnit 3.2.2 e HarfBuzz esplicito nel builder headless; gli altri test rimangono xUnit 2.9.3. M1 conserva le proprie versioni storiche. Decisione e verifica nell'[ADR 0008](adr/0008-avalonia-12.md) e nel [report](validation/AVALONIA_12.md).

## Comandi della soluzione definitiva

Dalla radice, con accesso alle sorgenti NuGet:

```powershell
.\validate.cmd
```

Per una cache completa e ripristino offline:

```powershell
.\validate.cmd -OfflinePackageSource .local-cache/nuget
```

Lo script esegue restore con lock, build Release, test, il controllo di raggiungibilità dei due cataloghi e dodici simulazioni: Foundation a 60 secondi/8 ore/32 ore; Civil bootstrap a 121 secondi/8 ore/32 ore, recupero a 662, magazzino a 1260, filiera Pane a 610 e 660, Case grandi a 880 secondi e 8 ore. Esegue inoltre **quattro confronti**: `--benchmark-housing` (tre strategie storiche `civil-growth-4` su otto ore, `housing-benchmark.json`), `--benchmark-housing-accelerated` (stesse strategie, `civil-growth-5`, `housing-accelerated-benchmark.json`), `--benchmark-bread-scale` (220 abitanti, quattro strategie, `bread-scale-benchmark.json`) e **`--benchmark-bread-high-density`** (320 abitanti, quattro strategie, `bread-high-density-benchmark.json`). Richiede **nove render**, incluso lo stato Pane a 1040×740, isolati per esecuzione in `runs/<id>/screenshots/`; pubblica il desktop e verifica il ciclo nativo Windows per entrambi gli scenari. Output in `.local-cache/m4/`, con risultati TRX isolati e riepilogo `validation.json`. `-SkipNativeSmoke` esclude soltanto il lancio automatico del pacchetto. **Il più recente gate riferito superato dall'utente è v24/M4.10 (174 casi xUnit attesi). v25/v26 prevedono 177 casi e non sono ancora stati eseguiti qui.** Non risultano disponibili in archivio i report grezzi delle conferme precedenti.

**M4.12/v26 — sola lettura dei risultati:** dopo una validazione Windows, `powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\scripts\report-m4.ps1` confronta i quattro JSON ed espone tempi, carenze e costi effettivi. Per salvarne l'output indicare `-OutputPath .local-cache\m4\m4-report.md`. Il [manuale](validation/M4_BENCHMARK_READER.md) precisa la provenienza e i limiti dei dati; il lettore non ripete il calcolo e non modifica le partite.

Per cancellare gli output di compilazione di tutti i progetti (soluzione definitiva, test, tools, experiments e legacy):

```powershell
.\clean.cmd -DryRun   # elenca soltanto le cartelle
.\clean.cmd
```

Rimuove solo le cartelle `bin` e `obj` accanto a ogni `.csproj`, all'interno del repository; non tocca `.git`, `.local-cache` (pacchetti NuGet e artefatti di validazione), salvataggi personali o collegamenti. Se un file è in uso (gioco, IDE o test aperti) segnala la cartella ed esce con codice 1. Dopo la pulizia serve un restore prima di `--no-restore`: `validate.cmd` lo esegue da solo.

Il ripristino offline disabilita l'audit solo per il comando corrente; non modificarne la configurazione globale per aggirare errori di rete. Gli script disabilitano la telemetria opzionale Avalonia per il processo di build e ripristinano le variabili ambientali alla fine.

La sorgente offline deve contenere anche le nuove versioni: una cache contenente soltanto Avalonia 11 non basta. In questo workspace i pacchetti di Avalonia 12 e del nuovo provider sono stati acquisiti da NuGet via HTTPS con verifica dei certificati e poi ripristinati nella cache locale; il client .NET continua a incontrare un errore TLS. Il confronto puntuale con il feed VulnerabilityInfo è documentato nel report, distinto dall'audit CLI.

Il gioco si avvia con `run-game.cmd` nello scenario civile; `--scenario economy-foundation` riapre lo scenario storico e il suo slot distinto. La verifica usa salvataggi privati, mentre il desktop interattivo usa LocalApplicationData. Non usare il file personale come fixture. Ulteriori regole nel [formato dei salvataggi](SAVE_FORMAT.md).

Il simulatore mantiene Foundation come default per conservare la timeline di riferimento. Per il civile: `dotnet run --project tools/SimRisorse.Simulation -c Release --no-restore -- --scenario civil-growth --strategy bootstrap --seconds 121`; `--strategy recovery --seconds 662` prova il recupero dalla carenza, `storage --seconds 1260` il magazzino, `bread --seconds 660` la filiera del Pane e `advanced --seconds 880` le Case grandi e i primi residenti avanzati. `advanced --hours 8` verifica il riempimento della capacità abitativa. Sono cronologie dichiarate, non un ottimizzatore di strategie.

Per confrontare costi, tempi e blocchi di tre politiche: `dotnet run --project tools/SimRisorse.Simulation -c Release --no-restore -- --benchmark-housing`. Regole e limitazioni sono nella [scheda di confronto](scenarios/HOUSING_BENCHMARK.md). Il risultato è un JSON leggibile anche senza interfaccia grafica; non costituisce la misura della durata del gioco completo.

Per confrontare una filiera Pane, due complete immediate, una seconda completa tardiva e un potenziamento **snello** su un avvio sintetico a 220 residenti: `dotnet run --project tools/SimRisorse.Simulation -c Release --no-restore -- --benchmark-bread-scale`. Il quarto profilo `lean-chain` di **M4.7** misura acquisti reali, lavoratori, attività dei quattro anelli, Pane prodotto/consumato e 1.800 secondi riconciliati per strategia. **Gate v17 riferito superato dall'utente**, senza JSON grezzo a disposizione: la [scheda](scenarios/BREAD_SCALE_BENCHMARK.md) ne documenta metriche, asserzioni e limiti.

Per stressare le stesse quattro strategie oltre il limite teorico della filiera snella: `dotnet run --project tools/SimRisorse.Simulation -c Release --no-restore -- --benchmark-bread-high-density`. Questo **nuovo gate M4.8 non ancora eseguito** parte da 320 abitanti, 360 posti e Cibo dimensionato, fino a 350 abitanti in 1.800 secondi. Misura anche i tempi per raggiungere 340/350. Vedere [scenario ad alta densità](scenarios/BREAD_HIGH_DENSITY_BENCHMARK.md) e report `bread-high-density-benchmark.json`.

Dopo ogni modifica del catalogo: `dotnet run --project tools/SimRisorse.Simulation -c Release --no-restore -- --check-catalog --scenario civil-growth`. Stampa strutture raggiungibili, risorse rinnovabili e problemi (prerequisito irraggiungibile, costo non ottenibile o oltre la capacità, input non rinnovabile, ricetta oltre la capacità) ed esce con codice 1 se ce ne sono. La validazione lo esegue per entrambi gli scenari.

Per la sola simulazione, dopo il build: `dotnet run --project tools/SimRisorse.Simulation -c Release --no-restore -- --hours 8`. Per rieseguire l'audit online senza alterare i percorsi dei pacchetti negli assets usare `dotnet list SimRisorse.slnx package --no-restore --vulnerable --include-transitive`.

Per riprodurre le verifiche M1 usare [le istruzioni della prova](../experiments/stack-probe/README.md). Lo script usa una cache NuGet nel workspace per evitare differenze di percorso tra ambienti di esecuzione, disabilita la telemetria opzionale Avalonia per il processo e registra esplicitamente se il ripristino offline ha escluso l'audit delle dipendenze.

## Documentazione delle scelte

| Tipo di modifica | Documento da aggiornare |
| --- | --- |
| Regola o obiettivo di gioco | Game design |
| Confine architetturale, dipendenza o compatibilità | ADR e architettura |
| Ordine o criteri del lavoro futuro | Roadmap |
| Modifica realizzata | Changelog |
| Stato operativo e punto di ripresa | Handoff |
| Comando o percorso di avvio cambiato | README pertinente |

Un cambio di idea non deve trasformare retroattivamente una proposta in una decisione già confermata. Un ADR superato conserva il collegamento alla decisione successiva.

## Resoconto obbligatorio a ogni incremento

Per consentire all'utente di pianificare anche quando non può eseguire test, ogni consegna deve contenere nell'ordine:

1. **Dove siamo:** milestone e sottofase (es. M4.6), versione/checkpoint, **ultima validazione effettivamente confermata**.
2. **Cosa abbiamo fatto:** cambiamenti circoscritti, moduli/documenti, decisioni già approvate e risultati effettivamente misurati.
3. **Cosa stiamo implementando:** obiettivo, confini, impatto su regole, formato di salvataggio e interfaccia.
4. **Cosa ci aspetta:** prossimo incremento concreto, criterio di uscita, dipendenze e posizione nella [roadmap](ROADMAP.md).
5. **Cosa rimane aperto:** test non eseguiti, rischi, scelte da confermare ed eventuali limiti noti.

Usare gli stati **Proposto**, **Implementato non validato**, **Verificato**, **Rinviato**. Separare esplicitamente la dichiarazione di test superati *riferita dall'utente*, l'evidenza tecnica *eseguita e ispezionata* e il conteggio *atteso*. Non presentare risultati aritmetici teorici o screenshot previsti come misure di un'esecuzione reale. Una revisione della roadmap non costituisce autorizzazione a modificare meccaniche di gioco non approvate.

## Verifiche proporzionate alla modifica

Per documentazione e riordini: controllare collegamenti, percorsi, differenze, integrità dei file spostati e riferimenti dei progetti. Non è necessario avviare una suite di gioco per una modifica solo editoriale.

Per il motore: testare regole osservabili e invarianti, evitando test che copiano semplicemente l'implementazione. Casi prioritari:

- acquisto senza materiali sufficienti che non altera lo stato;
- consumo e produzione coerenti con una ricetta;
- contesa delle stesse risorse e personale insufficiente;
- capacità piena e politica dell'eccesso;
- pausa senza avanzamento, ripresa e azioni alle frontiere temporali;
- assegnazione e rilascio dei lavoratori senza duplicazioni;
- ripetibilità di una cronologia di azioni.

Per la persistenza: usare un database temporaneo reale e verificare round trip, versioni, atomicità e stato parzialmente avanzato. Una simulazione non deve dipendere dal numero di aggiornamenti dell'interfaccia.

Per l'interfaccia: verificare i flussi modificati e la spiegazione dei blocchi. Per i pacchetti: provare il risultato sui sistemi operativi dichiarati, separando quelli verificati da quelli solo potenzialmente supportati.

Per il bilanciamento: eseguire scenari riproducibili e registrare versione delle regole, strategia e metriche. Un test automatico di completamento non sostituisce la valutazione del divertimento da parte di giocatori.

## Completamento di una funzionalità

- Il comportamento e i casi limite interessati sono definiti.
- I confini dell'architettura restano rispettati.
- Le verifiche pertinenti sono eseguite e i limiti rimasti sono dichiarati.
- I cambiamenti al formato di salvataggio o al bilanciamento hanno una gestione esplicita delle versioni.
- Documenti e handoff rispecchiano lo stato reale.
- Nessun salvataggio personale o output generato è aggiunto al codice sorgente.

## File locali e riferimenti

`.local-cache/` conserva gli artefatti locali raccolti durante il riordino; `.local-data/` è disponibile per eventuali dati di gioco di sviluppo. Entrambe sono ignorate da Git. Non ignorare indiscriminatamente tutti i file `.db`: un futuro database di test può essere intenzionalmente versionato.

Gli screenshot restano in `docs/references/screenshots/`. Nuovi asset del gioco avranno un percorso dedicato quando serviranno e non verranno confusi con i riferimenti di analisi.
