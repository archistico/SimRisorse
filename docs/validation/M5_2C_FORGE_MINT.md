# M5.2c — Confronto delle filiere Forgia/Zecca

**Verificato su Windows il 2026-11-02**, checkpoint **v40**. **M5.2 completata e chiusa**, dopo [Forgia](M5_2A_FORGE.md) e [Zecca](M5_2B_MINT.md). Catalogo corrente **`civil-growth-9`**, 19 tipi di edifici/16 risorse. Prezzi, ricette, formato SQLite/snapshot v2 e interfaccia invariati.

## Che cosa è stato aggiunto

- Comando `--benchmark-forge-mint` nel simulatore definitivo: **nove profili e quattro guardie**, usando il catalogo corrente approvato e i comandi reali di costruzione/assegnazione.
- Confronto controllato dell'obiettivo **240 Spade +360 Monete**, più tre politiche sostenute per **1.800 s e otto ore**, con estrazione, Cibo e Pane attivi.
- Contabilità di ogni risorsa per ogni secondo, produzione effettiva, addetti assegnati/attivi, carenza di Carbone/input e blocco per capienza.
- Replay a secondi interi e **250+750 ms**, con ripristino di un checkpoint a metà secondo durante il replay; metriche e snapshot finali identici.
- Strategia `crafting` dalla nuova partita, tre nuove cronologie nel validatore e due regressioni d'integrazione/persistenza.

I prezzi B restano nell'esperimento storico separato; non si applicano al catalogo giocabile e non sono stati rieseguiti in v40.

## Ipotesi comuni e costi

Colonia sintetica a **40 abitanti /40 posti**, una Casa piccola e una grande, **due Magazzini**: Spade/Monete con capacità 2.000. Scorte iniziali: 1.700 Legno lavorato/Pietra, 100 Cibo/Pane, zero Carbone, Ferro, Oro, Spade e Monete. Materiali di base senza lavoratori, per isolare il costo industriale. Le infrastrutture civili sono preesistenti e **non comprese** nei costi seguenti.

Alimentazione preesistente con sette addetti: Pescatore/Deposito, due Fattorie/Pozzo/Mulino/Forno. Nessuna carenza di Cibo o Pane durante i profili, popolazione sempre a 40 e nessun progresso di crescita.

Tutti i profili acquistano realmente Miniere ferro/oro/carbone, due Fonderie, Forgia e Zecca. Nei controlli ci sono 1.000 unità di ciascun minerale e miniere minerali senza addetti; nelle prove sostenute i minerali iniziano a zero e le due miniere lavorano.

| Politica | Miniere Carbone | Legno lavorato/Pietra spesi | Picco addetti industriali con estrazione |
| --- | ---: | ---: | ---: |
| `staged`, a fasi | 1 | **1.300 /950** | **5** |
| `parallel`, parallela | 2 | **1.430 /1.040** | **8** |
| `contended`, tutto attivo | 1 | **1.300 /950** | **7** |

La politica a fasi ripete **121 s di fusione e 120 s di lavorazione**, spostando due addetti fra Fonderie e Forgia/Zecca. Quella parallela mantiene tutti i trasformatori attivi con due Miniere di carbone. Il controllo con una sola miniera e tutto attivo espone la priorità Ferro 90 → Oro 91 → Forgia 100 → Zecca 101.

La seconda miniera costa **130/90** aggiuntivi. Rispetto alla politica a fasi, il parallelo impiega anche **tre addetti industriali in più al picco**: uno nella miniera aggiuntiva e due per mantenere contemporaneamente attive fusione e lavorazione. I sette addetti alimentari sono esclusi da questi picchi.

## Risultati controllati

Tempi relativi al checkpoint sintetico, **non dalla nuova partita**:

| Profilo | Orizzonte | Primo lotto Spade/Monete | Obiettivo 240/360 | Carbone prodotto/consumato/restante |
| --- | ---: | ---: | ---: | ---: |
| `control-staged` | 241 s | 122 s | **241 s** | 482 /480 /2 |
| `control-parallel` | 122 s | 3 s | **122 s** | 488 /482 /6 |
| `control-contended` | 180 s | Nessuno | Non raggiunto | 360 /358 /2 |

I tempi 241/122 s, già misurati nell'esperimento storico, sono ora verificati nel gioco corrente con prezzi A e contabilità del validatore. Entrambi i profili che raggiungono l'obiettivo eseguono esattamente **120 secondi-addetto** di Forgia e Zecca. Nel caso conteso entrambi perdono tutti i 180 secondi per Carbone insufficiente.

## Produzione sostenuta e capienza

| Politica | Spade/Monete a 30 minuti | Spade/Monete a otto ore | Obiettivo 240/360 |
| --- | ---: | ---: | ---: |
| A fasi | **1.680 /1.998** | **2.000 /1.998** | 241 s |
| Parallela | **2.000 /1.998** | **2.000 /1.998** | 122 s |
| Tutto attivo, una miniera | **0 /0** | **2.000 /1.998** | **2.240 s** |

Nel terzo caso le Fonderie consumano il Carbone fino alla saturazione delle scorte metalliche. I primi lotti di Spade/Monete arrivano a **2.002 s**; quando un metallo è pieno, la relativa Fonderia lascia Carbone ai trasformatori. Non è un blocco permanente, ma un ritardo molto maggiore rispetto alle altre politiche.

Le 1.998 Monete sono il massimo raggiungibile da zero in lotti da tre con capacità 2.000. I due posti residui non autorizzano un lotto parziale.

| Politica a 30 minuti | Forgia: attività /blocco Carbone /blocco capienza | Zecca: attività /blocco Carbone /blocco capienza |
| --- | ---: | ---: |
| A fasi | 840 /0 /0 | 666 /0 /174 |
| Parallela | 1.000 /1 /798 | 666 /1 /1.132 |
| Tutto attivo, una miniera | 0 /1.800 /0 | 0 /1.800 /0 |

Sono **secondi-addetto**, rilevati quando l'addetto è assegnato; nella politica a fasi non tutti i 1.800 secondi vengono trascorsi nella stessa struttura. Le diagnosi possono sovrapporsi: i contatori di carenza input e capienza non sono insiemi disgiunti e non vanno sommati per ricostruire la durata.

A otto ore ogni profilo ha prodotto **9.332 Carbone**, consumandone **7.332** e conservandone **2.000**. Cibo finale 1.999 e Pane 1.099, entro le rispettive capacità e senza carenze intermedie. L'uguaglianza delle scorte finali non rende equivalenti i tempi per ottenerle.

## Quattro guardie e ripresa

Checkpoint separati con capienza ordinaria 1.000 e strutture già presenti, per isolare i confini delle ricette:

| Guardia | Condizione | Risultato dopo un secondo |
| --- | --- | --- |
| `one-slot` | Spade 999, Monete 999 | Nessuna produzione o spesa di Ferro/Oro/Carbone |
| `two-coin-slots` | Spade 999, Monete 998 | Nessuna produzione o spesa di input |
| `exact-fit` | Spade 998, Monete 997 | Entrambe a 1.000; spesi esattamente due Carbone, un Ferro e un Oro |
| `coal-priority` | Due Carbone, Fonderie/Forgia/Zecca presidiate | Le Fonderie consumano i due Carbone; Spade/Monete restano zero |

Ogni guardia confronta passo intero e frazionato con ripristino dello snapshot. Tutti i profili verificano inoltre `scorta dopo = scorta prima − consumi abitanti − input + output` per ogni secondo, capacità e crescita; il bilancio cumulativo del Carbone e i secondi attivi di Forgia/Zecca devono coincidere con i prodotti finali.

Due regressioni xUnit aggiunte:

- Nuova colonia: acquisti delle filiere, checkpoint a **2.200,5 s**, confronto fra avanzamento unico e mezzi secondi fino all'obiettivo. Popolazione, frazioni, scorte e lavoratori finali identici, pausa senza avanzamento.
- SQLite reale: Forgia e Zecca attive, **2 Spade/3 Monete e un Carbone** a 1,5 s. Dopo salvataggio/caricamento e altri 500 ms: **4 Spade/3 Monete**, zero Carbone. La priorità della Forgia resta corretta e non compare un secondo lotto Monete.

## Percorso integrato dalla nuova partita

`--scenario civil-growth --strategy crafting` conserva i passaggi civili/industriali di riferimento: Cibo dall'avvio, Casa piccola a 61 s, Pane a 600 s, Casa grande a 760 s, Ferro a 1.100 s e Oro a 1.500 s. A **2.200 s** acquista e presidia seconda Miniera di carbone, Forgia e Zecca, per **580 Legno lavorato /430 Pietra** aggiuntivi. Nessun minerale, metallo o prodotto finale precaricato.

| Misura | 2.201 s, primi lotti | 2.320 s, obiettivo | Otto ore |
| --- | ---: | ---: | ---: |
| Spade | 2 | **240** | 1.000 |
| Monete | 3 | **360** | 999 |
| Ferro | 999 | 999 | 1.000 |
| Oro | 699 | 699 | 1.000 |
| Carbone | 502 | 502 | 1.000 |
| Legno lavorato | 130 | 249 | 1.000 |
| Pietra | 571 | 690 | 1.000 |
| Popolazione | 48 | 48 | 48 |
| Lavoratori liberi | 30 | 30 | 30 |

L'obiettivo è raggiunto a **38 minuti e 40 secondi di simulazione**. Le capacità sono quelle base, senza i due Magazzini sintetici: per questo a otto ore si conservano 1.000 Spade e 999 Monete. È una cronologia fattibile e riproducibile, non una prova di ottimalità.

## Gate eseguito e riproduzione

Comando `validate.cmd`, restore online con lock, SDK **10.0.201**, Avalonia 12.1.4, provider SQLite 10.0.14. Release con **zero avvisi/errori**. Run **`1c74597068ab4f96b4f6fd6d30518b45`**:

| Suite | Superati |
| --- | ---: |
| Core | 67 |
| Application | 97 |
| Infrastructure | 35 |
| Desktop | 61 |
| **Totale** | **260/260** |

Raggiungibilità di entrambi i cataloghi, **23 cronologie più crescita accelerata**, **sei benchmark**, pubblicazione e smoke nativi Windows dei due scenari superati. **33 PNG** generati (31 richiesti più due dettagli); nessuna modifica UI o nuova ispezione grafica necessaria in v40. I layout restano coperti dai test Desktop e dai rapporti precedenti. Salvataggi di verifica separati dalle partite personali.

Artefatti locali ignorati da Git:

- `.local-cache/m4/validation.json` e `crafting-validation.log`.
- `.local-cache/m4/runs/1c74597068ab4f96b4f6fd6d30518b45/forge-mint-benchmark.json`.
- Nella stessa run: `crafting-start.json`, `crafting-target.json`, `crafting-8-hours.json`, TRX e render. Questi quattro nuovi JSON hanno una copia corrente anche sotto `.local-cache/m4/`; il report conserva i percorsi della run.

```powershell
dotnet run --project tools/SimRisorse.Simulation -c Release --no-restore -- --benchmark-forge-mint
dotnet run --project tools/SimRisorse.Simulation -c Release --no-restore -- --scenario civil-growth --strategy crafting --seconds 2320
.\validate.cmd
```

## Chiusura e prossimo incremento

**M5.2 è Verificata e chiusa**: prime produzioni, costi, contesa, capienza, salvataggi, replay, confronto delle politiche e percorso integrato sono coperti da artefatti del validatore. Non cambiano regole v9 o SQLite v2. I rapporti e l'esperimento storici conservano le proprie misure senza essere presentati come nuove esecuzioni.

Prima di **M5.3 Caserma** occorre scegliere prezzo/prerequisiti, quantità di Spade per recluta, modello e durata/capienza dell'addestramento, alloggi/razioni e annullamento/congedo. **Il consumo delle Spade all'arruolamento è già deciso**, come prezzi/ricette industriali e S1. [Dossier militare aggiornato](../design/M5_3_ARMY_PREFEASIBILITY.md).

Nessun soldato, nave o pagamento introdotto. Gli obiettivi di circa otto ore efficienti/32+ ore lente riguardano la campagna fino alla vittoria, ancora da misurare in M6. Altre piattaforme, DPI fisici e accessibilità restano fuori da questo gate Windows.
