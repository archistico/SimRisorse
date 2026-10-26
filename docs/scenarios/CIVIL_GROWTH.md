# Scenario Civil Growth

Stato: **tre incrementi M4 implementati e verificati**. Scenario `civil-growth`, regole `civil-growth-3` (magazzino e filiera del pane aggiunti il 2026-10-27; `civil-growth-1` e `civil-growth-2` vengono convertiti, vedi sotto). I valori restano provvisori per lo sviluppo; il completamento del gioco in otto ore non è ancora misurato.

## Dotazione e catalogo

Il nuovo scenario conserva le tre strutture e le regole produttive di [Economy Foundation](ECONOMY_FOUNDATION.md): 8 abitanti, un taglialegna, una segheria e un tagliapietra, con un lavoratore ciascuno. Scorte iniziali: 120 legno lavorato, 80 pietra, zero tronchi, **30 cibo e zero pesce**. Capacità di base di ogni risorsa: 1.000. Una casa piccola già costruita ospita gli 8 abitanti; 5 sono liberi.

| Nuovo edificio | Costo | Personale | Effetto o ricetta | Prerequisito |
| --- | --- | --- | --- | --- |
| Casa piccola | 80 legno lavorato + 40 pietra | Nessuno | +8 posti abitativi per edificio | Nessuno |
| Magazzino | 100 legno lavorato + 60 pietra | Nessuno | +500 capacità per ognuna delle otto risorse, per edificio | Nessuno |
| Pescatore | 40 legno lavorato + 20 pietra | 1 per edificio | 1 pesce per lavoratore/s | Nessuno |
| Deposito alimentare | 60 legno lavorato + 30 pietra | 1 per edificio | 1 pesce → 1 cibo per lavoratore/s | Almeno 1 pescatore |
| Fattoria | 60 legno lavorato + 30 pietra | 1 per edificio | 1 grano per lavoratore/s | Nessuno |
| Pozzo | 30 legno lavorato + 40 pietra | 1 per edificio | 1 acqua per lavoratore/s | Nessuno |
| Mulino | 120 legno lavorato + 80 pietra | 1 per edificio | 2 grano → 1 farina per lavoratore/s | Almeno 1 fattoria |
| Forno | 150 legno lavorato + 100 pietra | 1 per edificio | 1 farina + 1 acqua → 3 cibo per lavoratore/s | Almeno 1 mulino e 1 pozzo |

Il deposito alimentare trasforma pesce in cibo e non aumenta la capacità; lo spazio aggiuntivo viene dal magazzino. Carne e altre trasformazioni non sono ancora implementate. Le priorità dei nuovi edifici sono 35, 37, 40, 50, 55, 56, 60 e 70, dopo i materiali di base; casa e magazzino non eseguono ricette. Grano, acqua e farina partono da zero con capacità di base 1.000.

## Filiera del pane

Scelta provvisoria del 2026-10-27: il pane **affianca** il pesce come seconda fonte dello stesso `Cibo`; non lo sostituisce, non introduce una risorsa distinta e non cambia il consumo degli abitanti. È più efficiente per lavoratore ma costa molto di più ad avviarsi:

| Filiera | Edifici e lavoratori | Costo | Cibo/s | Cibo per lavoratore/s |
| --- | --- | --- | --- | --- |
| Pesce | 1 pescatore + 1 deposito, 2 lavoratori | 100 legno lavorato + 50 pietra | 1 | 0,5 |
| Pane | 2 fattorie + 1 pozzo + 1 mulino + 1 forno, 5 lavoratori | 420 legno lavorato + 280 pietra | 3 | 0,6 |

Ogni anello lavora dal secondo successivo a quello in cui il suo input è comparso: grano e acqua al primo secondo, farina al secondo, cibo dal terzo. A regime grano (2), farina (1) e acqua (2) restano costanti. Con i valori attuali il pesce basta già a nutrire 100 abitanti, ben oltre i 16 posti raggiungibili con due case: il pane serve per costruire tutti i tipi di edificio (G-01), non per necessità alimentare. Equilibrio e un eventuale vantaggio esclusivo del pane restano da decidere con le prossime abitazioni.

## Capacità di magazzino

- Capacità effettiva = 1.000 + 500 × magazzini, uguale per tutte le otto risorse. Non viene salvata: deriva dagli edifici.
- Quando lo spazio libero non basta per l'output completo di un lavoratore, quel lavoratore non produce e non consuma input (regola M2 invariata). La UI segnala "Deposito pieno".
- Il nuovo spazio vale dal secondo successivo alla costruzione. Il magazzino è attivo appena costruito, senza personale.
- Non esiste demolizione: la capacità non diminuisce. Decisione nell'[ADR 0010](../adr/0010-storage-capacity-and-rules-conversion.md).

## Conversione dalle regole precedenti

| Da | A | Strutture aggiunte con quantità zero |
| --- | --- | --- |
| `civil-growth-1` | `civil-growth-2` | Magazzino |
| `civil-growth-2` | `civil-growth-3` | Fattoria, pozzo, mulino, forno |

Le conversioni si concatenano: un checkpoint `civil-growth-1` passa direttamente a `civil-growth-3`. Tempo, abitanti, progresso della crescita, scorte ed edifici restano invariati e la partita riparte in pausa. Al primo salvataggio successivo il file originale viene copiato in `civil-growth.<regole originali>.backup.sqlite3`, nella stessa cartella. Altre versioni di regole non vengono convertite.

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

La strategia `storage` segue `bootstrap` e al secondo **1200** trova legno lavorato, pietra e cibo a 1.000: segheria, tagliapietra e deposito alimentare sono fermi per deposito pieno, mentre tronchi e pesce si accumulano. Costruisce un magazzino (restano 900 legno lavorato e 940 pietra), la capacità sale a 1.500 e la produzione riprende dal secondo successivo: al secondo **1260** la pietra è a 1.000.

La strategia `bread` segue `bootstrap` e al secondo **600**, con 16 abitanti, 539 legno lavorato e 590 pietra, costruisce e assegna l'intera filiera del pane (restano 119 legno lavorato, 310 pietra e 6 persone libere). Dal terzo secondo il forno lavora; al secondo **660** il cibo prodotto è **4/s** (1 dal pesce, 3 dal pane).

## Avvio e verifiche

```powershell
.\run-game.cmd
dotnet run --project tools/SimRisorse.Simulation -c Release --no-restore -- --scenario civil-growth --strategy bootstrap --seconds 121
dotnet run --project tools/SimRisorse.Simulation -c Release --no-restore -- --scenario civil-growth --strategy recovery --seconds 662
dotnet run --project tools/SimRisorse.Simulation -c Release --no-restore -- --scenario civil-growth --strategy storage --seconds 1260
dotnet run --project tools/SimRisorse.Simulation -c Release --no-restore -- --scenario civil-growth --strategy bread --seconds 660
```

Il desktop usa `LocalApplicationData/SimRisorse/civil-growth.sqlite3`, sempre ripreso in pausa. La vecchia partita rimane separata: `run-game.cmd --scenario economy-foundation`. Formato e limiti in [SAVE_FORMAT.md](../SAVE_FORMAT.md), decisione nell'[ADR 0009](../adr/0009-civil-growth-and-save-generations.md), evidenze nel [report M4](../validation/M4_CIVIL_GROWTH.md).

Restano da sviluppare per M4: una progressione che renda necessarie le filiere avanzate (per esempio abitazioni più grandi), carne e strumenti per i colli di bottiglia. Demolizione e rimborsi restano da definire; esercito e isole sono M5. Nessuna vittoria o classifica è introdotta da questo scenario.
