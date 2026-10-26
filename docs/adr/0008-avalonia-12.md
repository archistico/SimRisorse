# 0008 — Avalonia 12 per il desktop

- Stato: Accettato
- Data: 2026-10-26
- Origine: richiesta esplicita dell'utente di usare Avalonia 12
- Sostituisce: la scelta delle versioni dei pacchetti nell'[ADR 0007](0007-desktop-persistence-and-lifecycle.md); gli altri confini e le regole di ciclo di vita restano validi

## Contesto

La baseline M3 era stata verificata con Avalonia 11.3.18. Il prodotto deve adottare la versione 12. La migrazione riguarda anche l'ambiente dei test grafici e deve conservare pausa, reattività e ripresa dei checkpoint esistenti.

## Decisione

Usare **Avalonia 12.1.4** per tutti i riferimenti Avalonia della soluzione definitiva, con versioni centrali e lock file aggiornati. Il target rimane .NET 10. La prova storica M1 mantiene le sue dipendenze separate.

I soli test Desktop passano a **xUnit 3.2.2**, richiesto dall'integrazione headless di Avalonia 12. Gli altri test mantengono xUnit 2.9.3; entrambi sono eseguiti dal comando di validazione e dal runner Visual Studio già presente.

Il builder headless configura esplicitamente HarfBuzz accanto a Skia e ne dichiara il pacchetto. L'applicazione usa `UsePlatformDetect`, che configura il text shaper. I timer ricevono il dispatcher della finestra. Il rendering di verifica usa le opzioni PNG esplicite della nuova API. Compiled bindings e notifiche di minimizzazione restano applicabili.

Il controllo delle dipendenze ha individuato l'advisory `GHSA-2m69-gcr7-jv3q` nella vecchia libreria SQLite nativa. Aggiornare **Microsoft.Data.Sqlite a 10.0.14**, che risolve SQLitePCLRaw 2.1.13 e include SQLite nativo 3.53.3 su Windows x64. Conservare il formato SQLite e snapshot v1 e le regole economiche esistenti.

## Alternative e conseguenze

- Restare su Avalonia 11 non soddisfa la scelta dell'utente.
- Limitarsi ai riferimenti grafici lascerebbe test headless incompatibili e non verificherebbe il ciclo di vita reale.
- Migrare anche il motore, il formato e le regole durante questo aggiornamento renderebbe meno chiara la verifica di compatibilità. La progressione civile segue con una specifica propria.

Windows è verificato con test headless, render e applicazione nativa. Linux/macOS rimangono da verificare. Un aggiornamento delle dipendenze non certifica automaticamente tutte le piattaforme.

## Verifica

[Report della migrazione](../validation/AVALONIA_12.md): 83 test, build senza avvisi, simulazioni, quattro render e ciclo nativo. Un checkpoint prodotto con la baseline precedente è conservato come fixture e ripreso con il nuovo provider senza cambiarne il contenuto.

## Fonti

- [Guida ufficiale alle modifiche incompatibili di Avalonia 12](https://docs.avaloniaui.net/docs/avalonia12-breaking-changes).
- [Pacchetto Avalonia 12.1.4](https://www.nuget.org/packages/Avalonia/12.1.4).
- [Migrazione ufficiale xUnit da v2 a v3](https://xunit.net/docs/getting-started/v3/migration).
- [Advisory SQLite della vecchia dipendenza](https://github.com/advisories/GHSA-2m69-gcr7-jv3q).
