# Scenario Civil Growth

Stato: **primo incremento M4 implementato e verificato**. Scenario `civil-growth`, regole `civil-growth-1`. I valori restano provvisori per lo sviluppo; il completamento del gioco in otto ore non è ancora misurato.

## Dotazione e catalogo

Il nuovo scenario conserva le tre strutture e le regole produttive di [Economy Foundation](ECONOMY_FOUNDATION.md): 8 abitanti, un taglialegna, una segheria e un tagliapietra, con un lavoratore ciascuno. Scorte iniziali: 120 legno lavorato, 80 pietra, zero tronchi, **30 cibo e zero pesce**. Capacità di ogni risorsa: 1.000. Una casa piccola già costruita ospita gli 8 abitanti; 5 sono liberi.

| Nuovo edificio | Costo | Personale | Effetto o ricetta | Prerequisito |
| --- | --- | --- | --- | --- |
| Casa piccola | 80 legno lavorato + 40 pietra | Nessuno | +8 posti abitativi per edificio | Nessuno |
| Pescatore | 40 legno lavorato + 20 pietra | 1 per edificio | 1 pesce per lavoratore/s | Nessuno |
| Deposito alimentare | 60 legno lavorato + 30 pietra | 1 per edificio | 1 pesce → 1 cibo per lavoratore/s | Almeno 1 pescatore |

Il deposito trasforma pesce in cibo; non aumenta la capacità di magazzino in questo incremento. Carne, pane e altre trasformazioni non sono ancora implementati. Le priorità dei nuovi edifici sono 35, 40 e 50, dopo i materiali di base; la casa non esegue ricette.

## Crescita e cibo

- Ogni abitante consuma **0,010 cibo/s**, anche se non assegnato.
- Con cibo sufficiente e posti abitativi liberi, ogni **60 secondi validi** arriva una persona, disponibile per le assegnazioni.
- Il ritmo è unico per la colonia: più case aumentano i posti, senza moltiplicare la velocità degli arrivi.
- Il fabbisogno usa la popolazione iniziale del secondo. La nuova persona consuma dal secondo successivo.
- La razione civile è prenotata prima delle ricette. Se manca la quantità completa, nessuna razione parziale viene sottratta.
- La carenza ferma la crescita, senza decessi o penalità alla produzione esistente. È quindi possibile recuperare riassegnando lavoratori e riavviando il cibo.
- Progresso parziale fermo in pausa, a case piene o durante carenze. Nessun avanzamento offline o accumulo di arrivi oltre la capienza.

Come nel Core M2, input e spazio sono quelli iniziali del passo: output e spazio liberato dai consumi diventano utilizzabili dal secondo successivo. Per esempio, cibo prodotto durante una carenza non nutre la popolazione retroattivamente nello stesso secondo.

Ordini immediati, acquisti da 1/5/10, assegnazione manuale, precisione in millesimi e pausa restano le regole esistenti. Case e produttori sono nello stesso elenco; la UI distingue effetti abitativi e ricette produttive.

## Cronologie riproducibili

La strategia `bootstrap` costruisce e assegna un pescatore e un deposito al tempo zero, con costo totale di 100 legno lavorato e 50 pietra. Al secondo 61 ha i materiali per costruire un'altra casa senza fermare la produzione di base. Al secondo **121** arriva il nono abitante; ci sono 4 lavoratori liberi e **140,32 cibo**. Il tempo passato con case piene non è stato accumulato per la crescita.

La strategia `recovery` costruisce prima una casa. Al secondo **600** ci sono 13 abitanti e zero cibo; gli arrivi sono fermi, mentre i tre produttori di base lavorano ancora. Costruendo e assegnando a quel punto pescatore e deposito, al secondo **662** arriva il quattordicesimo abitante. La scarsità iniziale è recuperabile senza riavviare la partita.

Con due case, `bootstrap` raggiunge la capienza di 16 abitanti. Le simulazioni di 8 e 32 ore mantengono popolazione e risorse nei limiti: non sono misure di vittoria o di durata ottimale del gioco completo.

## Avvio e verifiche

```powershell
.\run-game.cmd
dotnet run --project tools/SimRisorse.Simulation -c Release --no-restore -- --scenario civil-growth --strategy bootstrap --seconds 121
dotnet run --project tools/SimRisorse.Simulation -c Release --no-restore -- --scenario civil-growth --strategy recovery --seconds 662
```

Il desktop usa `LocalApplicationData/SimRisorse/civil-growth.sqlite3`, sempre ripreso in pausa. La vecchia partita rimane separata: `run-game.cmd --scenario economy-foundation`. Formato e limiti in [SAVE_FORMAT.md](../SAVE_FORMAT.md), decisione nell'[ADR 0009](../adr/0009-civil-growth-and-save-generations.md), evidenze nel [report M4](../validation/M4_CIVIL_GROWTH.md).

Restano da sviluppare per M4: magazzini espandibili e ulteriori fasi civili/alimentari. Demolizione e rimborsi restano da definire; esercito e isole sono M5. Nessuna vittoria o classifica è introdotta da questo scenario.
