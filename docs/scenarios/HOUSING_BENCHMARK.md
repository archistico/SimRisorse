# M4 — Confronto riproducibile delle strategie abitative

Data: 2026-10-27. **Storico M4 `civil-growth-4` dichiarato validato dall'utente nei checkpoint precedenti, senza JSON grezzi allegati.** In v24 il comando originale `--benchmark-housing` resta identico: rappresenta il confronto a velocità fissa. Il nuovo `--benchmark-housing-accelerated` ripete esattamente le tre politiche con `civil-growth-5`, bonus abitativi e residui frazionari. Entrambi riconciliano ogni secondo; il confronto accelerato **non è ancora stato eseguito con .NET** in questo ambiente.

Il confronto usa il motore di gioco effettivo per tre politiche dichiarate. Il tempo viene avanzato in passi di un secondo per otto ore simulate; nel corso dell'esperimento vengono costruiti e assegnati lavoratori secondo orari e condizioni fissi. Non è un risolutore di strategia ottima.

## Politiche confrontate

| Nome JSON | Abitazioni | Filiera Pane | Seconda filiera Cibo | Magazzino |
| --- | --- | --- | --- | --- |
| `small-only` | Solo Case piccole fino a 17 complessive; se i posti sono pieni, compra una nuova Casa piccola appena può | Assente | Quando raggiunge 80 abitanti | Assente |
| `large-standard` | Una Casa piccola aggiuntiva; Casa grande al secondo 760 e poi fino a quattro Case grandi, quando i posti sono pieni | Completa, al secondo 600 | Quando raggiunge 80 abitanti | Assente |
| `large-early` | Una Casa piccola aggiuntiva; Casa grande al secondo 641 e poi fino a quattro, quando i posti sono pieni | Completa, al secondo 481 | Al secondo 900 | Al secondo 900 |

Tutte costruiscono all'inizio un Pescatore e un Deposito alimentare e assegnano i rispettivi lavoratori. La prima Casa piccola aggiuntiva è comprata appena la segheria produce i materiali: **al secondo 61** per tutte e tre. La filiera Pane completa comprende due Fattorie, un Pozzo, un Mulino e un Forno (cinque lavoratori). Ogni acquisto deve essere realmente pagabile e ogni assegnazione deve rispettare la popolazione disponibile; altrimenti il confronto fallisce esplicitamente.

La strategia `large-early` non è necessariamente la migliore. Anticipa l'investimento e aumenta la riserva di Pane e Cibo, con un costo finanziario maggiore. Le tre cronologie rappresentano scelte comprensibili e ripetibili, non una ricerca esaustiva.

## Risultati attesi dalle regole, **da verificare con il motore**

Gli orari qui sotto si ricavano da una crescita al massimo di una persona ogni 60 secondi, partendo dagli otto abitanti iniziali e dagli sblocchi abitativi stabiliti. **Non sono risultati misurati** finché il comando non viene eseguito su .NET.

| Strategia | 48 abitanti | 100 abitanti | 136 abitanti | Popolazione massima con gli acquisti previsti |
| --- | ---: | ---: | ---: | ---: |
| Solo Case piccole | 2.461 s | 5.581 s | 7.741 s | 136 |
| Case grandi standard | 2.680 s | 5.800 s | 7.960 s | 144 |
| Case grandi anticipate | 2.561 s | 5.681 s | 7.841 s | 144 |

Le Case grandi standard accumulano una pausa a 16 abitanti tra il completamento delle prime due Case piccole e il nuovo sblocco al secondo 760. Anticipare la filiera e la costruzione di Casa grande riduce questa attesa. Le Case piccole risultano comunque più rapide su questi obiettivi **se i materiali e il Cibo non diventano limitanti**: l'arrivo di un nuovo abitante rimane vincolato ai 60 secondi, indipendentemente dalla capacità degli edifici.

La tabella è un controllo teorico della cronologia, non una conclusione di bilanciamento. Un'eventuale carenza di Pane, Cibo o materiali emergerebbe come differenza o errore nei risultati eseguiti.

### Controllo aritmetico indipendente (non è un test .NET)

Un secondo modello di calcolo, separato dal programma C#, ha ripercorso 28.800 passi per strategia con i costi, la precisione al millesimo, le capacità, le prenotazioni prima delle produzioni e lo sblocco dei nuovi abitanti. I tempi e i costi previsti nelle tabelle coincidono. Sono emerse queste **previsioni aggiuntive**, ancora da confrontare con l'esecuzione del motore vero:

| Strategia | Secondi con case piene | Secondi Forno attivo | Secondi Forno limitato dallo spazio | Cibo a 8 h | Pane a 8 h |
| --- | ---: | ---: | ---: | ---: | ---: |
| Solo Case piccole | 21.120 | 0 | 0 | 997,680 | 0 |
| Case grandi standard | 20.640 | 15.501 | 12.697 | 997,880 | 96,600 |
| Case grandi anticipate | 20.640 | 15.744 | 12.573 | 1.498,560 | 597,120 |

Il Forno passa molto tempo a riposo per spazio insufficiente, ma ciò non implica automaticamente una carenza: produce lotti da 3 Pane mentre la popolazione consuma meno di 3/s. Il Magazzino della terza strategia conserva più scorta, senza aumentare il numero di abitanti raggiunti con le regole di crescita adottate. La produttività e i tempi vanno verificati anche con il Core; questo modello indipendente può aver trascurato un caso limite dell'implementazione.

## Materiali impegnati dalle politiche

I costi includono edifici costruiti durante l'esperimento, non gli edifici presenti inizialmente. Si confrontano gli investimenti necessari per attraversare la soglia dei 136 abitanti; le Case grandi arrivano a 144 posti complessivi perché la Casa piccola aggiuntiva resta in servizio.

| Strategia | Case acquistate | Filiere Cibo | Filiera Pane | Magazzino | Totale legno lavorato | Totale pietra |
| --- | --- | --- | --- | --- | ---: | ---: |
| Solo Case piccole | 16 piccole | 2 | 0 | 0 | 1.480 | 740 |
| Case grandi standard | 1 piccola + 4 grandi | 2 | 1 | 0 | 1.340 | 740 |
| Case grandi anticipate | 1 piccola + 4 grandi | 2 | 1 | 1 | 1.440 | 800 |

Nella [valutazione iniziale](ADVANCED_HOUSING_PROPOSAL.md) il pareggio materiale delle Case grandi alla quarta si riferisce a 128 posti aggiuntivi con una sola filiera Pane e non include la Casa piccola di servizio. Nelle due politiche qui simulate viene comprata **una Casa piccola aggiuntiva** per rendere disponibili lavoratori prima dell'investimento nel Pane. Questo sposta il confronto pratico: alla quarta Casa grande il risparmio è **140 legno lavorato e zero pietra** rispetto alla politica con sole Case piccole, con otto posti liberi in più. Non dimostra ancora che le Case grandi siano economicamente o temporalmente preferibili.

## Indicatori registrati

Il comando produce, per ciascuna strategia:

- tempo simulato per raggiungere 48, 100 e 136 abitanti, se raggiunti;
- popolazione e capacità finali, Case piccole/grandi, Pescatori e Magazzini acquistati;
- materiali spesi, scorte finali di Cibo e Pane e massimo personale impiegato;
- secondi in cui la crescita è bloccata da abitazioni piene, Cibo o Pane mancanti;
- secondi di effettiva produzione del Forno e secondi di blocco per spazio insufficiente;
- rifiuto immediato di acquisti/assegnazioni non validi o scorte oltre la capacità.
- `ReconciledSeconds`: passi verificati confrontando consumi, input/output delle ricette, risorse finali e crescita prevista. Devono essere 28.800 per strategia; una differenza interrompe il benchmark indicando il secondo divergente.

La grandezza `BakeryActiveSeconds` misura i passi con almeno un lavoratore del Forno che potrebbe produrre secondo la previsione; non misura da sola l'utilità marginale del Pane immagazzinato. Il magazzino pieno può far fermare il Forno anche quando la popolazione è nutrita. Le scorte previste dal Core sono calcolate **prima** di ogni passo, rispettando la priorità dei consumi civili.

## Capacità oltre le quattro Case grandi

Il confronto di otto ore raggiunge al massimo **128 residenti avanzati**: con 0,015 Pane/s a persona servono **1,920 Pane/s**, inferiori ai 3 Pane/s della filiera. Il benchmark principale misura quindi lo sblocco e la capacità del deposito, ma non mette ancora sotto pressione la portata produttiva del Forno.

La soglia teorica di alimentazione **continuativa** di un singolo Forno è `3 / 0,015 = 200` residenti avanzati. Oltre questo valore, una scorta può inizialmente compensare la differenza. Una volta esaurita, nei secondi di carenza si ferma il progresso della crescita ma non spariscono abitanti né secondi di progresso validi. **Non è un blocco permanente inevitabile**, perché il Pane può ricostituirsi durante le interruzioni.

È predisposto un test Core con 220 abitanti iniziali (212 avanzati), otto Case grandi e 30 minuti: confronta un Forno, due Forni fin dall'inizio e il secondo Forno aggiunto al minuto 15. Il percorso con due Forni immediati deve ottenere tutti i 30 nuovi abitanti; quello con un solo Forno deve crescere più lentamente, mentre un rinforzo tardivo deve migliorare il risultato.

Un modello aritmetico separato, **non una prova eseguita con .NET**, prevede rispettivamente **246, 250 e 249 abitanti finali** (un Forno, due immediati, secondo tardivo). La suite .NET potrà confermare o correggere questi risultati. Nessun parametro `civil-growth-4` è stato cambiato.

### Il costo di sette Case grandi

Per garantire Pane **senza interruzioni** a sette Case grandi interamente occupate servono 224 × 0,015 = **3,360 Pane/s**, quindi due filiere produttive complete da 3/s. Il costo aggiuntivo della seconda filiera è 420 legno lavorato, 280 pietra e 5 lavoratori. Questa spesa riduce il vantaggio economico delle Case grandi, ma mantiene una scelta:

| Capacità nuova | Politica abitativa | Legno lavorato | Pietra | Lavoratori esclusivi del Pane |
| --- | --- | ---: | ---: | ---: |
| 224 posti | 28 Case piccole | 2.240 | 1.120 | 0 |
| 224 posti | 7 Case grandi + 1 filiera Pane, crescita intermittente a piena occupazione | 1.540 | 840 | 5 |
| 224 posti | 7 Case grandi + 2 filiere Pane, rifornimento continuo | 1.960 | 1.120 | 10 |

I costi confrontano **la stessa capacità aggiunta**, escludono le strutture comuni e l'eventuale Casa piccola supplementare necessaria per assegnare personale nella prima fase. Con due filiere Pane si risparmiano ancora 280 legno lavorato e nessuna pietra rispetto alle 28 Case piccole, ma servono 10 lavoratori dedicati e una sequenza di costruzione più complessa. Una sola filiera può bastare a proseguire più lentamente; il test a 220 abitanti ne verifica proprio la conseguenza.

## Esecuzione e criterio di revisione

Da PowerShell nella radice del progetto:

```powershell
dotnet run --project tools/SimRisorse.Simulation -c Release -- --benchmark-housing
```

La validazione generale `validate.cmd` esegue automaticamente il confronto dopo le dodici cronologie già previste, controlla le soglie teoriche, gli investimenti e l'assenza di carenze alimentari, quindi salva il risultato in `.local-cache/m4/housing-benchmark.json`. Se una previsione è sbagliata, la validazione si ferma e bisogna distinguere un errore di aspettativa da un difetto del Core.

**Gate ancora aperto:** non è presente l'SDK .NET nell'ambiente di preparazione. Non affermare che tempi, spese o inattività siano già stati verificati. Dopo l'esecuzione occorre confrontare soprattutto tempi di blocco per case piene, frequenza del Forno fermo a Pane pieno e impatto del Magazzino. Eventuali cambi di costi o consumi richiedono un nuovo identificatore di regole, nuove simulazioni e politica esplicita di compatibilità dei salvataggi.
