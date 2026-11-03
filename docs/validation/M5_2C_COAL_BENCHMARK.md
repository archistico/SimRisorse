# M5.2c — Benchmark Carbone, Forgia e Zecca (v41, gate Windows pendente)

Data: **28 ottobre 2026**. Il benchmark è **preparato**, non ancora misurato in .NET nell'ambiente di sviluppo: manca l'SDK. La versione **v40** (Forgia/Zecca e fix pennelli statici Avalonia) richiede ancora conferma dei test Desktop e del validatore.

## Scopo e confini

Il comando `--benchmark-forge-mint-coal` usa **le vere regole `civil-growth-9`**, non il catalogo ipotetico del precedente `ForgeMintProbe`, e ripete tre politiche industriali per **1.800** e **3.600** secondi. Lo scopo è misurare la contesa del Carbone a parità di obiettivo: almeno **240 Spade e 360 Monete**.

Il benchmark **non** aggiunge comandi al gioco, modifica ricette, introduce priorità configurabili, tocca i salvataggi dell'utente, attribuisce un valore di mercato alle Monete o approva i costi futuri di soldati/navi.

## Setup controllato

La colonia sintetica possiede 40 abitanti, 40 posti abitativi, un Forno **senza addetti** e otto Magazzini **preesistenti**. Inizia con 2.600 Tavole, 2.400 Pietra, 1.000 minerali di ferro, 1.000 minerali d'oro, 2.000 Cibo, 3.000 Pane, senza scorte di Carbone, Ferro, Oro, Spade o Monete. Le due miniere di minerali sono **comprate effettivamente** ma restano senza lavoratori per isolare la contesa del Carbone. Si comprano inoltre entrambe le fonderie, Forgia, Zecca e una o due miniere di Carbone.

| Strategia | Miniere Carbone | Prezzi industriali Tavole/Pietra | Personale contemporaneo | Regola |
| --- | ---: | ---: | ---: | --- |
| `phased` | 1 | 1.300 / 950 | 3 | Primo periodo con miniera + due fonderie; all'inizio del secondo 122 spostare i due addetti su Forgia e Zecca |
| `parallel` | 2 | 1.430 / 1.040 | 6 | Due miniere, due fonderie, Forgia e Zecca tutte presidiate fin dall'inizio |
| `all-staffed` | 1 | 1.300 / 950 | 5 | Una miniera, due fonderie, Forgia e Zecca tutte presidiate, senza interventi |

I prezzi derivano dalle reali chiamate `Construct`, i lavoratori da `AssignWorkers` e `ReleaseWorkers`. Non includono il costo delle infrastrutture civili già presenti. Tutte le prove hanno scorte e depositi iniziali uguali, salvo la **seconda miniera** nella configurazione parallela.

## Controlli automatici

1. Ogni secondo viene calcolato il forecast e riconciliato con lo stock realmente prodotto/consumato; capacità, popolazione e progresso della crescita non possono divergere.
2. Ogni profilo conta i **secondi-addetto** assegnati, produttivi e bloccati per Carbone, altri input e saturazione del deposito. Le cause diagnostiche possono sovrapporsi nello stesso secondo e non vanno sommate come categorie disgiunte.
3. Vengono registrati il primo lotto di Spade, il primo di Monete, il raggiungimento dell'obiettivo, le scorte finali e i bilanci cumulativi di Carbone, minerali, metalli, Spade e Monete.
4. Ogni profilo è eseguito due volte: passi interi di 1 secondo e passi da 250+750 ms. Durante il replay frazionario viene creato e ripristinato uno snapshot **dentro** un secondo ai passi 100 e 1.000. Risultati e snapshot finali devono essere identici.
5. La prova si interrompe con errore se il motore non riproduce gli oracoli teorici calcolati prima dell'implementazione: **241 secondi** nella strategia a fasi, **122 secondi** in quella parallela. La prova `all-staffed` deve esporre vera carenza di Carbone a Forgia e Zecca e un raggiungimento dell'obiettivo più tardivo.

Questi numeri rimangono **attesi ma non confermati** finché non viene eseguito il codice .NET. Se la simulazione li smentisce, conservare il risultato diagnostico e controllare le ipotesi, non modificare i conti per ottenere una conferma artificiale.

## Esecuzione su Windows

Per isolare il benchmark:

```powershell
dotnet run --project tools/SimRisorse.Simulation/SimRisorse.Simulation.csproj -c Release -- --benchmark-forge-mint-coal
```

Oppure eseguire il gate completo (comprende prima i test xUnit e poi tutti i benchmark):

```powershell
.\validate.cmd
```

Quando riesce, il validatore deposita `forge-mint-coal-benchmark.json` accanto agli altri benchmark in `.local-cache/m4/` e ne registra il percorso nel rapporto del gate. La generazione del file **non equivale** a una misurazione della durata di una partita dall'inizio: il benchmark parte con materie prime, cibo, pane, deposito e alloggi preallestiti.

## Passo successivo

1. Acquisire test Desktop e gate completo di v41 (che verifica anche il fix Avalonia introdotto in v39 e il catalogo v40).
2. Esaminare le sei righe reali del JSON, con particolare attenzione alla carenza `all-staffed`, allo stoccaggio nei 3.600 secondi e alla riproducibilità del replay.
3. Solo dopo il gate decidere se M5.2c richiede un vero scenario **da nuova partita**, con costruzioni, approvvigionamento di minerali e personale civile reali. Separare questi tempi dai 241/122 secondi sintetici.
4. Dopo M5.2, predisporre le decisioni **M5.3 Caserma/reclutamento** senza inventare i costi in Spade o le regole della popolazione militare.
