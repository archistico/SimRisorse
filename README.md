# SimRisorse

Gioco gestionale in cui si parte da materiali di base e si sviluppano filiere produttive, popolazione e capacità militare per arrivare a costruire tutti i tipi di edificio previsti dallo scenario.

La scelta centrale è come investire risorse e lavoratori: espandere la produzione adesso oppure risparmiare per il prossimo sblocco. Soldati, castelli e missioni di conquista delle isole fanno parte della visione di gioco.

## Stato del progetto

**27 ottobre 2026: M0–M4 completate; M5.1b Oro verificata su Windows con 204/204 test.** La [soluzione](SimRisorse.slnx) comprende motore, GUI, salvataggi SQLite e simulatore. `.\validate.cmd` completa build senza avvisi/errori, 16 cronologie più la prova di crescita, quattro benchmark, 17 render e prove native di entrambi gli scenari. [Risultati M5.1b](docs/validation/M5_1B_GOLD.md).

Lo scenario `civil-growth-7` ha **17 tipi di edifici e 14 risorse**. La Casa grande sblocca le miniere di ferro, carbone e oro; le rispettive miniere sbloccano le due fonderie. L'utente ha scelto i parametri A del Ferro e poi dell'Oro, mantenendo **S1**, conversione e backup. Il Ferro ha priorità con carbone scarso; una seconda miniera sostiene più lavorazioni. Crescita, Cibo/Pane, formato SQLite v2 e GUI retro **Consolas monospace** sono conservati. Carne fuori dalla prima campagna e demolizione rinviata.

**Prima campagna scelta:** [22 tipi](docs/design/FIRST_CAMPAIGN_PROPOSAL.md), di cui 17 presenti e 5 futuri. Per vincere occorrerà possederli tutti, con il Castello conclusivo sbloccato da una conquista obbligatoria. Vittoria e missioni sono contratti scelti, ancora da implementare. Prezzi di Forgia/Zecca e regole militari/navali restano decisioni separate.

La [roadmap](docs/ROADMAP.md) conserva la progressione M4 e distingue M5.1a verificata dagli incrementi futuri. I benchmark abitativi `civil-growth-4` e `-5` restano riproducibili. I dossier [M5.2](docs/design/M5_2_FORGE_MINT_PREFEASIBILITY.md), [M5.3](docs/design/M5_3_ARMY_PREFEASIBILITY.md) e [M5.4](docs/design/M5_4_NAVAL_PREFEASIBILITY.md) preparano regole ancora da scegliere: esercito, navi, isole e vittoria non sono ancora implementati.

L'impostazione tecnica adottata è un monolite modulare con funzionalità verticali e un motore indipendente dall'interfaccia. **C#/.NET 10, Avalonia 12.1.4/MVVM e SQLite sono lo stack desktop**, con Microsoft.Data.Sqlite 10.0.14. Core e Application usano solo il framework e i riferimenti interni previsti. Windows è verificato, Linux/macOS non ancora. Compatibilità dei salvataggi e controllo puntuale delle dipendenze sono descritti nel [report della migrazione](docs/validation/AVALONIA_12.md); l'audit CLI dell'ambiente incontra ancora un errore TLS.

## Scelte di gioco confermate

- Edifici gestiti tramite elenco e schede, senza collocamento su una mappa.
- Identità grafica: **pixel art retro 8-bit essenziale**, con prototipo monospace approvato e verificato; rifiniture e DPI fisici da ampliare.
- Produzione e progressione dipendenti dalle quantità di edifici e dall'impiego dei lavoratori.
- Presenza di soldati e progressione verso castelli e missioni di conquista delle isole.
- Partite sospendibili: nessun avanzamento durante la pausa o quando non si gioca.
- Classifica basata sul tempo di gioco necessario a completare l'obiettivo.
- Obiettivo di bilanciamento: circa **8 ore** per una strategia molto efficiente; **32 ore o più** con decisioni meno efficaci. Non è ancora un risultato misurato né un minimo teorico dimostrato.

La definizione esatta di vittoria, il ruolo obbligatorio delle isole e le formule economiche restano da precisare in [Game design](docs/GAME_DESIGN.md).

## Documentazione

| Documento | Contenuto |
| --- | --- |
| [Game design](docs/GAME_DESIGN.md) | Visione, regole confermate, proposte, questioni aperte e glossario |
| [Architettura](docs/ARCHITECTURE.md) | Confini, dipendenze e struttura prevista |
| [ADR](docs/adr/README.md) | Decisioni architetturali con stato e motivazioni |
| [Roadmap](docs/ROADMAP.md) | Stato verificato, M4.6–M4.9, gate prima di M5, M5–M7, dipendenze e criteri di completamento |
| [Sviluppo](docs/DEVELOPMENT.md) | Convenzioni, verifiche e gestione dei cambiamenti |
| [Handoff](docs/HANDOFF.md) | Stato operativo e prossimo lavoro da riprendere |
| [Dossier pre-M5 / D0](docs/design/PRE_M5_DECISION_GATE.md) | Inventario dei 43 riferimenti, 12 equivalenti presenti, opzioni di vittoria, isole, militari e criteri di scelta |
| [Scheda operativa D0](docs/design/D0_DECISION_BOARD.md) | Scelte ricevute e parametri residui; regole correnti v6, continuità S1 e prossimi incrementi |
| [Prima campagna](docs/design/FIRST_CAMPAIGN_PROPOSAL.md) | Catalogo dei 22 tipi scelto dall'utente; proposta separata di vittoria, prima conquista e casi di accettazione |
| [Prova industriale A/B](experiments/industry-probe/README.md) | Esperimento opzionale eseguito: ferro/oro, prezzi ipotetici e politiche di acquisto; regole separate dalla partita |
| [Prefattibilità M5.1](docs/design/M5_1_INDUSTRY_PREFEASIBILITY.md) | Ferro/carbone, oro, contesa, due ipotesi di prezzi, capacità, migrazioni e gate di verifica (nessun codice M5) |
| [Prefattibilità M5.2](docs/design/M5_2_FORGE_MINT_PREFEASIBILITY.md) | Forgia e Zecca, Spade e Monete, limite del Carbone, strategie a fasi/continua e decisioni economiche aperte (nessun codice M5) |
| [Prefattibilità M5.3](docs/design/M5_3_ARMY_PREFEASIBILITY.md) | Caserma, reclute, soldati, popolazione condivisa, addestramento, salvataggi e decisioni militari aperte (nessun codice M5) |
| [Prefattibilità M5.4](docs/design/M5_4_NAVAL_PREFEASIBILITY.md) | Cantiere, navi, flotta persistente/monouso, primo viaggio, trasporto, salvataggi e scelte navali ancora da approvare |
| [Changelog](CHANGELOG.md) | Modifiche effettivamente realizzate |
| [Riferimenti visivi](docs/references/README.md) | Screenshot originali e analisi delle evidenze |
| [Scenario della prova](docs/scenarios/ECONOMY_PROBE.md) | Valori e regole provvisorie del primo ciclo economico |
| [Verifica dello stack](docs/validation/M1_STACK_PROBE.md) | Ambiente, 23 test, pubblicazione e limiti della prova |
| [Scenario del Core](docs/scenarios/ECONOMY_FOUNDATION.md) | Catalogo, dotazione e regole attualmente eseguibili |
| [Verifica del motore](docs/validation/M2_CORE.md) | 45 test, simulazioni lunghe e limiti dell'audit |
| [Salvataggi](docs/SAVE_FORMAT.md) | Percorso, formato, checkpoint e comportamento in caso di errore |
| [Verifica del desktop](docs/validation/M3_DESKTOP.md) | 82 test, rendering e avvio nativo Windows |
| [Migrazione Avalonia 12](docs/validation/AVALONIA_12.md) | Baseline dei pacchetti, compatibilità dei checkpoint e dipendenze |
| [Scenario civile M4](docs/scenarios/CIVIL_GROWTH.md) | Regole eseguibili di abitazioni, crescita, alimentazione e recupero |
| [Confronto abitazioni M4](docs/scenarios/HOUSING_BENCHMARK.md) | Tre strategie riproducibili e criteri per valutare costi, carenze e tempi |
| [Audit incentivi case M4.13](docs/scenarios/HOUSING_INCENTIVE_AUDIT.md) | Vantaggio 2×, quattro combinazioni a costo minimo, capienza e domanda alimentare teorica |
| [Scala Pane M4](docs/scenarios/BREAD_SCALE_BENCHMARK.md) | Quattro strategie da 220 abitanti, gate v17 riferito superato, misure grezze non disponibili |
| [Stress Pane alta densità M4.8](docs/scenarios/BREAD_HIGH_DENSITY_BENCHMARK.md) | Confronto da 320 a 350 abitanti, ipotesi, indicatori e gate riferito superato con v21 (report grezzi assenti) |
| [Capacità filiera Pane M4](docs/scenarios/BREAD_CHAIN_CAPACITY.md) | Limiti teorici dei quattro anelli e confronto tra potenziamento completo e snello |
| [Direzione grafica 8-bit](docs/PIXEL_ART_UI.md) | Stile scelto, requisiti di leggibilità e piano del prototipo futuro |
| [Valutazione filiera Carne](docs/scenarios/MEAT_DESIGN_REVIEW.md) | Alternative e decisioni ancora aperte, senza introdurre meccaniche non approvate |
| [Studio demolizione M4](docs/scenarios/DEMOLITION_M4_REVIEW.md) | Confronto dei rimborsi e condizioni per non perdere capacità, persone o scorte |
| [Verifiche M4](docs/validation/M4_CIVIL_GROWTH.md) | Cronologia delle verifiche; gate corrente superato con 177/177 test |
| [Verifica M5.1a](docs/validation/M5_1A_IRON.md) | Ferro/carbone giocabili, conversione S1, 191 test e misure da nuova partita |
| [Verifica M5.1b](docs/validation/M5_1B_GOLD.md) | Oro giocabile, priorità sul carbone, conversione v6→v7, 204 test e misure |
| [Risultati misurati M4](docs/validation/M4_MEASURED_RESULTS.md) | Quattro benchmark letti, nove render ispezionati, provenienza e decisioni residue |
| [Lettura benchmark M4](docs/validation/M4_BENCHMARK_READER.md) | Utility v26, quattro JSON storici/accelerati, confronti misurati e limiti della prova |

## Struttura attuale

```text
README.md
CHANGELOG.md
LICENSE.txt
docs/
  adr/
  references/screenshots/
experiments/
  stack-probe/
scripts/
src/
  SimRisorse.Core/
  SimRisorse.Application/
  SimRisorse.Infrastructure/
  SimRisorse.Desktop/
tests/
  SimRisorse.Core.Tests/
  SimRisorse.Application.Tests/
  SimRisorse.Infrastructure.Tests/
  SimRisorse.Desktop.Tests/
tools/
  SimRisorse.Simulation/
SimRisorse.slnx
validate.cmd
run-game.cmd
```

Nessun progetto della soluzione definitiva dipende dalla bozza storica Windows Forms o da `experiments/`. La cartella `legacy/winforms/`, citata nei documenti storici, non era inclusa nell'archivio sorgente ricevuto per questa lavorazione.

Le cache locali sono state raccolte in `.local-cache/`, esclusa da Git. Le immagini di riferimento sono state trasferite dalla precedente cartella `screnshot/`, mantenendo nomi e contenuto.

## Esecuzione

Per validare la soluzione definitiva con sorgenti NuGet accessibili:

```powershell
.\validate.cmd
```

Con la cache completa del workspace si può usare `validate.cmd -OfflinePackageSource .local-cache/nuget`; è possibile indicare un'altra cache purché contenga le versioni correnti. Lo script compila, esegue i test, controlla simulazioni di 60 secondi, 8 e 32 ore e pubblica/avvia il desktop su Windows con un salvataggio privato di verifica. Il percorso offline non esegue l'audit delle dipendenze.

Quando saranno disponibili i quattro JSON del gate, si potranno **leggere senza ripetere simulazioni** con `powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\scripts\report-m4.ps1`. Il comando stampa un rapporto Markdown con le misure effettive; la [guida](docs/validation/M4_BENCHMARK_READER.md) spiega come conservarlo e quali conclusioni non permette.

Per aprire il gioco dopo la validazione:

```powershell
.\run-game.cmd
```

Le viste Edifici e Risorse mostrano quantità, capacità, costi, personale e flussi previsti al secondo. Puoi impartire ordini da 1, 5 o 10 unità. Il gioco parte in pausa e tenta di riprendere il salvataggio esistente.

Per i residenti avanzati, la barra Crescita include una diagnosi del rifornimento di Pane: differenzia capacità nominale dei Forni, input insufficienti e deposito quasi pieno, con un'autonomia indicativa della scorta senza nuove forniture. Il gate v17 comprendente questa diagnostica è stato riferito superato dall'utente; i dettagli visivi e i relativi screenshot non sono allegati.

La diagnosi proviene dal modello di lettura di Application, indipendente dalla grafica. Se Cibo e Pane mancano insieme vengono segnalate entrambe le necessità. Miniere e fonderie spiegano prerequisiti, input e capacità mancanti. Gate corrente **204/204 test superati**; avvisi sopra Costruisci, geometria hover e layout industriali verificati alle due risoluzioni. La base grafica è accettata; DPI fisici da ampliare.

Nel nuovo scenario le Case piccole offrono 8 posti, le Case grandi 32 e richiedono un Forno già costruito. Con spazio, Cibo e Pane quando necessario, il ritmo base è di un abitante ogni 60 secondi validi, accelerato dalle case aggiuntive fino a 2×. Tutti consumano 0,010 Cibo/s a persona; oltre la capienza delle Case piccole serve anche 0,015 Pane/s a persona avanzata. Le carenze fermano la crescita senza rimuovere abitanti. I valori sono provvisori per lo sviluppo, non il bilanciamento finale.

Salvataggio ogni minuto e alla chiusura ordinata, oltre al comando manuale. Il nuovo scenario usa `%LOCALAPPDATA%\SimRisorse\civil-growth.sqlite3` su Windows. Il vecchio checkpoint `game.sqlite3` rimane separato e si riprende con `run-game.cmd --scenario economy-foundation`. La ripresa conserva la crescita parziale e non produce risorse per il tempo trascorso a gioco chiuso. Dettagli nel [formato dei salvataggi](docs/SAVE_FORMAT.md).

Dopo la validazione, per eseguire il nuovo motore senza finestre:

```powershell
dotnet run --project tools/SimRisorse.Simulation -c Release --no-restore -- --seconds 60
dotnet run --project tools/SimRisorse.Simulation -c Release --no-restore -- --scenario civil-growth --strategy bootstrap --seconds 121
```

L'esperimento iniziale si avvia ancora con `scripts/run-stack-probe.cmd`, dopo la sua validazione separata: vedere il [README M1](experiments/stack-probe/README.md). La bozza storica Windows Forms non è la nuova applicazione e non è inclusa in questo archivio.

## Licenza

Il repository contiene il testo della GNU Affero General Public License v3 in [LICENSE.txt](LICENSE.txt), conservato senza modifiche.
