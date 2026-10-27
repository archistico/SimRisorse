# Scenario Civil Growth

Stato corrente: **`civil-growth-7` verificata**, 17 edifici / 14 risorse, **204/204 test**. M4 chiusa; prototipo monospace approvato e demolizione rinviata. M5.1a introduce Ferro/Carbone e M5.1b l'Oro con parametri A approvati separatamente e continuità S1: [ultimo report](../validation/M5_1B_GOLD.md). Cibo/Pane, crescita e abitazioni restano invariati. I benchmark storici conservano v4/v5; v6 resta disponibile per compatibilità.

## Dotazione e catalogo

Il nuovo scenario conserva le tre strutture e le regole produttive di [Economy Foundation](ECONOMY_FOUNDATION.md): 8 abitanti, un taglialegna, una segheria e un tagliapietra, con un lavoratore ciascuno. Scorte iniziali: 120 legno lavorato, 80 pietra, zero tronchi, **30 Cibo, zero Pesce e zero Pane**. Capacità di base 1.000 per otto risorse, ma solo **100 per Pane**. Una Casa piccola già costruita ospita gli 8 abitanti; 5 sono liberi.

| Nuovo edificio | Costo | Personale | Effetto o ricetta | Prerequisito |
| --- | --- | --- | --- | --- |
| Casa piccola | 80 legno lavorato + 40 pietra | Nessuno | +8 posti abitativi per edificio | Nessuno |
| **Casa grande** | **160 legno lavorato + 80 pietra** | Nessuno | **+32 posti abitativi** per edificio | Almeno 1 forno |
| Magazzino | 100 legno lavorato + 60 pietra | Nessuno | +500 capacità per ognuna delle nove risorse, per edificio | Nessuno |
| Pescatore | 40 legno lavorato + 20 pietra | 1 per edificio | 1 pesce per lavoratore/s | Nessuno |
| Deposito alimentare | 60 legno lavorato + 30 pietra | 1 per edificio | 1 pesce → 1 cibo per lavoratore/s | Almeno 1 pescatore |
| Fattoria | 60 legno lavorato + 30 pietra | 1 per edificio | 1 grano per lavoratore/s | Nessuno |
| Pozzo | 30 legno lavorato + 40 pietra | 1 per edificio | 1 acqua per lavoratore/s | Nessuno |
| Mulino | 120 legno lavorato + 80 pietra | 1 per edificio | 2 grano → 1 farina per lavoratore/s | Almeno 1 fattoria |
| Forno | 150 legno lavorato + 100 pietra | 1 per edificio | 1 farina + 1 acqua → **3 Pane** per lavoratore/s | Almeno 1 mulino e 1 pozzo |

Il deposito alimentare trasforma pesce in Cibo di base e non aumenta la capacità; lo spazio aggiuntivo viene dal magazzino. Carne e altre trasformazioni non sono ancora implementate. Le priorità delle strutture aggiunte sono 35, 36, 37, 40, 50, 55, 56, 60 e 70, dopo i materiali di base; case e magazzino non eseguono ricette. Grano, acqua e farina partono da zero con capacità di base 1.000.

## Filiera del pane

Con la decisione approvata il 2026-10-27, il Pane è **una risorsa distinta dal Cibo**. Tutti gli abitanti consumano Cibo; chi occupa virtualmente una Casa grande consuma anche Pane. Il Forno non aggiunge più Cibo.

| Filiera | Edifici e lavoratori | Costo totale | Output continuo |
| --- | --- | --- | --- |
| Pesce | 1 pescatore + 1 deposito, 2 lavoratori | 100 legno lavorato + 50 pietra | 1 Cibo/s |
| Pane | 2 fattorie + 1 pozzo + 1 mulino + 1 forno, 5 lavoratori | 420 legno lavorato + 280 pietra | 3 Pane/s |

Ogni anello lavora dal secondo successivo a quello in cui il suo input è comparso: grano e acqua al primo secondo, farina al secondo, Pane dal terzo. La capacità di Pane iniziale è 100 e il forno produce blocchi indivisibili di 3: da scorta zero raggiunge 99, quindi attende spazio sufficiente. Una filiera del Pane alimenta a regime 200 residenti avanzati al tasso approvato di 0,015/s; il pescato continua a servire per la razione Cibo comune.

## Capacità di magazzino

- Capacità effettiva = 1.000 + 500 × magazzini per le otto risorse precedenti; **100 + 500 × magazzini per Pane**. Non viene salvata: deriva dagli edifici.
- Quando lo spazio libero non basta per l'output completo di un lavoratore, quel lavoratore non produce e non consuma input (regola M2 invariata). La UI segnala "Deposito pieno".
- Il nuovo spazio vale dal secondo successivo alla costruzione. Il magazzino è attivo appena costruito, senza personale.
- Non esiste demolizione: la capacità non diminuisce. Decisione nell'[ADR 0010](../adr/0010-storage-capacity-and-rules-conversion.md).

## Conversione dalle regole precedenti

| Da | A | Strutture aggiunte con quantità zero |
| --- | --- | --- |
| `civil-growth-1` | `civil-growth-2` | Magazzino |
| `civil-growth-2` | `civil-growth-3` | Fattoria, pozzo, mulino, forno |
| `civil-growth-3` | `civil-growth-4` | Casa grande; aggiunta risorsa Pane (zero) e cambio ricetta del Forno |
| `civil-growth-4` | `civil-growth-5` | Nessun edificio o risorsa aggiunti; accelerazione abitativa; resto frazionario iniziale zero |
| `civil-growth-5` | `civil-growth-6` | Miniere ferro/carbone e fonderia a zero; nuove scorte industriali zero, capacità 1.000 +500 per Magazzino; frazione esistente conservata |
| `civil-growth-6` | `civil-growth-7` | Miniera e Fonderia d'oro a zero; scorte nuove zero, capacità 1.000 +500 per Magazzino; Ferro, Carbone e frazioni esistenti conservati |

Le conversioni si concatenano da `civil-growth-1` alla corrente `-7`. Tempo, abitanti, progresso e scorte esistenti restano invariati. Il resto frazionario parte da zero all'introduzione di v5 e si conserva nei passaggi successivi. Il vecchio Cibo rimane Cibo; Pane, metalli e minerali partono da zero ai rispettivi passaggi. Ripresa in pausa, backup dell'originale alla prima scrittura; versioni sconosciute e dati incoerenti rifiutati.

## Crescita e cibo

- Ogni abitante consuma **0,010 cibo/s**, anche se non assegnato.
- Gli abitanti avanzati sono `max(0, popolazione − 8 × case_piccole)` e richiedono in aggiunta **0,015 Pane/s** ciascuno. Non servono assegnazioni né campi aggiuntivi nel salvataggio.
- Con Cibo, Pane necessario e posti liberi, i progressi raggiungono una soglia pari a **60 secondi di crescita base**. Ogni secondo di gioco aggiunge un progresso proporzionale alle case; il nuovo abitante è disponibile dalla fine di quel secondo e consuma dal successivo. Il primo residente avanzato richiede già scorta sufficiente per la propria razione.
- **Accelerazione approvata:** la prima Casa piccola non dà bonus; ogni Casa piccola successiva dà **+15%**, ogni Casa grande dà **+30%**, fino a **+100%** complessivo. Esempi: una Casa piccola = 60 s/abitante; due Case piccole = primo arrivo dopo 53 s (ritmo 1,15×); una piccola e una grande = 47 s (1,30×); a 2× = 30 s. I resti non sono scartati dopo un arrivo.
- Il ritmo è **unico per la colonia**, non nasce una coda autonoma per ciascuna casa. Un nuovo edificio aumenta soltanto il progresso futuro; quanto accumulato rimane valido. La GUI mostra la velocità corrente, il progresso frazionario e una stima dei secondi al prossimo arrivo, valida se le condizioni rimangono favorevoli.
- Il fabbisogno usa la popolazione iniziale del secondo. La nuova persona consuma dal secondo successivo.
- Le razioni sono prenotate prima delle ricette, secondo le stesse regole di contesa. Se manca Pane, la colonia continua a consumare Cibo comune (quando sufficiente); non vengono prelevate frazioni di una razione collettiva.
- La carenza ferma la crescita, senza decessi o penalità alla produzione esistente. È quindi possibile recuperare riassegnando lavoratori e riavviando il cibo.
- Progresso parziale (anche il resto in ventesimi) fermo in pausa, a case piene o durante carenze. Nessun avanzamento offline o accumulo di arrivi oltre la capienza. I checkpoint `civil-growth-4` vengono convertiti senza perdere i secondi già maturati.

Come nel Core M2, input e spazio sono quelli iniziali del passo: output e spazio liberato dai consumi diventano utilizzabili dal secondo successivo. Per esempio, cibo prodotto durante una carenza non nutre la popolazione retroattivamente nello stesso secondo.

Ordini immediati, acquisti da 1/5/10, assegnazione manuale, precisione in millesimi e pausa restano le regole esistenti. Case e produttori sono nello stesso elenco; la UI distingue effetti abitativi e ricette produttive.

### Come leggere la situazione del Pane nella UI

Quando la colonia ospita residenti avanzati, la colonna Crescita mostra anche un'indicazione di rifornimento:

- **Capacità nominale dei Forni insufficiente**: la somma della produzione massima dei lavoratori assegnati ai Forni è inferiore al consumo dei residenti avanzati. La UI indica ora **quanti Pane/s mancano** e quanti addetti supplementari richiederebbe, al minimo, la ricetta del Forno. Occorre aumentare la capacità produttiva o ridurre i residenti avanzati con altre Case piccole.
- **Filiera Pane limitata**: i Forni potrebbero produrre abbastanza, ma nel prossimo secondo mancano input o altre condizioni operative; controllare Fattorie, Pozzi, Mulini, scorte e assegnazioni.
- **Forno in attesa: deposito quasi pieno**: non c'è spazio per un lotto completo. Un saldo negativo del prossimo secondo non significa necessariamente che serva un altro Forno: la scorta esistente continua ad alimentare gli abitanti.
- **Produzione Pane sufficiente nel prossimo secondo**: fotografia del passo immediato, non garanzia di sostenibilità futura se cambiano residenti, forniture o scorte.

L'**autonomia senza produzione** divide la scorta attuale per la domanda attuale, arrotonda per difetto e mostra ore, minuti o secondi. Per esempio, 20 Pane con nove residenti avanzati (0,135/s) corrispondono a **2 min 28 s**. Non considera nuove persone o produzione successiva; non è un conto alla rovescia verso una carestia. Questa diagnosi non modifica il motore né viene persistita.

Esempio di capacità: **220 abitanti con una Casa piccola** corrispondono a 212 residenti avanzati. Occorrono `212 × 0,015 = 3,180 Pane/s`. Un Forno con un lavoratore ha capacità nominale di `3 Pane/s`: mancano **0,180 Pane/s**, equivalenti a **un ulteriore addetto al Forno** (e quindi, nello scenario attuale, un ulteriore edificio, perché ogni Forno ospita un addetto). Il personale addizionale è soltanto il minimo *nominale*: la seconda filiera di Fattorie, Pozzo, Mulino e Forno deve anche procurare input e spazio. Due Forni assegnati con zero Farina non producono Pane. La proiezione mostra distintamente questa carenza di input.

Per evitare che la capacità del Forno sia scambiata per quella dell'intera catena, il modello di lettura espone anche una [valutazione quantitativa degli anelli Grano, Acqua, Farina e Pane](BREAD_CHAIN_CAPACITY.md). Il limite medio teorico viene stimato dalle ricette e dagli addetti, ma non sostituisce la previsione dei lotti produttivi nel prossimo secondo né un benchmark giocato.

**Prima dell'arrivo del primo residente avanzato**, il consumo corrente di Pane è ancora zero. Se una Casa grande è disponibile ma la scorta è insufficiente per il prossimo arrivo, la crescita mostra “Manca pane” e un'indicazione dedicata invita a controllare Forno, filiera e lavoratori. Il messaggio è visibile anche con zero residenti avanzati e scompare quando la scorta diventa sufficiente; ricompare la diagnosi ordinaria appena arriva il primo residente avanzato.

L'interfaccia e il Core conservano la distinzione fra produzione e disponibilità: se una filiera riprende a funzionare durante un secondo bloccato dalla carenza, **la risorsa prodotta può sostenere la crescita soltanto dal secondo seguente**. L'abitante arrivato consuma a sua volta dal passo successivo al suo arrivo. Sono predisposte prove automatiche per entrambi i recuperi (prima Pane, poi Cibo), in attesa del gate .NET.

## Cronologie riproducibili della baseline storica `civil-growth-4`

**Attenzione:** i tempi esatti delle cronologie seguenti riguardano le **regole storiche a intervallo fisso** e restano verificabili aggiungendo `--rules legacy` al simulatore. Sono conservati per confrontare i risultati dei gate M4 precedenti. Le nuove partite e il Desktop utilizzano invece `civil-growth-5` e quindi possono raggiungere più velocemente gli stessi traguardi.

La strategia `bootstrap` costruisce e assegna un pescatore e un deposito al tempo zero, con costo totale di 100 legno lavorato e 50 pietra. Al secondo 61 ha i materiali per costruire un'altra casa senza fermare la produzione di base. Al secondo **121** arriva il nono abitante; ci sono 4 lavoratori liberi e **140,32 cibo**. Il tempo passato con case piene non è stato accumulato per la crescita.

La strategia `recovery` costruisce prima una casa. Al secondo **600** ci sono 13 abitanti e zero cibo; gli arrivi sono fermi, mentre i tre produttori di base lavorano ancora. Costruendo e assegnando a quel punto pescatore e deposito, al secondo **662** arriva il quattordicesimo abitante. La scarsità iniziale è recuperabile senza riavviare la partita.

Con due case, `bootstrap` raggiunge la capienza di 16 abitanti. Le simulazioni di 8 e 32 ore mantengono popolazione e risorse nei limiti: non sono misure di vittoria o di durata ottimale del gioco completo.

La strategia `storage` segue `bootstrap` e al secondo **1200** trova legno lavorato, pietra e cibo a 1.000: segheria, tagliapietra e deposito alimentare sono fermi per deposito pieno, mentre tronchi e pesce si accumulano. Costruisce un magazzino (restano 900 legno lavorato e 940 pietra), la capacità sale a 1.500 e la produzione riprende dal secondo successivo: al secondo **1260** la pietra è a 1.000.

La strategia `bread` segue `bootstrap` e al secondo **600**, con 16 abitanti, 539 legno lavorato e 590 pietra, costruisce e assegna l'intera filiera del Pane (restano 119 legno lavorato, 310 pietra e 6 persone libere). Dal terzo secondo il forno lavora; al secondo **610** la previsione è 3 Pane/s e 1 Cibo/s. Entro il secondo **660** la scorta Pane raggiunge 99 e il Forno attende spazio libero.

La strategia **`advanced`** segue `bread`, costruisce una Casa grande al secondo **760** e riattiva la crescita. I risultati previsti sono 18 abitanti al secondo 880, di cui 2 avanzati; fabbisogno Pane 0,030/s e capacità abitativa totale 48. La cronologia viene provata anche a 8 ore. **Il gate precedente è stato dichiarato superato dall'utente**, ma il JSON numerico non è allegato qui. Non è un benchmark di ottimalità.

Il confronto storico `--benchmark-housing` rimane identico alla baseline `civil-growth-4`: tre politiche alternative in otto ore e tempi per 48, 100 e 136 abitanti. **Il nuovo `--benchmark-housing-accelerated` ripete le stesse politiche con `civil-growth-5`**, registrando il vantaggio effettivo delle case sul numero e sui tempi degli arrivi. Entrambi riconciliano ogni secondo con il motore. Risultati e costi storici nella [scheda comparativa](HOUSING_BENCHMARK.md); il nuovo JSON va letto dopo il gate v24.

## Avvio e verifiche

```powershell
.\run-game.cmd
dotnet run --project tools/SimRisorse.Simulation -c Release --no-restore -- --scenario civil-growth --strategy bootstrap --seconds 180
dotnet run --project tools/SimRisorse.Simulation -c Release --no-restore -- --scenario civil-growth --strategy bootstrap --seconds 121 --rules legacy
dotnet run --project tools/SimRisorse.Simulation -c Release --no-restore -- --scenario civil-growth --strategy recovery --seconds 662 --rules legacy
dotnet run --project tools/SimRisorse.Simulation -c Release --no-restore -- --scenario civil-growth --strategy storage --seconds 1260 --rules legacy
dotnet run --project tools/SimRisorse.Simulation -c Release --no-restore -- --scenario civil-growth --strategy bread --seconds 660 --rules legacy
dotnet run --project tools/SimRisorse.Simulation -c Release --no-restore -- --scenario civil-growth --strategy advanced --seconds 880 --rules legacy
dotnet run --project tools/SimRisorse.Simulation -c Release --no-restore -- --benchmark-housing-accelerated
```

Il desktop usa `LocalApplicationData/SimRisorse/civil-growth.sqlite3`, sempre ripreso in pausa. La vecchia partita rimane separata: `run-game.cmd --scenario economy-foundation`. Formato e limiti in [SAVE_FORMAT.md](../SAVE_FORMAT.md), decisione nell'[ADR 0009](../adr/0009-civil-growth-and-save-generations.md), evidenze nel [report M4](../validation/M4_CIVIL_GROWTH.md).

Restano per M4: validare **v27** con build, test e render Windows, misurare strategie e tempi (anche confrontando case piccole e grandi), quindi **decidere** se Carne e demolizione siano dentro il perimetro attuale o vadano rinviate. Non sono meccaniche approvate. Esercito e isole sono M5; nessuna vittoria o classifica è introdotta da questo scenario.
