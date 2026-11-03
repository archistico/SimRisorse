# M5.2d — Dalla prima colonia a Spade e Monete (v42, verifica Windows pendente)

Data: **28 ottobre 2026**. Il benchmark `--benchmark-forge-mint-campaign` è stato **scritto ma non eseguito in .NET** in questo ambiente. Il gate del catalogo v40, il fix Desktop v39 e i due benchmark successivi restano da acquisire su Windows. **Non sono state cambiate le regole `civil-growth-9`.**

## Perché esiste

Il benchmark M5.2c utilizza una colonia artificiale già sviluppata con 1.000 minerali di ferro e oro pronti; stima la sola contesa del Carbone. Questo nuovo controllo parte invece **esattamente da `GameSession.Create(CivilGrowthScenario.Create())`**: 8 abitanti, 120 Tavole, 80 Pietra, 30 Cibo, una Casa piccola e gli addetti iniziali a legna e pietra. Zero miniere, alloggi avanzati, depositi, minerali, metalli, Spade e Monete pre-caricati.

Le decisioni vengono eseguite con comandi autentici del Core (`Construct`, `AssignWorkers`, `ReleaseWorkers`); nessuna scorciatoia di risorse, popolazione, lavoratori, capacità o tempo. La cronologia civile/industriale iniziale riproduce la strategia `gold` già presente nel simulatore.

## Cronologia programmata del giocatore

| Secondo simulato | Azione |
| ---: | --- |
| 0 | Costruire e presidiare Pescatore e Deposito alimentare |
| 61 | Costruire una seconda Casa piccola |
| 600 | Due Fattorie, Pozzo, Mulino, Forno; presidiare tutti |
| 760 | Costruire Casa grande |
| 1.100 | Costruire/presidiare Miniera di ferro, Miniera di carbone, Fonderia di ferro |
| 1.500 | Costruire/presidiare Miniera d'oro e Fonderia d'oro |
| Da 2.000 | Acquistare Forgia e Zecca **solo quando le scorte coprono insieme il costo reale** |

Le tre varianti sono:

| Strategia | Carbone | Operazione all'apertura delle trasformazioni | Costo cumulativo Tavole/Pietra |
| --- | --- | --- | ---: |
| `phased-one-mine` | 1 miniera | Spostare gli addetti dalle due fonderie a Forgia e Zecca | **2.060 / 1.400** |
| `concurrent-one-mine` | 1 miniera | Conservare gli addetti alle fonderie e aggiungerne due a Forgia/Zecca | **2.060 / 1.400** |
| `concurrent-two-mines` | 2 miniere | Comprare/presidiare prima la seconda Miniera di carbone, poi Forgia/Zecca quando pagabili | **2.190 / 1.490** |

**Nota di equilibrio:** con l'apertura storica `gold`, la miniera del Carbone precede di 400 secondi la seconda fonderia. In quel periodo può accumularsi Carbone. Per questo una strategia con **una sola miniera** potrebbe sostenere temporaneamente quattro lavorazioni utilizzando la scorta; la seconda miniera costa 130 Tavole/90 Pietra e può ritardare gli acquisti successivi. I tempi di arrivo all'obiettivo devono essere letti dal **rapporto reale**, senza trasferire i 241/122 s del benchmark sintetico a questa apertura.

L'obiettivo è raggiungere **240 Spade e 360 Monete** entro **2.400 secondi simulati**, con calcolo del primo lotto, tempi di acquisto, Carbone in magazzino prima dell'apertura, Carbone residuo al traguardo, abitanti, addetti massimi, blocchi per mancanza di Carbone e scorte finali.

## Cosa viene verificato

1. Tutte le costruzioni avvengono solo dopo prerequisiti e disponibilità di materiale; il costo di ciascuna viene registrato.
2. Ogni secondo l'inventario ottenuto è confrontato con il piano economico del Core, inclusi consumo di Cibo/Pane e produzioni industriali; capienza mai superata.
3. Le strategie sono ripetute con passi interi da 1 s e passi di 250+750 ms. Nel replay frazionario la sessione viene ripristinata da uno snapshot a metà secondo ai passi 999 e 1.999. Risultato dettagliato e snapshot finale devono coincidere.
4. Il gate PowerShell controlla prezzi cumulativi, acquisti, raggiungimento degli obiettivi e durata di 2.400 secondi. I **tempi effettivi non sono precompilati** nella documentazione e non devono essere dichiarati prima dell'esecuzione.

## Esecuzione

```powershell
dotnet run --project tools/SimRisorse.Simulation/SimRisorse.Simulation.csproj -c Release -- --benchmark-forge-mint-campaign
```

Oppure il gate completo, che comprende i test e tutti i benchmark:

```powershell
.\validate.cmd
```

Il report viene scritto in `.local-cache/m4/forge-mint-campaign-benchmark.json` e il percorso viene inserito in `validation.json` **solo se l'esecuzione riesce**. L'archivio ZIP del progetto preserva il benchmark e il protocollo, non i risultati di una simulazione non ancora eseguita.

## Limiti e prossimo gate

È **una sola apertura deterministica**, con acquisti programmati a tempi fissi e addetti gestiti secondo un copione: non dimostra che sia la migliore strategia né che otto ore siano sufficienti per vincere. Non riguarda il reclutamento, le navi o le conquiste. Dopo aver acquisito la validazione v42 e confrontato i tre JSON reali (M5.1c, M5.2c, M5.2d), si potrà chiudere la filiera M5.2 oppure preparare un'ulteriore variante di strategia, motivata dai dati.
