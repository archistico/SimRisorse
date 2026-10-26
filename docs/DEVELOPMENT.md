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

Lo script esegue restore con lock, build Release, test e simulazioni di 60 secondi, 8 e 32 ore; genera quattro render della UI, pubblica il desktop e ne verifica il ciclo nativo su Windows. Output locali in `.local-cache/m3/`, con risultati TRX isolati per esecuzione e riepilogo `validation.json`. `-SkipNativeSmoke` esclude soltanto il lancio automatico del pacchetto.

Il ripristino offline disabilita l'audit solo per il comando corrente; non modificarne la configurazione globale per aggirare errori di rete. Gli script disabilitano la telemetria opzionale Avalonia per il processo di build e ripristinano le variabili ambientali alla fine.

La sorgente offline deve contenere anche le nuove versioni: una cache contenente soltanto Avalonia 11 non basta. In questo workspace i pacchetti di Avalonia 12 e del nuovo provider sono stati acquisiti da NuGet via HTTPS con verifica dei certificati e poi ripristinati nella cache locale; il client .NET continua a incontrare un errore TLS. Il confronto puntuale con il feed VulnerabilityInfo è documentato nel report, distinto dall'audit CLI.

Il gioco si avvia con `run-game.cmd`; la verifica usa un salvataggio privato, mentre il desktop interattivo usa LocalApplicationData. Non usare il file personale come fixture. Ulteriori regole nel [formato dei salvataggi](SAVE_FORMAT.md).

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
