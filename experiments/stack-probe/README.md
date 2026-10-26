# Prova tecnica M1

Piccola applicazione eseguibile che verifica insieme C#/.NET, Avalonia e SQLite. È separata dal futuro gioco definitivo e non dipende dalla bozza Windows Forms.

La finestra consente di costruire tre tipi di edificio, assegnare gli otto lavoratori disponibili, avviare e fermare la produzione, salvare e riprendere la partita. Le regole sono nella [scheda dello scenario](../../docs/scenarios/ECONOMY_PROBE.md).

## Prerequisiti e versioni della prova

- SDK .NET `10.0.2xx`; verificato con `10.0.201` su Windows 10 x64, build 19045.
- Avalonia `11.3.18`, inclusa la piattaforma headless della stessa versione.
- Microsoft.Data.Sqlite `10.0.9`.
- Versioni delle dipendenze e lock file sono circoscritti a questa cartella.

Avalonia 11.3.18 è stata scelta per la prova perché tutti i pacchetti, inclusi quelli di test, erano già presenti nella cache locale. **Non è una dichiarazione di versione più recente né la scelta definitiva della versione del prodotto.** Prima dello scaffold M2 verificare le versioni supportate e valutare la linea Avalonia corrente.

## Validazione completa

Dalla radice del repository:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\scripts\validate-stack-probe.ps1
```

L'opzione di execution policy riguarda il solo processo avviato e non modifica la configurazione persistente. In una sessione PowerShell che permette gli script locali è sufficiente eseguire direttamente `scripts/validate-stack-probe.ps1`.

Per usare una cache NuGet già disponibile come sorgente offline, senza richieste di rete:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\scripts\validate-stack-probe.ps1 -OfflinePackageSource "$env:USERPROFILE\.nuget\packages"
```

Lo script ripristina con lock file, compila in Release, esegue i test, genera due immagini della finestra e pubblica un pacchetto dipendente dal runtime. Su Windows avvia anche il pacchetto con `--smoke-test`, che prova avvio e comandi e si chiude automaticamente. `-SkipNativeSmoke` permette di escludere solo quest'ultimo controllo.

Pacchetti, pubblicazione e risultati sono in `.local-cache/`. La telemetria opzionale di build Avalonia è disabilitata per il processo di validazione. Il percorso offline disabilita l'audit NuGet perché non consulta servizi di rete: non equivale a una verifica di sicurezza delle dipendenze.

## Avvio interattivo

Dopo la validazione, dalla radice:

```powershell
.\scripts\run-stack-probe.cmd
```

In alternativa aprire `.local-cache/stack-probe/publish/SimRisorse.StackProbe.Desktop.exe`. La pubblicazione è framework-dependent e richiede il runtime .NET 10; non è un installer autonomo.

La partita inizia in pausa. Selezionare una struttura, costruire se ci sono materiali, assegnare persone e avviare la produzione. Le scorte della segheria richiedono i tronchi disponibili al passo precedente. La minimizzazione mette la partita in pausa.

Il salvataggio interattivo usa un singolo slot in `%LOCALAPPDATA%/SimRisorse/StackProbe/save.sqlite3`. L'avvio non carica né sovrascrive automaticamente il contenuto. "Nuova partita" azzera la sessione in memoria; il salvataggio esistente cambia solo premendo "Salva". Nessun avanzamento a programma chiuso.

## Progetti

| Progetto | Funzione |
| --- | --- |
| Model | Scenario, assegnazioni, ricette, tempo e snapshot; nessun pacchetto esterno |
| Persistence | Adattatore SQLite con scrittura atomica e validazione del caricamento |
| Desktop | Finestra Avalonia, binding MVVM e adattatore temporale |
| Tests | Verifiche del modello, SQLite reale e UI headless con rendering Skia |

I test usano database temporanei univoci, separati dal salvataggio personale. Il fallimento di scrittura viene provocato con un trigger SQLite, per verificare il rollback di metadati e payload.

## Evidenze e limiti

Il [report M1](../../docs/validation/M1_STACK_PROBE.md) registra i risultati e le parti non verificate. Le immagini locali si rigenerano con lo script; non sono asset del gioco.

Nessuna vittoria, classifica, crescita della popolazione, alimento, demolizione, addestramento o conquista è implementata qui. La simulazione di otto ore nei test verifica stabilità e ripetibilità del piccolo scenario, non il bilanciamento del gioco completo.
