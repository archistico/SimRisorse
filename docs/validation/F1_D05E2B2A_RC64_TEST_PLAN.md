# RC64 — Piano di validazione F1-D05e.2b.2a

**Baseline:** RC63 validata dall'utente con **771/771 test**. **RC64:** 6 test aggiuntivi (5 Application, 1 Infrastructure), **777 attesi**. SDK .NET 10 non disponibile nell'ambiente di produzione del pacchetto.

## Automatismi

| Verifica | Atteso |
| --- | --- |
| `./validate.cmd` su Windows | Compilazione completa e 777/777 test |
| Nuovo checkpoint | 15 famiglie; assenza `food-store` in memoria |
| Proiezione v14 temporanea | 16 famiglie con vecchio produttore inerte; nessuna mutazione dello snapshot v15 |
| Comandi costruzione v15 e capacità | Nessun rientro della sedicesima famiglia dopo i commit; la proiezione delle capacità non ricerca più la famiglia ritirata |
| Tick unificato | Nessun produttore storico nel checkpoint risultante; produzione Cibo cittadino zero |
| Ritiro su stato nuovo | Idempotente, senza introdurre lo slot |
| SQLite gen. 6 | Salva/ricarica 15 famiglie senza duplicazioni e conserva le scorte |
| Test storici RC61–RC63 | Conservare rifiuto di produttore ereditato occupato, grafica e filiere |

## Test manuali consigliati

1. ` .\validate.cmd `, poi ` .\run-game.cmd `. Confermare tutte le sezioni della GUI v15 senza perdere le scelte della v14; confrontare con ` .\run-game.cmd --v14 `.
2. Nuova partita v15: filtro filiere, Macellaio e altre cinque strutture; edifici bloccati in grigio e diagnosi, dettagli con icone, pulsanti +1/+5/+10, assegnazione lavoratori, barra crescita nel menu.
3. Costruire una struttura e salvare/riprendere. Nessun cambio involontario di lavoratori, inventario o selezione.
4. Facoltativo: `dotnet run --project tools/SimRisorse.Simulation -c Release --no-restore -- --audit-v15 --seconds 120 --profile combined` e poi `--hours 24`. Atteso `ImbalancedSeconds = 0`; non inferire esito prima di eseguirli.

## In caso di errore

Allegare il primo errore C# o il primo test fallito con stack trace. Distinguere errori di compilazione da errori xUnit. Non correggere la GUI modificando stili approvati.
