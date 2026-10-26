# Scenario economico di riferimento M2

Stato: implementato nel primo Core, con valori iniziali provvisori. Non è lo scenario completo da otto ore e non possiede ancora un obiettivo di vittoria.

## Identità

- Scenario: `economy-foundation`.
- Regole: `economy-foundation-1`.
- Contratto snapshot in memoria: `1`; nessun formato di salvataggio su disco ancora adottato.
- Definizione: [EconomyFoundationScenario.cs](../../src/SimRisorse.Application/Scenarios/EconomyFoundationScenario.cs).

La dotazione e la filiera riprendono la [prova M1](ECONOMY_PROBE.md), ma non importano i suoi assembly o i suoi salvataggi SQLite.

## Dotazione e catalogo

Otto persone, di cui tre assegnate, senza crescita o consumo alimentare. Scorte iniziali: 0 tronchi, 120 legno lavorato e 80 pietra. Capacità fissa: 1.000 per ciascuna risorsa. Un edificio di ogni tipo già presente e dotato di un lavoratore.

| Identificatore | Costo | Posti per edificio | Ricetta per lavoratore/secondo | Priorità | Prerequisito per nuovi acquisti |
| --- | --- | --- | --- | --- | --- |
| `tree-feller` | 20 legno, 10 pietra | 1 | +2 tronchi | 10 | Nessuno |
| `sawmill` | 40 legno, 20 pietra | 1 | −2 tronchi, +1 legno | 20 | Almeno un taglialegna |
| `stonecutter` | 30 legno, 10 pietra | 1 | +1 pietra | 30 | Nessuno |

Gli acquisti multipli pagano tutto il costo o non modificano nulla. I nuovi posti restano vuoti finché il giocatore assegna persone. Assegnazione e rimozione possono riguardare più lavoratori; anche queste operazioni sono atomiche.

## Regole del passo

Le risorse supportano tre decimali esatti, le persone sono intere. La produzione esegue ricette complete a passi di un secondo, usando input e spazio presenti all'inizio del passo. Gli output non alimentano altre attività nello stesso passo.

La contesa segue priorità crescente e, a parità, identificatore ordinale. Anche gli edifici che producono la stessa risorsa devono prenotare lo spazio disponibile. Il riepilogo previsionale usa le stesse prenotazioni dell'avanzamento effettivo.

Nuove sessioni e riprese da snapshot iniziano in pausa. Gli ordini sono consentiti in pausa. Il Core non misura tempo reale: accetta avanzamenti espliciti, al massimo 24 ore per chiamata, conservando i tick residui. Le simulazioni più lunghe usano più chiamate.

## Risultati di riferimento

Senza ordini, con un lavoratore per tipo:

| Tempo | Tronchi | Legno lavorato | Pietra |
| --- | --- | --- | --- |
| 0 s | 0 | 120 | 80 |
| 1 s | 2 | 120 | 81 |
| 2 s | 2 | 121 | 82 |
| 60 s | 2 | 179 | 140 |

A lungo termine le scorte raggiungono i limiti e le attività si fermano. Questo comportamento verifica la capacità, non rappresenta una partita conclusa o una strategia ottimale.

## Esecuzione senza finestra

Dopo la validazione della soluzione, dalla radice:

```powershell
dotnet run --project tools/SimRisorse.Simulation -c Release --no-restore -- --seconds 60
dotnet run --project tools/SimRisorse.Simulation -c Release --no-restore -- --hours 32
```

L'output JSON descrive tempo, popolazione, scorte, quantità degli edifici e previsione del prossimo passo. Il programma accetta al massimo 168 ore per esecuzione; la partita del Core non ha quel limite totale. Il tempo simulato non richiede attese reali.

Restano esclusi da questo scenario: crescita, mantenimento alimentare, demolizione, priorità modificabili, militari, isole, vittoria e classifica. Politiche e compromessi sono nell'[ADR 0006](../adr/0006-economic-step-and-resource-precision.md).
