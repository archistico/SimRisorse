# SimRisorse

Gioco gestionale in cui si parte da materiali di base e si sviluppano filiere produttive, popolazione e capacità militare per arrivare a costruire tutti i tipi di edificio previsti dallo scenario.

La scelta centrale è come investire risorse e lavoratori: espandere la produzione adesso oppure risparmiare per il prossimo sblocco. Soldati, castelli e missioni di conquista delle isole fanno parte della visione di gioco.

## Stato del progetto

**27 ottobre 2026: M0–M3 completate; gate tecnico M4 verificato su Windows con 177/177 test superati.** La [soluzione definitiva](SimRisorse.slnx) comprende motore, GUI, salvataggi SQLite e simulatore. Dopo la correzione dei pennelli condivisi di `PixelGlyph`, `.\validate.cmd` completa build Release senza avvisi/errori, simulazioni, quattro benchmark e prove native di entrambi gli scenari. Ispezionati quattro TRX e nove render, eseguito il [lettore dei benchmark M4](docs/validation/M4_BENCHMARK_READER.md); [misure, provenienza e limiti](docs/validation/M4_MEASURED_RESULTS.md).

Lo [scenario Civil Growth](docs/scenarios/CIVIL_GROWTH.md) usa `civil-growth-5`: ogni Casa piccola oltre la prima accelera la crescita del 15%, ogni Casa grande del 30%, fino al doppio della velocità, conservando la frazione nel salvataggio. Il prototipo GUI **pixel art retro 8-bit essenziale** include pittogrammi per 12 edifici e 9 risorse. M4 resta da chiudere con il giudizio utente sul prototipo e la classificazione della demolizione; Carne è fuori dalla prima campagna per D0.1. Esercito, isole, vittoria e classifica appartengono alle milestone successive; la [scheda D0](docs/design/D0_DECISION_BOARD.md) e gli studi M5 distinguono il perimetro scelto dalle regole ancora da approvare.

**Catalogo futuro scelto dall'utente (D0.1):** [prima campagna con 22 tipi](docs/design/FIRST_CAMPAIGN_PROPOSAL.md), i 12 già presenti più 10 industriali, militari e navali. Gli altri 21 riferimenti, inclusi Cacciatore e Allevamento, sono fuori dalla prima campagna. È una scelta di perimetro; i tipi nuovi non sono ancora implementati e prezzi, ricette e vittoria restano distinti.

La [roadmap operativa](docs/ROADMAP.md) distingue **M4.6–M4.8** (benchmark e diagnostica), **M4.9** (GUI compatta, hover), **M4.10** (crescita accelerata e migrazione sicura), **M4.11** (prototipo 8-bit), **M4.12** (lettura quantitativa dei risultati), **M4.13** (audit degli incentivi abitativi), **M4.14** (decisioni D0) e **M4.15** (prefattibilità navale). I dati numerici degli stress precedenti restano utili e i benchmark `civil-growth-4` sono preservati come confronto storico. I dossier [D0](docs/design/PRE_M5_DECISION_GATE.md), [M5.1](docs/design/M5_1_INDUSTRY_PREFEASIBILITY.md), [M5.2](docs/design/M5_2_FORGE_MINT_PREFEASIBILITY.md), [M5.3](docs/design/M5_3_ARMY_PREFEASIBILITY.md) e [M5.4](docs/design/M5_4_NAVAL_PREFEASIBILITY.md) restano **studi non approvati**. Seguono M5–M7: nessuna filiera o regola militare/navale è stata introdotta.

L'impostazione tecnica adottata è un monolite modulare con funzionalità verticali e un motore indipendente dall'interfaccia. **C#/.NET 10, Avalonia 12.1.4/MVVM e SQLite sono lo stack desktop**, con Microsoft.Data.Sqlite 10.0.14. Core e Application usano solo il framework e i riferimenti interni previsti. Windows è verificato, Linux/macOS non ancora. Compatibilità dei salvataggi e controllo puntuale delle dipendenze sono descritti nel [report della migrazione](docs/validation/AVALONIA_12.md); l'audit CLI dell'ambiente incontra ancora un errore TLS.

## Scelte di gioco confermate

- Edifici gestiti tramite elenco e schede, senza collocamento su una mappa.
- Identità grafica: **pixel art retro 8-bit essenziale**, confermata; v25 introduce un primo prototipo Avalonia, ancora da validare e perfezionare prima della GUI definitiva.
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
| [Scheda operativa D0](docs/design/D0_DECISION_BOARD.md) | Otto decisioni da approvare, dipendenze M4–M7, versione corrente `civil-growth-5` e scelta S1/S2 dei salvataggi |
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

La diagnosi proviene dal modello di lettura di Application, indipendente dalla grafica, ed è riutilizzata nel prototipo 8-bit. Se Cibo e Pane mancano insieme prima del primo residente avanzato, vengono segnalate entrambe le necessità senza modificare la priorità della simulazione. Il gate corrente è **verificato su Windows: 177/177 test superati**. Il prototipo conserva avvisi sopra Costruisci senza scorrimento, hover distinto dallo stato disabilitato e bordo a spessore invariato; i render alle due risoluzioni sono stati ispezionati. La valutazione dello stile e dei DPI reali resta aperta.

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
