# M2 — Verifica del motore economico

Data: 2026-10-26. Esito: **M2 completata**, con Core, casi d'uso verticali e strumento di simulazione indipendenti da UI, SQLite, prototipo legacy ed esperimento M1.

## Struttura e ambiente

La [soluzione definitiva](../../SimRisorse.slnx) contiene cinque progetti: Core, Application, due progetti di test e Simulation. Core e Application non hanno dipendenze NuGet esterne; Simulation dipende da Application e dal framework.

Target .NET 10 LTS, compilato con SDK locale 10.0.201 su Windows 10 x64. `global.json` consente feature band successive di .NET 10 senza prerelease. Non è stato installato o aggiornato un SDK globale durante il lavoro. La scelta del canale LTS è riferita alla [politica ufficiale .NET](https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-core).

I soli progetti di test usano Microsoft.NET.Test.Sdk 17.14.1, xUnit 2.9.3 e runner Visual Studio 3.1.5, già verificati con questo SDK. Versioni e dipendenze transitive sono fissate da configurazione centrale e lock file. Non sono dichiarate le versioni più recenti disponibili; la scelta di Avalonia e SQLite per M3 non introduce pacchetti UI nel Core.

Verificato tramite valutazione MSBuild che la bozza conserva `net6.0-windows`, gestione pacchetti non centralizzata e assenza di warnings-as-errors. L'esperimento M1 mantiene le proprie configurazioni locali e Avalonia 11.3.18.

## Risultati finali

| Controllo | Esito |
| --- | --- |
| Build Release | Zero errori e avvisi |
| Test Core | 36 superati |
| Test Application e architettura | 9 superati |
| Totale M2 | **45 superati, 0 falliti** |
| Simulazione di 60 secondi | 2 tronchi, 179 legno, 140 pietra |
| Simulazioni di 8 e 32 ore | Tempo corretto e quantità entro la capacità |
| Dipendenze degli assembly | Core solo framework; Application solo Core e framework |

Non sono stati rieseguiti i 23 test della prova M1, il cui codice non è stato modificato. Le valutazioni delle configurazioni verificano che i nuovi file alla radice non ne cambino le dipendenze.

## Comportamenti verificati

- Costi multipli atomici, acquisti sconosciuti o non validi, prerequisiti e overflow senza addebiti parziali.
- Assegnazioni intere, più posti per edificio, rilascio del personale e distinzione tra capienza e persone disponibili.
- Risorse in millesimi, rifiuto dell'arrotondamento implicito, lavorazioni complete e arresto a deposito pieno.
- Contesa di input e spazio tra più attività; priorità indipendente dall'ordine di caricamento e spareggio ordinale.
- Output e spazio liberato disponibili solo al passo successivo; previsioni calcolate con le stesse prenotazioni.
- Pausa, ordini durante la pausa, residuo inferiore al secondo e ripresa in pausa.
- Avanzamenti in blocco e a frazioni di secondo equivalenti su otto ore.
- Rifiuto di snapshot incompatibili, scorte oltre il limite, assegnazioni non valide e tipi duplicati.
- Sequenza riproducibile di 500 ordini con verifica dei limiti e immutabilità degli snapshot precedenti.
- Cento richieste concorrenti di costruzione o assegnazione senza doppie spese o duplicazione di persone.

## Audit delle dipendenze

È stato tentato il comando di audit online con dipendenze transitive e `--no-restore`. L'accesso a NuGet non è riuscito per un errore TLS dell'ambiente: "Nessuna credenziale disponibile nel pacchetto di sicurezza". **L'audit online non è completato**; non è stato disabilitato il controllo dei certificati.

Il ripristino e la validazione funzionale usano la sorgente offline e disabilitano l'audit solo per quell'esecuzione. I progetti di produzione M2 non includono pacchetti esterni; questo non costituisce una certificazione del runtime o dei pacchetti di test. L'audit va ripetuto da un ambiente con accesso NuGet funzionante.

## Riproduzione

Dalla radice, con pacchetti già disponibili nella cache indicata:

```powershell
.\validate.cmd -OfflinePackageSource "$env:USERPROFILE\.nuget\packages"
```

Con sorgenti online accessibili è sufficiente `validate.cmd`. Lo script ripristina con lock file e cache nel workspace, compila, esegue i test e controlla le tre durate di simulazione. Artefatti sotto `.local-cache/m2/`: file TRX e JSON delle simulazioni.

Per riprovare il solo audit dopo un ripristino valido:

```powershell
dotnet list SimRisorse.slnx package --no-restore --vulnerable --include-transitive --source https://api.nuget.org/v3/index.json
```

## Limiti e seguito

Nessuna nuova UI o persistenza definitiva ancora collegata al Core. Gli snapshot sono contratti in memoria, non file di salvataggio compatibili automaticamente con quelli M1. Popolazione, capacità e ricette a un secondo sono limitazioni dello scenario attuale; crescita, nuovi depositi e cicli differenti richiedono estensioni esplicite.

Le simulazioni lunghe verificano stabilità e regole, non la durata ottimale del gioco completo. Il catalogo valida i prerequisiti, ma non dimostra da solo che tutte le filiere economiche siano avviabili. Linux/macOS, pacchetti desktop e audit online restano non verificati in M2.

Il prossimo passo è M3: contratti di salvataggio applicativi, adattatore SQLite, desktop Avalonia collegato alle funzionalità definitive, gestione del tempo reale e prove di chiusura/ripresa.
