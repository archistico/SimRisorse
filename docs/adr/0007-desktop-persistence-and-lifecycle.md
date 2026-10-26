# 0007 — Desktop, salvataggi e ciclo di vita della partita

- Stato: Accettato
- Data: 2026-10-26
- Ambito: M3, prima partita desktop collegata al Core
- Origine: ripresa senza produzione offline, interfaccia a elenchi e persistenza locale

La sola scelta delle versioni di pacchetto è superata dall'[ADR 0008](0008-avalonia-12.md), su richiesta dell'utente. La baseline descritta qui resta il riferimento storico delle verifiche M3.

## Contesto

Il Core M2 non dipende da interfaccia o database. M3 deve collegarlo al desktop, mostrare i flussi delle risorse e conservare una sessione lunga senza bloccare la finestra. Un caricamento asincrono non deve sostituire una partita nuova o modificata nel frattempo.

## Decisione

### Confini e versioni

Adottare `SimRisorse.Desktop` per viste, presentazione e orologio, e `SimRisorse.Infrastructure` per l'adattatore SQLite. I ViewModel invocano funzionalità applicative; non chiamano il driver SQL. Il Core resta invariato e privo di dipendenze NuGet esterne.

Usare Avalonia 11.3.18, inclusa la medesima versione headless, e Microsoft.Data.Sqlite 10.0.9. È una baseline riproducibile già verificata e disponibile nella cache completa locale, non una dichiarazione di versione più recente o di audit completato. Il tentativo di audit online incontra ancora un errore TLS. Aggiornamenti di versione e verifica delle dipendenze restano attività esplicite prima della distribuzione pubblica; non si disabilita la validazione TLS per scaricare pacchetti.

### Persistenza

Il contratto `IGameSaveStore` appartiene ad Application. Gli handler catturano snapshot coerenti e svolgono l'I/O fuori dal lock del runtime e dal dispatcher, usando un gate separato per serializzare le operazioni di persistenza. Il provider Microsoft.Data.Sqlite esegue sincronicamente anche i propri metodi ADO.NET asincroni: non si fa affidamento su quei metodi per mantenere la UI reattiva.

Il formato SQLite v1 usa tre tabelle tipizzate per metadati, risorse in millesimi ed edifici. Una transazione sostituisce l'intero checkpoint. Il file porta un application id e una versione; database estranei e versioni non supportate non vengono sovrascritti.

Ogni caricamento registra una revisione della sessione e la mette in pausa. Prima della sostituzione si verificano revisione, identità delle regole e invarianti del Core. Un risultato tardivo viene scartato se sono intervenuti altri comandi o una nuova partita. Un caricamento fallito conserva la sessione corrente.

Un salvataggio cattura lo stato dopo aver acquisito il gate di persistenza: richieste in coda non portano checkpoint catturati in precedenza a sovrascriverne di più recenti. L'annullamento non garantisce di interrompere una scrittura SQL già avviata; una scrittura completata resta valida.

### Interfaccia e tempo

Le viste Edifici e Risorse mostrano quantità, capacità, assegnazioni, costi e motivi di blocco. Produzione, consumo e saldo sono previsioni del prossimo secondo di gioco, ottenute dal medesimo piano del Core. Rimangono visibili in pausa e sono etichettate come previsioni, non come flussi effettivi a gioco fermo.

L'orologio usa timestamp monotoni, sincronizzati prima degli ordini. Pausa, minimizzazione e notifiche di sospensione/ripresa Windows fermano la sessione; il ritorno alla finestra non la riavvia automaticamente. La perdita del focus da sola non ferma il gioco. Un intervallo del timer superiore a due secondi provoca una pausa conservativa senza recupero, anche come fallback dove non siano state ricevute notifiche native.

### Avvio e chiusura

Il desktop tenta di riprendere il singolo slot all'avvio, sempre in pausa e senza misurare il tempo trascorso dal salvataggio. Il file predefinito è sotto `LocalApplicationData/SimRisorse/game.sqlite3`, separato dalla prova M1.

Il gioco salva manualmente, ogni minuto di tempo reale e alla chiusura ordinata. Gli autosalvataggi saltano operazioni già occupate e conferme di nuova partita aperte. Una chiusura con errore di scrittura viene interrotta: il giocatore può riprovare o scegliere esplicitamente di uscire senza salvare.

Un avvio con file incompatibile o non valido non autorizza a sovrascriverlo automaticamente con la sessione iniziale di ripiego. L'autosalvataggio resta sospeso. La nuova partita richiede conferma prima di sostituire la sessione in memoria.

## Alternative e conseguenze

- SQL nei ViewModel: meno confini iniziali, ma accoppiamento tra presentazione e formato dei salvataggi.
- Serializzare `GameView`: perderebbe precisione e responsabilità dello stato; le proiezioni non sono il contratto persistente.
- Applicare qualunque caricamento completato: potrebbe cancellare decisioni prese durante l'attesa.
- Salvare solo alla chiusura: non offre checkpoint periodici per una sessione lunga.
- Recuperare tutto il tempo rilevato dopo la sospensione: produrrebbe avanzamento mentre il giocatore è assente.

La chiusura ordinata e la transazione sono verificate; non garantiscono la conservazione di tutto il progresso dopo arresto forzato o spegnimento improvviso. Il checkpoint periodico riduce l'intervallo non salvato, senza offrire una garanzia temporale se la UI o il disco sono bloccati.

Linux/macOS, file di regole precedenti, migrazioni, salvataggi multipli e aggiornamento automatico delle versioni non sono inclusi nella verifica M3.

## Verifica

Test del Core invariati, funzionalità applicative concorrenti, SQLite reale con errore di scrittura indotto, UI headless e quattro render. Pubblicazione Windows con ciclo nativo di salvataggio, nuova sessione, ricaricamento, notifica di sospensione simulata e chiusura ordinata.

## Collegamenti

- [Formato dei salvataggi](../SAVE_FORMAT.md).
- [Report M3](../validation/M3_DESKTOP.md).
- [Limitazioni asincrone Microsoft.Data.Sqlite](https://learn.microsoft.com/en-us/dotnet/standard/data/sqlite/async).
- [Hook delle notifiche Win32 in Avalonia 11.3.18](https://github.com/AvaloniaUI/Avalonia/blob/11.3.18/src/Avalonia.Controls/Platform/Win32Properties.cs).
