**Nota stato G1-D19 (03/11/2026):** questa guida contiene cronologie tecniche precedenti e comandi ancora utili, ma i conteggi test storici non descrivono il rilascio corrente. Ultimo gate Windows comunicato: **v60-E3b4-RC1 427/427**; RC3 GUI correttiva attende **439 test** e audit visivo. Per priorità e stato attuale leggere [CURRENT_STATE](CURRENT_STATE.md), [HANDOFF](HANDOFF.md) e [ROADMAP](ROADMAP.md).

---

> **v59-RC1:** oltre a `\.\validate.cmd`, è disponibile `dotnet run --project tools/SimRisorse.Simulation -c Release --no-restore -- --benchmark-military-capacity`. I risultati automatici sono in `.local-cache/m4/military-capacity-benchmark.json` e `.local-cache/m4/military-balance-8-hours.json`. [Protocollo M5.3c](validation/M5_3C_V59_MILITARY_BALANCE.md).

> **RC5 — simulazioni storiche e correnti.** Le strategie `industry` e `gold` senza `--rules` usano `civil-growth-13`; per confrontare i vecchi risultati M5.1/M5.2 si usa esplicitamente `--rules historical-v9` (solo `bootstrap`, `industry`, `gold`). `--rules legacy` continua a selezionare `civil-growth-4`. Il validatore esegue entrambe le famiglie di controlli e non le confonde. Vedere [rapporto](validation/V58_RC5_VALIDATOR_VERSION_DRIFT.md).

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

Lo script esegue restore con lock, build Release, test e raggiungibilità dei due cataloghi. Le **16 cronologie** comprendono le dodici Foundation/Civil storiche, due prove `industry` e due `gold`, più crescita accelerata corrente v7. Sono ora **cinque benchmark**: quattro abitativi/Pane v4/v5 e a 220/320 abitanti, più M5.1c sul Carbone (cinque profili da 180 secondi). Richiede **17 render** standard; i test del dettaglio producono altri due PNG, per **19 immagini** nello stesso `runs/<id>/screenshots/`. Pubblica e verifica il ciclo nativo Windows per entrambi gli scenari. Output in `.local-cache/m4/`, TRX isolati e `validation.json`; `-SkipNativeSmoke` esclude soltanto il lancio del pacchetto. **Ultimo gate riferito dall'archivio locale: 206/206 test**, zero avvisi/errori, **prima** del nuovo benchmark. [Interfaccia verificata](validation/DESKTOP_UI.md); [gate precedente dell'Oro](validation/M5_1B_GOLD.md); [M5.1c ancora da eseguire](validation/M5_1C_INDUSTRY_SCALE.md).

**M4.12/v26 — sola lettura dei risultati:** dopo una validazione Windows, `powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\scripts\report-m4.ps1` confronta i quattro JSON ed espone tempi, carenze e costi effettivi. Per salvarne l'output indicare `-OutputPath .local-cache\m4\m4-report.md`. Il [manuale](validation/M4_BENCHMARK_READER.md) precisa la provenienza e i limiti dei dati; il lettore non ripete il calcolo e non modifica le partite.

Per cancellare gli output di compilazione di tutti i progetti (soluzione definitiva, test, tools, experiments e legacy):

```powershell
.\clean.cmd -DryRun   # elenca soltanto le cartelle
.\clean.cmd
```

Rimuove solo le cartelle `bin` e `obj` accanto a ogni `.csproj`, all'interno del repository; non tocca `.git`, `.local-cache` (pacchetti NuGet e artefatti di validazione), salvataggi personali o collegamenti. Se un file è in uso (gioco, IDE o test aperti) segnala la cartella ed esce con codice 1. Dopo la pulizia serve un restore prima di `--no-restore`: `validate.cmd` lo esegue da solo.

Il ripristino offline disabilita l'audit solo per il comando corrente; non modificarne la configurazione globale per aggirare errori di rete. Gli script disabilitano la telemetria opzionale Avalonia per il processo di build e ripristinano le variabili ambientali alla fine.

La sorgente offline deve contenere le versioni correnti: una cache contenente soltanto Avalonia 11 non basta. Durante la migrazione ad Avalonia 12 il client .NET incontrava un errore TLS; l'acquisizione dei pacchetti via HTTPS e il confronto puntuale con VulnerabilityInfo sono documentati nel [rapporto della migrazione](validation/AVALONIA_12.md). Sono evidenze di quell'esecuzione, distinte dall'audit CLI; non attestano lo stato attuale del feed.

Dalla RC31 `run-game.cmd` apre la GUI v14 sul suo slot SQLite separato. `--legacy-v13` apre la GUI classica e `--scenario economy-foundation` riapre lo scenario storico v13 e il relativo slot distinto; `--smoke-test` continua a usare la modalità legacy per compatibilità con le prove native. La verifica usa salvataggi privati, mentre il desktop interattivo usa LocalApplicationData. Non usare il file personale come fixture. Ulteriori regole nel [formato dei salvataggi](SAVE_FORMAT.md).

Il simulatore mantiene Foundation come default per conservare la timeline di riferimento. Per il civile: `dotnet run --project tools/SimRisorse.Simulation -c Release --no-restore -- --scenario civil-growth --strategy bootstrap --seconds 121`; `--strategy recovery --seconds 662` prova il recupero dalla carenza, `storage --seconds 1260` il magazzino, `bread --seconds 660` la filiera del Pane e `advanced --seconds 880` le Case grandi e i primi residenti avanzati. `advanced --hours 8` verifica il riempimento della capacità abitativa. Sono cronologie dichiarate, non un ottimizzatore di strategie.

Per confrontare costi, tempi e blocchi di tre politiche: `dotnet run --project tools/SimRisorse.Simulation -c Release --no-restore -- --benchmark-housing`. Regole e limitazioni sono nella [scheda di confronto](scenarios/HOUSING_BENCHMARK.md). Il risultato è un JSON leggibile anche senza interfaccia grafica; non costituisce la misura della durata del gioco completo.

Per confrontare una filiera Pane, due complete immediate, una seconda completa tardiva e un potenziamento **snello** su un avvio sintetico a 220 residenti: `dotnet run --project tools/SimRisorse.Simulation -c Release --no-restore -- --benchmark-bread-scale`. Il quarto profilo `lean-chain` di **M4.7** misura acquisti reali, lavoratori, attività dei quattro anelli, Pane prodotto/consumato e 1.800 secondi riconciliati per strategia. Il confronto è stato eseguito: [risultati misurati](validation/M4_MEASURED_RESULTS.md) e [regole dello scenario](scenarios/BREAD_SCALE_BENCHMARK.md).

Per stressare le stesse quattro strategie oltre il limite teorico della filiera snella: `dotnet run --project tools/SimRisorse.Simulation -c Release --no-restore -- --benchmark-bread-high-density`. Il benchmark M4.8, già eseguito e mantenuto nel gate corrente, parte da 320 abitanti, 360 posti e Cibo dimensionato, fino a 350 abitanti in 1.800 secondi. Misura anche i tempi per raggiungere 340/350. Vedere [scenario ad alta densità](scenarios/BREAD_HIGH_DENSITY_BENCHMARK.md), [risultati misurati](validation/M4_MEASURED_RESULTS.md) e report `bread-high-density-benchmark.json`.

L'[esperimento M5.2](../experiments/forge-mint-probe/README.md) è **separato dal gioco e da `validate.cmd`**: crea un catalogo A/B provvisorio con Forgia/Zecca in memoria a partire dal catalogo storico `civil-growth-7`, senza alterare il gioco corrente `civil-growth-8` e senza salvare partite. Il contratto A è già approvato e Forgia/Spade ora è implementata nel gioco, ma la prova A/B resta indipendente. Contiene sei profili e sei guardie F8/F9 (lotti con deposito quasi pieno ed esatta capienza, priorità del Carbone), ripetuti con avanzamento intero/frazionario. Esecuzione: `dotnet run --project experiments/forge-mint-probe/SimRisorse.ForgeMintProbe.csproj -c Release`. **Ancora non compilato/eseguito nell'ambiente di preparazione**.

Per verificare M5.1c senza aprire il desktop: `dotnet run --project tools/SimRisorse.Simulation -c Release --no-restore -- --benchmark-industry-scale`. Calcola cinque profili sintetici di contesa del Carbone con prezzi invariati, controllo delle scorte a ogni secondo e replay a passi frazionati. Il gate salva `industry-scale-benchmark.json`; [ipotesi e risultati teorici attesi](validation/M5_1C_INDUSTRY_SCALE.md). **Non ancora eseguito con .NET nel presente checkpoint.**

Per il benchmark preparatorio M5.2c sul catalogo attivo v9: `dotnet run --project tools/SimRisorse.Simulation -c Release -- --benchmark-forge-mint-coal`. Confronta una miniera di Carbone a fasi, due miniere in parallelo e una miniera con tutte le lavorazioni presidiate, a 1.800/3.600 secondi; include replay frazionari e ripristino da checkpoint. Il gate completo salva `forge-mint-coal-benchmark.json`. [Protocollo M5.2c](validation/M5_2C_COAL_BENCHMARK.md). **Da eseguire e misurare su Windows**, senza alterare le regole del gioco o i salvataggi dell'utente.

Per la nuova traiettoria M5.2d **dalla partita iniziale**, senza risorse o edifici industriali pre-caricati: `dotnet run --project tools/SimRisorse.Simulation -c Release -- --benchmark-forge-mint-campaign`. Reimpiega il percorso civile storico, poi acquista Forgia/Zecca a risorse disponibili, confronta una/due Miniere di carbone e la riassegnazione degli addetti. Il gate completo salva `forge-mint-campaign-benchmark.json`. [Protocollo M5.2d](validation/M5_2D_REAL_START.md). **Non ancora compilato o eseguito in questo ambiente**.

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

Le immagini di riferimento restano in `docs/references/screenshots/`. Gli screenshot dell'applicazione destinati alla documentazione sono in `docs/screenshots/`: il [README della cartella](screenshots/README.md) descrive origine e aggiornamento di `colonia.png`. Gli artefatti completi dei test restano nella cache locale; non aggiungerli tutti al repository. Le icone attuali del gioco sono definite in `PixelGlyph`, separate dai riferimenti di analisi.
