# M4 — Proposta per case avanzate e pane

Data: 2026-10-27. **Stato: parametri approvati dall'utente e implementati in `civil-growth-4`; benchmark e test Windows in attesa.**

Questo documento conserva la valutazione originale dell'estensione di `civil-growth-3` che ha portato alla decisione approvata; l'implementazione è documentata nell'[ADR 0011](../adr/0011-advanced-housing-and-bread-rations.md). I conti di confronto qui sotto sono aritmetici e **non** risultati di benchmark eseguiti con il motore.

## Problema attuale

- Un pescatore e un deposito alimentare costano complessivamente 100 legno lavorato e 50 pietra, impiegano 2 persone e producono 1 Cibo/s. Con 0,010 Cibo/abitante/s possono nutrire 100 abitanti.
- La filiera del forno costa 420 legno lavorato e 280 pietra, impiega 5 persone e produce 3 Cibo/s. Alimenta fino a 300 abitanti con il medesimo Cibo, ma le due case iniziali della cronologia di riferimento ospitano solo 16 persone.
- Anche aggiungendo case grandi, la produzione di pesce si potrebbe moltiplicare. Usare il forno come unico prerequisito edilizio ne renderebbe necessaria la **costruzione**, non il **funzionamento**.
- Un obbligo di forno operativo basato soltanto su `RunnableWorkers` può bloccare la crescita quando il deposito del Cibo è pieno, benché la colonia sia ben nutrita. Va evitata questa dipendenza indiretta dalla capacità di un output condiviso.

## Soluzioni confrontate

| Alternativa | Vantaggio | Limite | Valutazione |
| --- | --- | --- | --- |
| A. Casa grande richiede il forno, ma si consuma soltanto Cibo | Poche modifiche al Core e salvataggi | Basta costruire il forno e poi spegnerlo; il pane non ha un ruolo alimentare reale | Non raccomandata come soluzione definitiva |
| B. Casa grande richiede un forno attivo che produce il medesimo Cibo | Nessuna nuova risorsa | Falsi blocchi a magazzino pieno, difficoltà a distinguere l'origine del Cibo; incentivi a lasciare il forno acceso anche senza bisogno | Non raccomandata |
| **C. Pane come risorsa distinta, richiesta dagli abitanti in case avanzate** | Consumi verificabili, recupero dalle carenze, filiera realmente utile; confronto strategico con più case piccole | Serve estendere il piano civile, la UI e la conversione delle regole | **Raccomandata** |

## Parametri proposti per `civil-growth-4`

| Elemento | Valore attuale | Proposta |
| --- | --- | --- |
| Casa piccola | +8 posti, 80 legno lavorato + 40 pietra, senza personale | Invariata |
| **Casa grande** | Assente | **+32 posti**, costo **160 legno lavorato + 80 pietra**, nessun lavoratore |
| Prerequisito Casa grande | — | Almeno **un Forno costruito**; la crescita richiederà anche pane disponibile |
| Nuova risorsa `bread` | Assente | Nome visibile **Pane**, scorta iniziale zero, capacità iniziale **100** |
| Magazzino | +500 capacità per ciascuna delle otto risorse | +500 per ciascuna delle **nove** risorse, incluso Pane |
| Forno | 1 Farina + 1 Acqua → 3 Cibo/s per lavoratore | 1 Farina + 1 Acqua → **3 Pane/s** per lavoratore |
| Alimentazione comune | 0,010 Cibo per abitante/s | **Invariata per tutti gli abitanti** |
| Alimentazione case grandi | Assente | **0,015 Pane per abitante avanzato/s**, in aggiunta al Cibo comune |
| Crescita | Una persona ogni 60 secondi validi | Invariata; si arresta se manca Cibo, Pane richiesto o capacità abitativa |
| Perdite di popolazione | Nessuna | Invariate: una carenza sospende la crescita, non fa scomparire abitanti |

I numeri sono esatti al millesimo, come richiede il Core. `Pane` non è una variante testuale di `Cibo`: è una risorsa con identificatore e scorta indipendenti. Il forno non genera più Cibo base. In tal modo la filiera del pesce mantiene un ruolo anche nelle colonie grandi.

### Calcolo dei residenti e delle razioni

La distribuzione dei residenti è **derivata**, non salvata come occupazione di singoli edifici:

```text
posti_piccoli       = 8 × numero_case_piccole
posti_grandi        = 32 × numero_case_grandi
abitanti_avanzati   = max(0, abitanti_totali - posti_piccoli)
fabbisogno_cibo/s   = 0,010 × abitanti_totali
fabbisogno_pane/s   = 0,015 × abitanti_avanzati
capacità_abitativa  = posti_piccoli + posti_grandi
```

Prima si occupano virtualmente le case piccole e poi le grandi. Un giocatore può scegliere più case piccole, consumando più materiali per posto ma riducendo il fabbisogno di Pane. Costruire una casa piccola in seguito può ridurre il numero derivato di residenti avanzati senza spostamenti o decessi simulati. Non si introduce un tetto artificiale alle case piccole.

La razione di Cibo resta intera: se non c'è Cibo sufficiente per tutti, non viene consumata una frazione della razione collettiva e la crescita si ferma. Se il Cibo è disponibile, viene consumato anche quando manca il Pane: un'interruzione del forno non rende gratuito il mantenimento ordinario. Il Pane si consuma per gli abitanti avanzati soltanto quando la scorta copre la razione avanzata completa. Nessuna razione parziale, scorta negativa o decesso.

L'arrivo di una persona che dovrebbe occupare una casa grande richiede già una scorta sufficiente per la sua prima razione di Pane del secondo successivo; la persona nuova inizia comunque a consumare soltanto nel passo seguente. Le risorse prodotte durante un passo possono essere usate solo dal passo successivo, come ora. In pausa o a gioco chiuso non ci sono consumi né arrivi.

## Valutazione economica

La Casa grande offre quattro volte i posti di una Casa piccola, a costo doppio: **5 legno lavorato e 2,5 pietra per posto**, contro **10 e 5** della casa piccola. Richiede però l'investimento iniziale della filiera del forno (420 legno lavorato, 280 pietra, 5 lavoratori) e una fornitura continuativa di Pane.

Il confronto seguente riguarda **solo i costi edilizi per aggiungere pari capacità**, includendo una sola filiera completa del forno per la via avanzata, ma non i pescatori, le strutture comuni, i magazzini, i costi di attesa o il valore dei lavoratori:

| Case grandi | Posti aggiunti | Solo case piccole (legno / pietra) | Filiera pane + case grandi (legno / pietra) | Differenza a favore delle grandi |
| ---: | ---: | ---: | ---: | ---: |
| 1 | 32 | 320 / 160 | 580 / 360 | −260 / −200 |
| 2 | 64 | 640 / 320 | 740 / 440 | −100 / −120 |
| 3 | 96 | 960 / 480 | 900 / 520 | +60 / −40 |
| **4** | **128** | **1.280 / 640** | **1.060 / 600** | **+220 / +40** |
| 5 | 160 | 1.600 / 800 | 1.220 / 680 | +380 / +120 |
| 6 | 192 | 1.920 / 960 | 1.380 / 760 | +540 / +200 |

Il costo iniziale dell'intera filiera si ripaga in **entrambi i materiali dalla quarta Casa grande**, se un solo forno basta. È un punto di equilibrio dei *costi di costruzione*, non del tempo totale o dell'occupazione dei lavoratori. Oltre sei case, le capacità produttive del Pane vanno riesaminate.

### Capacità delle filiere e tempo di crescita

| Condizione | Fabbisogno | Capacità continua |
| --- | --- | --- |
| 100 abitanti totali | 1 Cibo/s | Una filiera pesce: 1 Cibo/s |
| 32 abitanti avanzati | 0,480 Pane/s | Una filiera pane: 3 Pane/s |
| 128 abitanti avanzati | 1,920 Pane/s | Una filiera pane ancora sufficiente |
| 192 abitanti avanzati | 2,880 Pane/s | Una filiera pane quasi al limite |
| 224 abitanti avanzati | 3,360 Pane/s | Serve più di una filiera equivalente, a regime |

Con 8 abitanti iniziali e quattro case grandi (+128 posti), la colonia può arrivare a 136 persone senza altre case. Per mantenerle occorrono **1,360 Cibo/s** e **1,920 Pane/s**. Una sola filiera pesce produce 1 Cibo/s: a regime occorre anche espandere la filiera di base o consumare scorte, mentre una filiera pane da 3/s è sufficiente. Due filiere pesce costano complessivamente 200 legno, 100 pietra e 4 lavoratori; una del pane costa 420, 280 e 5 lavoratori.

La crescita rimane al massimo **un abitante/minuto**: riempire una Casa grande richiede almeno 32 minuti di tempo simulato valido; occupare 128 posti nuovi almeno 128 minuti (2 h e 8 min). Questi sono limiti teorici, esclusi acquisti, carenze e pause. Non dimostrano il raggiungimento dell'obiettivo globale di otto ore, che include sistemi futuri.

### Rischi di bilanciamento da misurare

1. **Troppo pane in magazzino.** Una capacità iniziale di 100 Pane, rispetto ai 1.000 delle altre risorse, rende visibile la gestione della fornitura; un Magazzino porta la capacità a 600. A 128 residenti avanzati, 100 Pane durano circa 52 secondi senza nuova produzione; 600 durano circa 5 minuti e 12 secondi. Il forno può fermarsi a stock pieno senza bloccare la crescita finché Pane rimane disponibile.
2. **Scorte come ammortizzatore.** A 136 abitanti il Cibo richiesto supera la produzione di un solo pescatore di 0,360/s. Le grandi scorte di Cibo possono ritardare molto la necessità della seconda filiera: il bilanciamento va misurato anche a 8 e 32 ore, non solo sui flussi istantanei.
3. **Lavoratori della filiera.** L'investimento in pane impegna 5 persone. Il punto di equilibrio nei materiali non implica automaticamente una strategia più rapida: occorrono cronologie con espansione di segherie, cave e personale.
4. **Soglie tardive.** Una sola filiera pane copre 200 residenti avanzati; il limite si manifesta con la settima Casa grande da 32 posti, non con la prima. Se i test mostrano un forno sempre fermo e poca decisione economica, regolare *prima* la capacità Pane o la domanda, non cambiare simultaneamente più ricette.
5. **Strategia alternativa.** Case piccole aggiuntive evitano la razione di Pane ma costano più materiali per posto. Conservare questa alternativa offre una scelta anziché uno sblocco artificiale; va misurato quando diventa ottimale.

## Implementazione proposta dopo l'approvazione

1. Introdurre `civil-growth-4` con nuova risorsa `bread`, Casa grande, nuova ricetta del Forno e capacità di Pane; nessun cambiamento a Economy Foundation.
2. Estendere `PopulationRules` e il pianificatore civile a due fabbisogni, proiezioni separate e motivo di blocco Pane mancante. La suddivisione residenti resta derivata dal numero di case piccole e dalla popolazione, non da un nuovo contatore persistente.
3. Dichiarare **conversione esplicita** `civil-growth-3`→`-4`: nuova Casa grande a zero, Pane a zero; Cibo, scorte, persone, edifici e tempo invariati. La vecchia produzione di Cibo del Forno non può essere ricostruita per provenienza e non va retro-convertita in Pane. Conservarne una copia di sicurezza come nelle conversioni precedenti. Le versioni `-1` e `-2` transitano per i passaggi già esistenti.
4. Conservare il formato snapshot SQLite v2: `ResourceBundle` è sparso, una risorsa assente equivale già a zero e l'adattatore SQLite legge/scrive gli identificatori presenti. Questa **compatibilità è stata verificata per lettura del codice, non ancora con test eseguiti**; aggiungere un test di caricamento `-3`→`-4` con Pane assente prima di confermarla.
5. Aggiornare viste Edifici/Risorse, consumo Cibo e Pane, residenti di base/avanzati, carenze e schermate compatte. Evitare che il motore dipenda da Avalonia.
6. Testare conversioni, raggiungibilità del catalogo, contesa di input, limiti di capacità, crescita al confine tra le due categorie di posti, riserve di Pane a zero/piene, pausa, salvataggio intermedio, recupero da carenza e cronologie riproducibili fino a 32 ore.
7. Confrontare almeno tre strategie: sole case piccole, quattro case grandi, case grandi con investimento anticipato in secondo pescatore/magazzino. Misurare tempo per arrivare a 48/100/136 abitanti, legno e pietra spesi, durata dei blocchi, utilizzo dei lavoratori, scorte e tempi di ricostituzione.

## Decisioni da confermare

- **Proposta principale:** risorsa Pane distinta, Casa grande da 32 posti a costo 160/80 e consumo avanzato aggiuntivo 0,015 Pane/abitante/s.
- **Da verificare in simulazione prima del congelamento dei valori:** capacità iniziale Pane 100, economicità dalla quarta casa e tempo di sblocco della seconda filiera pane.
- **Ancora aperto e non deciso qui:** vittoria globale (Q-01), limiti eventuali alle case piccole, demolizione (Q-08), bisogni alimentari dei soldati e bilanciamento complessivo delle 8/32 ore. Il giocatore resta libero di costruire molti edifici dello stesso tipo.

**Decisione ricevuta:** l'utente ha approvato i valori e la risorsa Pane distinta. Il codice di `civil-growth-4` li adotta; non promuovere tuttavia il gate a PASS senza risultati .NET e verifica visuale. In particolare il forno produce blocchi da 3 e con capacità iniziale 100 può arrivare a 99, non 100, in assenza di consumo.
