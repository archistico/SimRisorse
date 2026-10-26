# 0005 — C#/.NET, Avalonia e SQLite per il primo desktop

- Stato: Accettato
- Data: 2026-10-26
- Ambito: runtime, interfaccia desktop e persistenza locale
- Origine: proposta indicata dall'utente e adottata tecnicamente dopo la richiesta di proseguire e la prova M1

## Contesto

L'interfaccia prevista è composta da elenchi, schede e indicatori. Il gioco è locale e sospendibile. L'utente sta valutando C#, SQLite e Avalonia per il supporto a più sistemi operativi. La bozza attuale usa Windows Forms su `net6.0-windows`.

## Decisione

- C# su .NET 10 LTS per la nuova soluzione.
- Avalonia con MVVM per il desktop.
- SQLite per i salvataggi, dietro i contratti applicativi.
- Usare `Microsoft.Data.Sqlite` per il primo adattatore; EF Core non è necessario per il modello verificato e non viene introdotto ora.

Prima piattaforma verificata: Windows 10 x64, build 19045. Linux e macOS restano obiettivi possibili, da provare prima di dichiararne il supporto. La portabilità del framework non equivale a verifica dell'applicazione su tutti i sistemi operativi.

La prova usa Avalonia 11.3.18 e Microsoft.Data.Sqlite 10.0.9 con SDK 10.0.201, disponibili localmente. Queste versioni fissano la riproducibilità dell'esperimento; quelle del prodotto vanno selezionate all'inizio di M2 verificando supporto e compatibilità correnti.

## Alternative

- Windows Forms: continuità con la bozza, ma destinazione Windows.
- Applicazione web: accesso dal browser, con interfaccia e persistenza differenti; non scelta dall'utente.
- Salvataggio su file strutturato: possibile per uno stato piccolo, da confrontare con esigenze di atomicità, storico e migrazioni.
- EF Core su SQLite: mapping e migrazioni più strutturati, con un costo aggiuntivo da giustificare rispetto al modello reale.

## Conseguenze

La scelta riusa C# per motore, desktop e simulazioni. La prova concreta di UI, salvataggio e pubblicazione è completata su Windows. Il motore deve comunque restare utilizzabile con un'interfaccia o un adattatore diverso.

## Verifica e accettazione

Il 2026-10-26 la prova separata ha verificato elenco e dettaglio, binding MVVM, ordini, pausa, I/O fuori dal dispatcher, salvataggio e ricaricamento, versioni e rollback di una transazione fallita. Build Release: zero errori e avvisi; 23 test superati; pubblicazione framework-dependent avviata su Windows con esito positivo.

La scelta dello stack è accettata per iniziare M2. L'esperimento non vale come implementazione del Core definitivo. Audit online delle dipendenze, altri sistemi operativi, distribuzione self-contained e gestione completa della sospensione restano non verificati.

## Collegamenti e fonti

Riferimenti consultati durante la discussione del 2026-10-26; ricontrollare compatibilità e versioni al momento dell'implementazione.

- [Architettura](../ARCHITECTURE.md).
- [Evidenze M1](../validation/M1_STACK_PROBE.md).
- [Prova eseguibile](../../experiments/stack-probe/README.md).
- [Politica di supporto .NET](https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-core).
- [Piattaforme supportate da Avalonia](https://docs.avaloniaui.net/docs/supported-platforms).
- [SQLite come formato di applicazione](https://www.sqlite.org/appfileformat.html).
- [Microsoft.Data.Sqlite](https://learn.microsoft.com/en-us/dotnet/standard/data/sqlite/).
