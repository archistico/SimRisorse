# SimRisorse

![SimRisorse: menu laterale, card delle scorte e dettaglio del Taglialegna](docs/screenshots/colonia.png)

Gioco gestionale in cui sviluppi una colonia investendo materiali e lavoratori nelle filiere produttive. Parti da tronchi, legno lavorato e pietra, fai crescere la popolazione e sblocchi alimentazione e industria.

Il progetto è in sviluppo. La prima campagna prevede **22 tipi di edifici**: per vincere occorrerà possederli tutti, costruendo per ultimo il Castello, sbloccato da una conquista obbligatoria. Navi, missioni, conquista e vittoria devono ancora essere implementati.

## Stato attuale

**3 novembre 2026 — M5.3a/b implementata, da verificare su Windows.** Regole `civil-growth-10`, **20 tipi di edifici e 16 risorse**: la **Caserma** (260 Legno lavorato/200 Pietra, dopo la Forgia) trasforma abitanti liberi in soldati al costo di **5 Spade** ciascuno; ogni Caserma addestra una recluta alla volta in **30 secondi**. Reclute e soldati restano residenti; l'arruolamento si può annullare. Il codice è stato scritto senza SDK .NET disponibile: **308 test attesi**, [rapporto](docs/validation/M5_3A_BARRACKS.md). Restano Cantiere e Castello.

- Crescita civile con Case piccole e grandi, Cibo e Pane, Magazzini e capacità delle scorte.
- Miniere di ferro, carbone e oro, due fonderie e priorità del Ferro quando il Carbone scarseggia.
- Interfaccia retro con font monospace, icone pixel, menu laterale Edifici/Risorse, **tabella delle risorse ordinata alfabeticamente** con stima del tempo di riempimento (`hh:mm:ss`), cifre allineate a destra e quantità arrotondate all'unità. Le card mantengono la precisione precedente.
- Dettaglio con descrizione, costi, produzioni e consumi **in elenchi con icone**. Ordini da **1, 5 o 10** unità; assegnazioni manuali dei lavoratori.
- Pausa, salvataggio manuale e automatico, ripresa senza produzione a gioco chiuso.
- Conversione dei vecchi salvataggi civili con backup dell'originale; formato SQLite v2.

Ultima validazione completa Windows (v40, prima della Caserma): **260/260 test**, build senza avvisi/errori, **23 cronologie più crescita accelerata**, **sei benchmark**, pubblicazione e smoke nativi dei due scenari. Generati **33 render**. M5.2c confronta nove profili e quattro guardie con replay e contabilità per secondo; [risultati e limiti](docs/validation/M5_2C_FORGE_MINT.md).

La **Forgia** costa 240 Legno lavorato/180 Pietra e trasforma **1 Ferro +1 Carbone → 2 Spade/s**; la **Zecca** costa 210/160 e trasforma **1 Oro +1 Carbone → 3 Monete/s**, con un addetto ciascuna. Le fonderie hanno priorità sul Carbone, poi Forgia e Zecca. Spade per il futuro arruolamento e Monete per pagamenti espliciti; nessuna valuta generale. **M5.2c è verificata:** obiettivo 240 Spade/360 Monete in **241 s a fasi /122 s in parallelo** nel confronto sintetico; **2.320 s dalla nuova partita** con la strategia integrata `crafting`. La [prova isolata storica](experiments/forge-mint-probe/README.md) resta distinta dalle nuove misure sul catalogo corrente. Le Spade si consumano ora nell'arruolamento; i prezzi navali saranno scelti in M5.4; le circa **8 ore efficienti / 32+ ore lente** restano obiettivi della campagna completa.

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

Selezionando la **Caserma** compaiono la sezione Reclutamento e i pulsanti **Arruola** e **Annulla** al posto dei lavoratori. Arruolare impiega solo abitanti liberi e spende subito 5 Spade a persona; le reclute attendono un posto e diventano soldati dopo 30 secondi validi. Reclute e soldati non lavorano ma continuano a occupare le case e a mangiare. Annullando si recuperano persone e Spade, prima dalla coda e poi da chi ha meno progresso.

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
| [M5.1c — benchmark industriale](docs/validation/M5_1C_INDUSTRY_SCALE.md) | Cinque profili di contesa del Carbone verificati |
| [M5.3a/b — Caserma](docs/validation/M5_3A_BARRACKS.md) | Contratto militare, 308 test attesi, **da verificare su Windows** |
| [M5.2c — Filiere integrate](docs/validation/M5_2C_FORGE_MINT.md) | Ultimo gate verificato da 260 test, confronto strategie e percorso dalla nuova partita |
| [M5.2b — Zecca](docs/validation/M5_2B_MINT.md) | Gate storico da 258 test, Monete e conversione v8→v9 |
| [M5.2a — Forgia](docs/validation/M5_2A_FORGE.md) | Gate storico da 235 test, Spade e conversione v7→v8 |
| [Changelog](CHANGELOG.md) | Modifiche realizzate |

Lo screenshot iniziale è un render dell'applicazione attuale, conservato nel repository. [Provenienza e aggiornamento](docs/screenshots/README.md).

## Licenza

Il repository contiene la GNU Affero General Public License v3 in [LICENSE.txt](LICENSE.txt).
