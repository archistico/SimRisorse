# M4 — Primo incremento di crescita civile

Data: 2026-10-26. Esito: **case, crescita e prima filiera alimentare implementate e verificate su Windows**. M4 rimane in corso per le ulteriori fasi civili e la capacità di magazzino espandibile.

Baseline invariata: .NET 10, Avalonia 12.1.4, Microsoft.Data.Sqlite 10.0.14; xUnit 3 per Desktop e 2 per gli altri test. Core e Application mantengono dipendenze solo dal framework e dai riferimenti interni. Nessun nuovo pacchetto.

## Risultati

Build Release senza errori o avvisi, restore con lock file, **117 test superati**:

| Progetto | Test |
| --- | --- |
| Core | 49 |
| Application | 24 |
| Infrastructure | 21 |
| Desktop | 23 |

I nuovi controlli coprono:

- Arrivo soltanto al sessantesimo secondo valido, consumo dal passo successivo e persone intere.
- Progresso a 59,5 secondi salvato, pausa senza avanzamento e arrivo dopo altri 0,5 secondi.
- Case piene senza accumulo di arrivi; più case non moltiplicano il ritmo.
- Cibo prenotato prima delle ricette; nessuna razione parziale e nessun riuso immediato dello spazio liberato.
- Carenza che conserva persone e progresso; cibo nuovo utilizzabile dal passo successivo.
- Snapshot invalidi, capacità abitativa fuori limite e acquisti falliti senza mutazioni parziali.
- Proiezioni confrontate con le variazioni effettive di tutte le risorse per 181 secondi, inclusi due arrivi.
- Ripresa e rollback su SQLite reale v2: popolazione, crescita, tempo frazionario, scorte ed edifici rimangono coerenti.
- Stato di crescita mancante, negativo o oltre i limiti tecnici rifiutato; errore applicativo senza sostituire la partita corrente.
- File v1 e v2 che non si sovrascrivono, in entrambe le direzioni; fixture M3 ancora caricabile senza modificarla.
- UI con case, posti, cibo, crescita e carenze; nuova partita nello scenario corrente e percorsi personali separati.

## Simulazioni e desktop

| Cronologia | Durata | Esito |
| --- | --- | --- |
| Foundation, nessun ordine | 60 s, 8 h, 32 h | Timeline precedente conservata e risorse entro capacità |
| Civil, bootstrap | 121 s | 9 abitanti, progresso 0, 140,32 cibo |
| Civil, recupero | 662 s | 14 abitanti, progresso 0, produzione alimentare recuperata |
| Civil, bootstrap | 8 h e 32 h | 16 abitanti, nessuna crescita oltre i posti e scorte nei limiti |

Otto render: viste Edifici/Risorse per i due scenari, a 1240×850 e 1040×740. Ispezionate le nuove viste; controllati i comandi primari e il contenimento della barra di crescita nella colonna laterale. Le schede mostrano tre risorse in evidenza; la tabella Risorse conserva l'elenco completo. I saldi negativi hanno segno esplicito e colore distinto.

Pubblicazione Windows e ciclo nativo completati per entrambi gli scenari. Per Civil: costruzione della casa, progresso 59 s, salvataggio, nuova partita, ricaricamento e singolo arrivo dopo 1 s. Per entrambi: notifica Win32 di sospensione alla sola finestra e chiusura ordinata con salvataggio. File privati di verifica, nessun utilizzo dei salvataggi personali.

## Riproduzione

```powershell
.\validate.cmd -OfflinePackageSource .local-cache/nuget
.\run-game.cmd
.\run-game.cmd --scenario economy-foundation
```

Il validatore scrive in `.local-cache/m4/`: `validation.json`, otto PNG, sette JSON di simulazione, pacchetto e risultati/log/database isolati per esecuzione sotto `runs/<id>/`. La cache NuGet rimane quella del workspace. I report precedenti restano nelle cartelle M1–M3.

## Limiti

La filiera dimostra raggiungibilità e recupero per le due cronologie, non ottimalità matematica o bilanciamento sulle otto ore. Non sono implementati magazzini espandibili, pane/carne, demolizione, esercito, isole o classifica.

Nessuna conversione automatica di regole o formati; scelta dello scenario tramite argomento di avvio, non ancora tramite schermata dedicata. Linux/macOS, sospensione fisica del computer, spegnimento improvviso e distribuzione self-contained restano da verificare. Audit CLI non completato per TLS nell'ambiente; nessuna dipendenza è cambiata rispetto al [controllo puntuale della migrazione Avalonia](AVALONIA_12.md).
