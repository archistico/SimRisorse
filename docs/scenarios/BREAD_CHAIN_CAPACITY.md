# M4 — Capacità nominale dell'intera filiera del Pane

Data: 2026-10-27. **Stato: proiezione informativa implementata; nuovo gate .NET in attesa.** Non modifica ricette, costruzioni, consumo, crescita, salvataggi o regole `civil-growth-4`.

## Perché la capacità del Forno non basta

Due Forni con due addetti possono produrre al massimo **6 Pane/s**, ma soltanto se la colonia procura abbastanza Farina e Acqua. Una seconda struttura non sostituisce Mulino, Pozzo e Fattorie. Il modello `GameView.BreadSupply.ChainCapacity` quantifica separatamente gli anelli, usando le ricette esistenti e il personale assegnato.

Per **220 abitanti**, una Casa piccola fornisce 8 posti di base: 212 residenti sono avanzati e richiedono `212 × 0,015 = 3,180 Pane/s`. Per ogni unità di Pane, servono `1/3` Farina, `1/3` Acqua e `2/3` Grano, perché le ricette sono `2 Grano → 1 Farina` e `1 Farina + 1 Acqua → 3 Pane`.

| Risorsa | Fabbisogno medio per 3,180 Pane/s | Produzione con una filiera | Addetti minimi teorici | Addetti in una filiera |
| --- | ---: | ---: | ---: | ---: |
| Grano (Fattoria) | 2,120/s | 2/s | 3 | 2 |
| Acqua (Pozzo) | 1,060/s | 1/s | 2 | 1 |
| Farina (Mulino) | 1,060/s | 1/s | 2 | 1 |
| Pane (Forno) | 3,180/s | 3/s | 2 | 1 |

Il proiettore calcola automaticamente la domanda a ritroso dalle ricette, senza usare identificatori fissi di Fattoria, Mulino o Pozzo. Funziona soltanto quando ogni risorsa attraversata ha **un unico produttore e una ricetta a uscita singola, senza cicli**. In presenza di alternative o coprodotti non restituisce una stima, perché servirebbe un modello di allocazione più articolato.

## Come interpretare la capacità

Ogni anello fornisce un equivalente nominale di Pane al secondo. Esempio: 2 Fattorie producono 2 Grano/s; con le conversioni approvate, questo equivale a `2 × (1/2) × 3 = 3 Pane/s`. L'**equivalente minimo** fra Grano, Acqua, Farina e Pane è un **limite teorico superiore alla media produttiva sostenibile**. Non è il forecast del prossimo secondo.

- `NominalProductionPerSecond`: capacità del solo Forno con addetti assegnati.
- `NextSecondProductionPerSecond`: previsione reale per il prossimo secondo, tenendo conto di input disponibili, lotti, contesa e spazio.
- `ChainCapacity.BreadEquivalentCapacityPerSecond`: limite superiore medio derivato dai lavoratori dell'intera filiera, ignorando temporaneamente scorte, deposito e consumi concorrenti.
- `ChainCapacity.LimitingResourceIds`: anelli che definiscono tale limite; ogni `Stage` espone domanda, produzione nominale, deficit e addetti teorici mancanti.

Se un Mulino lavora a intermittenza perché una sola Fattoria produce 1 Grano/s ma ogni ricetta ne consuma 2, l'equivalente medio teorico è **1,5 Pane/s**; non va arrotondato a zero. Il consumo reale rimane discreto, secondo le regole del Core. Per questo la proiezione non promette che l'intera media sarà sempre raggiunta.

## Due investimenti candidati oltre 200 avanzati

| Potenziamento rispetto a una filiera completa | Fattorie totali | Pozzi | Mulini | Forni | Lavoratori aggiuntivi | Legno lavorato aggiuntivo | Pietra aggiuntiva | Limite teorico Pane/s |
| --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| Nessuno | 2 | 1 | 1 | 1 | 0 | 0 | 0 | 3 |
| **Filiera snella** | 3 | 2 | 2 | 2 | **4** | **360** | **250** | **4,5** |
| Seconda filiera completa | 4 | 2 | 2 | 2 | 5 | 420 | 280 | 6 |

La filiera snella risparmierebbe **1 lavoratore, 60 legno lavorato e 30 pietra** rispetto al raddoppio integrale. Il vantaggio nei costi è aritmetico, **ma il rendimento non è deducibile senza il JSON**: il Mulino richiede lotti da 2 Grano, le riserve possono esaurirsi, la capacità di deposito può fermare il Forno e la popolazione continua a crescere. Il confronto M4.7 fra quattro politiche è stato **riferito superato dall'utente** nel gate v17; i dati grezzi restano da acquisire. Un [profilo M4.8 da 320 abitanti](BREAD_HIGH_DENSITY_BENCHMARK.md) è implementato per verificare la soglia di 4,5 Pane/s, ancora senza esito .NET. Non si cambiano i costi o le ricette prima di osservarne gli esiti.

## Integrazione con la GUI 8-bit

La GUI potrà mostrare quattro indicatori compatti (Grano, Acqua, Farina, Pane) con disponibilità nominale, fabbisogno e stato. Non deve ricalcolare queste quantità: i dati sono nel modello Application. Non presentare mai l'equivalente medio della filiera come produzione garantita o scorta già disponibile. Il colore è accompagnato da numero ed etichetta leggibile.

## Verifiche del nuovo incremento

- Nessun residente avanzato: proiezione della catena assente, con eventuale avviso separato per il primo arrivo.
- 220 abitanti, una filiera completa: capacità nominale di tutta la catena **3 Pane/s**, domanda **3,180/s**.
- Due Forni, ma un solo gruppo di produttori intermedi: capacità Forni **6 Pane/s**, catena ancora **3 Pane/s**.
- Una Fattoria soltanto, con due Forni assegnati: limite teorico **1,5 Pane/s**, Grano come unico anello minimo.
- Due filiere complete, ingredienti iniziali assenti: limite teorico **6 Pane/s**, ma produzione nel prossimo secondo **zero**. La diagnosi di input mancanti deve rimanere distinta.

I test Application controllano i dati; non possono sostituire una simulazione temporale della strategia snella. Nuovo gate Windows **non ancora eseguito**.
