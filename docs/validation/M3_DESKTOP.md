# M3 — Desktop e persistenza collegati al Core

Data: 2026-10-26. Esito: **M3 completata su Windows**, con la prima partita desktop collegata al motore definitivo. Il contenuto resta lo scenario economico di tre strutture e otto abitanti.

Successivamente il prodotto è stato aggiornato ad Avalonia 12: vedere il [report della migrazione](AVALONIA_12.md) per le versioni e le verifiche correnti. Questo report conserva la baseline iniziale M3.

## Ambiente e dipendenze

SDK 10.0.201 su Windows 10 x64, runtime .NET 10.0.5. Avalonia 11.3.18 e relativo pacchetto headless, Microsoft.Data.Sqlite 10.0.9. Le versioni sono fissate e ripristinabili dalla cache locale; non sono dichiarate le più recenti o già sottoposte ad audit online.

La soluzione contiene nove progetti. Core rimane dipendente solo dal framework; Application dipende solo da Core e framework. Infrastructure implementa il port di salvataggio; Desktop compone gli adattatori e usa funzionalità e proiezioni applicative. Non esistono riferimenti a `legacy/` o `experiments/`.

## Risultati finali

| Verifica | Esito |
| --- | --- |
| Build Release | Zero errori e avvisi |
| Core | 36 test superati |
| Application e architettura | 18 test superati |
| Infrastructure | 12 test superati |
| Desktop e orologio | 16 test superati |
| Totale | **82 superati, 0 falliti** |
| Simulazioni da terminale | 60 secondi, 8 e 32 ore, esiti attesi |
| Render della UI | Edifici e Risorse a 1240×850 e 1040×740 |
| Pubblicazione Windows | Framework-dependent, avvio nativo riuscito |
| Comando `run-game.cmd` | Verificato nella modalità di prova nativa |

## Flussi verificati

### Applicazione e persistenza

- Checkpoint con tempo frazionario, risorse e assegnazioni coerenti; ricaricamento sempre in pausa.
- Snapshot con formato, regole o assegnazioni non validi rifiutati senza sostituire la sessione corrente.
- Caricamento rallentato che non può sostituire una nuova partita.
- I/O trattenuto artificialmente durante il quale comandi, tempo e letture rimangono utilizzabili.
- Salvataggi serializzati: quello successivo cattura lo stato corrente dopo l'attesa.
- SQLite reale con quantità in millesimi e transazione completa su metadati, risorse ed edifici.
- Errore indotto sull'ultima scrittura: checkpoint precedente interamente conservato.
- Database estranei, file futuri e snapshot di versione futura non sovrascritti.
- File mancanti non creati dal caricamento; valori negativi e dati incompleti rifiutati.

### Interfaccia e ciclo di vita

- Ordini multipli tramite binding e input mouse simulato; limiti di personale e risorse rispettati.
- Viste di quantità/capacità e produzione, consumo e saldo previsti con unità `/s`.
- Nuova partita con conferma e annullamento senza perdita della sessione.
- Chiusura ordinata che salva; nuova finestra che riprende il checkpoint in pausa senza avanzamento offline.
- Salvataggio fallito in chiusura: finestra conservata, stato disponibile e successivo tentativo riuscito.
- Avvio incompatibile che non sovrascrive il file con autosalvataggi o chiusura.
- Checkpoint periodico che non interrompe il tempo e non salva mentre è aperta la conferma di nuova partita.
- Orologio a 32 ore e quantità con tre decimali; pausa alla minimizzazione e dopo un intervallo di sospensione.
- Controlli principali entro l'area visibile alle due dimensioni, con ispezione delle quattro immagini prodotte.

### Verifica nativa Windows

Il pacchetto pubblicato è stato avviato con un file SQLite privato della prova. Ha eseguito costruzione, assegnazione, avanzamento, salvataggio, nuova sessione e caricamento. Una notifica `WM_POWERBROADCAST` di sospensione è stata inviata esclusivamente alla sua finestra; il gioco è andato in pausa. La chiusura ordinata ha completato il salvataggio e il processo è terminato con codice zero.

Non è stato sospeso o spento realmente il computer. La prova valida il ricevitore della notifica e il percorso applicativo, non ogni condizione hardware o sistema operativo.

## Riproduzione

Con cache completa dalla radice:

```powershell
.\validate.cmd -OfflinePackageSource "$env:USERPROFILE\.nuget\packages"
.\run-game.cmd
```

Con sorgenti NuGet accessibili si può omettere `-OfflinePackageSource`. `-SkipNativeSmoke` esclude soltanto il lancio nativo automatico. L'avvio interattivo usa il file sotto LocalApplicationData; la validazione usa un file di prova separato.

Artefatti ignorati da Git in `.local-cache/m3/`:

- `validation.json`, riepilogo dell'ultima esecuzione;
- `runs/<id>/test-results/`, file TRX isolati per esecuzione;
- `runs/<id>/native-save.sqlite3` e log dell'avvio nativo;
- `buildings.png`, `buildings-compact.png`, `resources.png`, `resources-compact.png`;
- `publish/`, pacchetto dipendente dal runtime;
- JSON delle simulazioni di riferimento.

Il controllo del processo conserva il relativo handle prima dell'attesa, per ottenere il codice di uscita anche in Windows PowerShell. Nessuna esecuzione con codice assente viene trattata come riuscita.

## Audit e limiti

Audit NuGet con dipendenze transitive e `--no-restore` nuovamente tentato: non completato per errore TLS dell'ambiente, "Nessuna credenziale disponibile nel pacchetto di sicurezza". Non sono stati aggirati i certificati. La validazione offline disabilita l'audit solo per quell'esecuzione.

Non verificati: Linux/macOS, pacchetto self-contained, spegnimento improvviso, migrazioni e versioni di regole precedenti, classifica pubblica e bilanciamento completo. Il fallback temporale provoca pausa anche dopo un ritardo del dispatcher superiore a due secondi; non è un sistema completo di rilevazione dell'attività del giocatore.

La partita attuale ha popolazione e capacità fisse. Case, cibo, crescita, demolizione, militari, isole e monumenti restano nelle milestone successive.

## Riferimenti

- [ADR 0007](../adr/0007-desktop-persistence-and-lifecycle.md).
- [Formato dei salvataggi](../SAVE_FORMAT.md).
- [Filiere negli screenshot](../references/RESOURCE_AND_PRODUCTION_CHAINS.md).
