# SimRisorse

![SimRisorse: menu laterale, card delle scorte e dettaglio del Taglialegna](docs/screenshots/colonia.png)

Gioco gestionale in cui sviluppi una colonia investendo materiali e lavoratori nelle filiere produttive. Parti da tronchi, legno lavorato e pietra, fai crescere la popolazione e sblocchi alimentazione e industria.

Il progetto è in sviluppo. La prima campagna prevede **22 tipi di edifici**: per vincere occorrerà possederli tutti, costruendo per ultimo il Castello, sbloccato da una conquista obbligatoria. Militari, navi, conquista e vittoria devono ancora essere implementati.

## Stato attuale

**28 ottobre 2026 — M0–M4 completate, M5.1a Ferro e M5.1b Oro verificate nel progetto locale su Windows.** Lo scenario corrente è `civil-growth-7`, con **17 tipi di edifici e 14 risorse**. Il benchmark **M5.1c** è incluso nel validatore; per la v33 l'utente ha confermato «test passati», senza allegare il riepilogo del gate.

- Crescita civile con Case piccole e grandi, Cibo e Pane, Magazzini e capacità delle scorte.
- Miniere di ferro, carbone e oro, due fonderie e priorità del Ferro quando il Carbone scarseggia.
- Interfaccia retro con font monospace, icone pixel, menu laterale Edifici/Risorse, **tabella delle risorse ordinata alfabeticamente** con stima del tempo di riempimento (`hh:mm:ss`), cifre allineate a destra e quantità arrotondate all'unità. Le card mantengono la precisione precedente.
- Dettaglio con descrizione, costi, produzioni e consumi **in elenchi con icone**. Ordini da **1, 5 o 10** unità; assegnazioni manuali dei lavoratori.
- Pausa, salvataggio manuale e automatico, ripresa senza produzione a gioco chiuso.
- Conversione dei vecchi salvataggi civili con backup dell'originale; formato SQLite v2.

Ultima validazione completa con **rapporto dettagliato** nell'archivio locale: **206/206 test superati**, build senza avvisi/errori, simulazioni, quattro benchmark e prove native di entrambi gli scenari. Verificati i layout **1040×740 e 1240×850**. Il 28/10 l'utente ha confermato che **i test della v33 sono passati** (208 casi xUnit attesi dopo i due nuovi test Desktop); l'output del gate non è stato fornito. **La v36 aggiunge due casi UI sulle scrollbar e sulla selezione ocra alle prove v35: 211 casi attesi, ancora da eseguire su Windows.** [Risultati della verifica precedente](docs/validation/DESKTOP_UI.md) e [protocollo M5.1c](docs/validation/M5_1C_INDUSTRY_SCALE.md).

Il prossimo passo sul gioco, **dopo il gate Windows**, è **implementare il contratto M5.2 A approvato dall'utente il 28/10**. Il [dossier M5.2](docs/design/M5_2_FORGE_MINT_PREFEASIBILITY.md) registra prezzi, ricette, addetti, destinazione delle Spade/Monete, priorità e deposito, più due strategie per il Carbone e un confronto teorico. Una [prova M5.2 separata](experiments/forge-mint-probe/README.md) contiene sei profili A/B **più sei controlli dei limiti di deposito/priorità**, ancora da eseguire con .NET. **Non sono state introdotte le relative regole nel gioco.** Forgia, Zecca, Caserma, Cantiere e Castello restano futuri; rimangono da decidere i quantitativi di arruolamento, i prezzi navali e gli altri contratti militari. Carne fuori dalla prima campagna e demolizione rinviata. L'obiettivo di circa **8 ore** con una strategia efficiente e **32 ore o più** con strategie lente riguarda il gioco completo e non è ancora una durata dimostrata.

## Avvio su Windows

Servono il **.NET SDK 10** indicato in [global.json](global.json) e l'accesso alle sorgenti NuGet per il primo ripristino. Dalla radice del repository:

```powershell
.\validate.cmd
.\run-game.cmd
```

La validazione ripristina i pacchetti con lock, compila, esegue test e simulazioni, pubblica il desktop e ne verifica il ciclo nativo con salvataggi privati. Gli artefatti sono in `.local-cache/m4/`, esclusa da Git. Con una cache completa è disponibile anche il ripristino offline:

```powershell
.\validate.cmd -OfflinePackageSource .local-cache/nuget
```

Il percorso offline non esegue l'audit delle dipendenze. Comandi, benchmark e modalità di verifica sono nella [guida di sviluppo](docs/DEVELOPMENT.md).

## Come si gioca oggi

La partita parte **in pausa** e tenta di riprendere il salvataggio esistente. Dal menu laterale puoi passare tra Edifici e Risorse; le card di abitanti, posti, lavoratori liberi e scorte restano visibili.

Seleziona una struttura per leggere costi, produzione per addetto e secondo, consumi e prerequisiti. Scegli la quantità, costruisci e assegna i lavoratori. Gli ordini sono consentiti anche in pausa; il pulsante **Avvia produzione** fa avanzare la simulazione. L'hover ocra distingue i pulsanti disponibili da quelli disabilitati in grigio.

Le Case piccole offrono **8 posti** e le grandi **32**, con prerequisito Forno. Con spazio e alimentazione disponibili arriva un abitante ogni **60 secondi validi** al ritmo base; le case aggiuntive accelerano gli arrivi fino a **2×**. Tutti consumano Cibo, mentre i residenti oltre la capacità delle Case piccole richiedono anche Pane. La barra Crescita segnala carenze e blocchi. I valori correnti sono regole di sviluppo, non bilanciamento finale.

Il salvataggio avviene ogni minuto e alla chiusura ordinata, oltre al comando manuale. Su Windows la partita civile usa `%LOCALAPPDATA%\SimRisorse\civil-growth.sqlite3`. Il vecchio scenario mantiene il proprio slot ed è disponibile con `run-game.cmd --scenario economy-foundation`. [Formato, conversioni e recupero](docs/SAVE_FORMAT.md).

Per eseguire il motore senza finestre dopo la validazione:

```powershell
dotnet run --project tools/SimRisorse.Simulation -c Release --no-restore -- --scenario civil-growth --strategy gold --seconds 1502
```

I quattro benchmark abitativi e del Pane si possono leggere senza ripetere le simulazioni con `powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\scripts\report-m4.ps1`. Il quinto, industriale, si esegue con `dotnet run --project tools/SimRisorse.Simulation -c Release -- --benchmark-industry-scale`. [Guida ai benchmark M4](docs/validation/M4_BENCHMARK_READER.md); [M5.1c](docs/validation/M5_1C_INDUSTRY_SCALE.md).

## Stack e struttura

**C#/.NET 10, Avalonia 12.1.4/MVVM e SQLite**, con Microsoft.Data.Sqlite 10.0.14. Monolite modulare: il motore resta indipendente dall'interfaccia. Windows è verificato; Linux/macOS e DPI fisici differenti restano da verificare.

| Cartella | Responsabilità |
| --- | --- |
| `src/SimRisorse.Core/` | Stato e regole della simulazione |
| `src/SimRisorse.Application/` | Casi d'uso, scenari e modelli di lettura |
| `src/SimRisorse.Infrastructure/` | Salvataggi SQLite |
| `src/SimRisorse.Desktop/` | Interfaccia Avalonia |
| `tests/` | Test dei quattro livelli |
| `tools/SimRisorse.Simulation/` | Scenari e benchmark senza finestre |
| `experiments/` | Prove separate dalla soluzione principale |
| `docs/` | Design, decisioni, verifiche e screenshot |

## Documentazione

| Documento | Contenuto |
| --- | --- |
| [Indice completo](docs/README.md) | Documentazione corrente, studi e rapporti storici |
| [Game design](docs/GAME_DESIGN.md) | Regole confermate e questioni aperte |
| [Roadmap](docs/ROADMAP.md) | Milestone completate e prossimi incrementi |
| [Handoff](docs/HANDOFF.md) | Punto operativo da cui continuare |
| [Architettura e ADR](docs/ARCHITECTURE.md) | Confini del codice e decisioni tecniche |
| [Sviluppo](docs/DEVELOPMENT.md) | Comandi, convenzioni e verifiche |
| [Salvataggi](docs/SAVE_FORMAT.md) | Formato, backup e compatibilità |
| [Interfaccia retro](docs/PIXEL_ART_UI.md) | Layout, icone, font e stati dei controlli |
| [Ultima verifica dell'interfaccia](docs/validation/DESKTOP_UI.md) | Gate da 206 test, render e limiti |
| [M5.1c — benchmark industriale](docs/validation/M5_1C_INDUSTRY_SCALE.md) | Cinque profili di contesa del Carbone; gate Windows ancora pendente |
| [Changelog](CHANGELOG.md) | Modifiche realizzate |

Lo screenshot iniziale è un render dell'applicazione attuale, conservato nel repository. [Provenienza e aggiornamento](docs/screenshots/README.md).

## Licenza

Il repository contiene la GNU Affero General Public License v3 in [LICENSE.txt](LICENSE.txt).
