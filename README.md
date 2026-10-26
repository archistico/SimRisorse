# SimRisorse

Gioco gestionale in cui si parte da materiali di base e si sviluppano filiere produttive, popolazione e capacità militare per arrivare a costruire tutti i tipi di edificio previsti dallo scenario.

La scelta centrale è come investire risorse e lavoratori: espandere la produzione adesso oppure risparmiare per il prossimo sblocco. Soldati, castelli e missioni di conquista delle isole fanno parte della visione di gioco.

## Stato del progetto

**26 ottobre 2026: M0–M3 completate e desktop aggiornato ad Avalonia 12.** La [soluzione definitiva](SimRisorse.slnx) contiene motore, casi d'uso, interfaccia Avalonia, salvataggi SQLite e simulatore, verificati con **83 test**. Puoi giocare al primo ciclo economico con tre strutture e otto abitanti. Per M4 è preparata una [proposta di crescita civile](docs/scenarios/CIVIL_GROWTH_DRAFT.md); popolazione dinamica, catalogo completo, esercito e isole non sono ancora implementati. La bozza Windows Forms e la prova M1 restano separate come riferimenti.

L'impostazione tecnica adottata è un monolite modulare con funzionalità verticali e un motore indipendente dall'interfaccia. **C#/.NET 10, Avalonia 12.1.4/MVVM e SQLite sono lo stack desktop**, con Microsoft.Data.Sqlite 10.0.14. Core e Application usano solo il framework e i riferimenti interni previsti. Windows è verificato, Linux/macOS non ancora. Compatibilità dei salvataggi e controllo puntuale delle dipendenze sono descritti nel [report della migrazione](docs/validation/AVALONIA_12.md); l'audit CLI dell'ambiente incontra ancora un errore TLS.

## Scelte di gioco confermate

- Edifici gestiti tramite elenco e schede, senza collocamento su una mappa.
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
| [Roadmap](docs/ROADMAP.md) | Milestone, dipendenze e criteri di completamento |
| [Sviluppo](docs/DEVELOPMENT.md) | Convenzioni, verifiche e gestione dei cambiamenti |
| [Handoff](docs/HANDOFF.md) | Stato operativo e prossimo lavoro da riprendere |
| [Changelog](CHANGELOG.md) | Modifiche effettivamente realizzate |
| [Riferimenti visivi](docs/references/README.md) | Screenshot originali e analisi delle evidenze |
| [Scenario della prova](docs/scenarios/ECONOMY_PROBE.md) | Valori e regole provvisorie del primo ciclo economico |
| [Verifica dello stack](docs/validation/M1_STACK_PROBE.md) | Ambiente, 23 test, pubblicazione e limiti della prova |
| [Scenario del Core](docs/scenarios/ECONOMY_FOUNDATION.md) | Catalogo, dotazione e regole attualmente eseguibili |
| [Verifica del motore](docs/validation/M2_CORE.md) | 45 test, simulazioni lunghe e limiti dell'audit |
| [Salvataggi](docs/SAVE_FORMAT.md) | Percorso, formato, checkpoint e comportamento in caso di errore |
| [Verifica del desktop](docs/validation/M3_DESKTOP.md) | 82 test, rendering e avvio nativo Windows |
| [Migrazione Avalonia 12](docs/validation/AVALONIA_12.md) | Baseline corrente, 83 test, compatibilità dei checkpoint e dipendenze |
| [Proposta M4](docs/scenarios/CIVIL_GROWTH_DRAFT.md) | Case, crescita e prima filiera alimentare; bozza non implementata |

## Struttura attuale

```text
README.md
CHANGELOG.md
LICENSE.txt
docs/
  adr/
  references/screenshots/
legacy/
  winforms/
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

Nessun progetto della soluzione definitiva dipende da `legacy/` o `experiments/`.

Le cache locali sono state raccolte in `.local-cache/`, esclusa da Git. Le immagini di riferimento sono state trasferite dalla precedente cartella `screnshot/`, mantenendo nomi e contenuto.

## Esecuzione

Per validare la soluzione definitiva con sorgenti NuGet accessibili:

```powershell
.\validate.cmd
```

Con la cache completa del workspace si può usare `validate.cmd -OfflinePackageSource .local-cache/nuget`; è possibile indicare un'altra cache purché contenga le versioni correnti. Lo script compila, esegue i test, controlla simulazioni di 60 secondi, 8 e 32 ore e pubblica/avvia il desktop su Windows con un salvataggio privato di verifica. Il percorso offline non esegue l'audit delle dipendenze.

Per aprire il gioco dopo la validazione:

```powershell
.\run-game.cmd
```

Le viste Edifici e Risorse mostrano quantità, capacità, costi, personale e flussi previsti al secondo. Puoi impartire ordini da 1, 5 o 10 unità. Il gioco parte in pausa e tenta di riprendere il salvataggio esistente.

Salvataggio ogni minuto e alla chiusura ordinata, oltre al comando manuale. Il file è sotto `%LOCALAPPDATA%\SimRisorse\game.sqlite3` su Windows. La ripresa non produce risorse per il tempo trascorso a gioco chiuso. Dettagli e gestione degli errori nel [formato dei salvataggi](docs/SAVE_FORMAT.md).

Dopo la validazione, per eseguire il nuovo motore senza finestre:

```powershell
dotnet run --project tools/SimRisorse.Simulation -c Release --no-restore -- --seconds 60
```

L'esperimento iniziale si avvia ancora con `scripts/run-stack-probe.cmd`, dopo la sua validazione separata: vedere il [README M1](experiments/stack-probe/README.md). La [bozza Windows Forms](legacy/winforms/README.md) non è la nuova applicazione.

## Licenza

Il repository contiene il testo della GNU Affero General Public License v3 in [LICENSE.txt](LICENSE.txt), conservato senza modifiche.
