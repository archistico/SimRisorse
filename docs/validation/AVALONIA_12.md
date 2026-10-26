# Migrazione ad Avalonia 12

Data: 2026-10-26. Esito: **migrazione completata e verificata su Windows**. Lo scenario giocabile resta quello M3; non sono cambiate formule, dotazione o formato dei salvataggi.

## Versioni

| Componente | Versione |
| --- | --- |
| SDK / target | 10.0.201 / net10.0 |
| Avalonia e headless | 12.1.4 |
| xUnit Desktop | 3.2.2 |
| xUnit degli altri test | 2.9.3 |
| Microsoft.Data.Sqlite | 10.0.14 |
| SQLitePCLRaw | 2.1.13 |
| SQLite nativo Windows x64 | 3.53.3 |

Versioni e decisione nell'[ADR 0008](../adr/0008-avalonia-12.md). La prova M1 conserva Avalonia 11.3.18 nel proprio perimetro storico.

## Evidenze

- Restore con lock file, build Release senza errori o avvisi.
- **83 test superati**: 36 Core, 18 Application, 13 Infrastructure, 16 Desktop.
- Nuovo test di compatibilità su [checkpoint M3 conservato](../../tests/SimRisorse.Infrastructure.Tests/Fixtures/README.md), prodotto con il provider precedente: stato completo ripristinato in pausa, file originale invariato e nuova copia salvabile.
- Quattro render: viste Edifici e Risorse a 1240×850 e 1040×740. Conservati controlli primari e testi visibili.
- Simulazioni di 60 secondi, 8 e 32 ore con la medesima cronologia di riferimento e scorte entro capacità.
- Pubblicazione framework-dependent Windows e ciclo nativo con salvataggio privato, nuova partita, caricamento, notifica Win32 di sospensione e chiusura ordinata: codice zero.

Gli 83 test includono reattività durante I/O, rollback SQLite, protezione di versioni future, autosalvataggio e mancato recupero del tempo di assenza. Nessuna prova usa il file personale del giocatore.

## Dipendenze e rete

Il client NuGet dell'ambiente incontra ancora un errore TLS. I pacchetti mancanti sono stati scaricati dagli endpoint HTTPS ufficiali di NuGet con Python e verifica standard dei certificati, poi ripristinati da sorgenti locali. Non sono state modificate configurazioni globali o disabilitate verifiche TLS.

È stato eseguito anche un confronto delle **63 coppie pacchetto/versione** dei lock file della soluzione definitiva con i dati ufficiali `VulnerabilityInfo` di NuGet, base aggiornata al 19 ottobre e aggiornamento del 26 ottobre 2026. La vecchia baseline risolveva SQLitePCLRaw.lib.e_sqlite3 2.1.11, segnalata dall'advisory [GHSA-2m69-gcr7-jv3q](https://github.com/advisories/GHSA-2m69-gcr7-jv3q). Dopo l'aggiornamento del provider, nessuna versione risolta rientra negli intervalli segnalati da quei dati.

Questo controllo puntuale usa i lock file e i dati scaricati; il normale audit CLI resta da ripristinare nell'ambiente. Non comprende la bozza, la prova storica, il runtime .NET o vulnerabilità non presenti nel feed. Specifica della sorgente: [Vulnerability Info di NuGet](https://learn.microsoft.com/en-us/nuget/api/vulnerability-info).

## Riproduzione e artefatti

Con la cache completa del workspace:

```powershell
.\validate.cmd -OfflinePackageSource .local-cache/nuget
.\run-game.cmd
```

Su un ambiente con accesso NuGet funzionante si può usare `validate.cmd` senza opzione offline. Per il normale audit: `dotnet list SimRisorse.slnx package --no-restore --vulnerable --include-transitive`.

Il validatore continua a scrivere in `.local-cache/m3/`: riepilogo con versioni risolte, TRX isolati per esecuzione, quattro PNG, pubblicazione, simulazioni e database privati. Feed e confronto puntuale delle vulnerabilità sono in `.local-cache/avalonia12-audit/`; i pacchetti scaricati in `.local-cache/avalonia12-feed/`. Sono artefatti locali ignorati da Git.

Linux/macOS, distribuzione self-contained e sospensione fisica del computer restano non verificati. Il test nativo invia una notifica alla sola finestra dell'applicazione.
