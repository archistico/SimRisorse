# F1-D01 / RC40 — Modello delle filiere alimentari

Data: 2026-11-04. Stato: **modello approvato, nucleo Core isolato implementato; integrazione nel gameplay NON ancora attiva**.

## Motivazione e differenze dalla RC39

Nella RC39 il Pescatore produce `fish`, il `food-store` converte soltanto `fish` in `food` (1:1); Fattoria → Grano, Pozzo → Acqua, Mulino → Farina e Forno → `bread` sono già presenti. La popolazione consuma `food` da un inventario cittadino condiviso e gli abitanti avanzati consumano anche `bread`. La capacità alimentare non è ancora esclusiva dei depositi: Municipio, Magazzino ed edifici contribuiscono alle scorte comuni. Ogni `BuildingDefinition` ammette soltanto una `ProductionRecipe`.

**Nuovo contratto:** tutti i prodotti finali alimentari (Pesce, Pane, Carne, Frutta e Verdura) sono ricevuti **come scorte alimentari grezze** dal deposito. Solo l'output di lavorazione del deposito, `Cibo pronto`, alimenta il fabbisogno ordinario. Il Pane continua ad alimentare anche una razione **distinta e riservata** per le abitazioni avanzate. La quantità convertita in Cibo e quella riservata non possono coincidere. Bovini e Selvaggina sono intermedi produttivi per il Macellaio, non Cibo pronto.

## Filiere concordate

| Percorso | Edifici | Input produttivi | Output | Ruolo economico |
| --- | --- | --- | --- | --- |
| Pesca | Pescatore | Nessuno per ciclo | Pesce | Breve, costo di avvio contenuto |
| Pane | Fattoria + Pozzo + Mulino + Forno | Grano → Farina; Farina + Acqua | Pane | Filiera lunga, requisito per abitazioni avanzate |
| Allevamento | Fattoria + Pozzo → Stalla → Macellaio | Grano + Acqua → Bovini → Carne | Carne | Più costosa, resa potenzialmente maggiore a regime |
| Caccia | Cacciatore → Macellaio | Nessun input agricolo, Selvaggina lavorata | Carne | Alternativa semplice, portata produttiva inferiore |
| Orto | Orto | Da definire: input di lavorazione ed eventuale Acqua | Verdura | Filiera corta, avvio rapido |
| Frutta | Frutteto | Da definire: input e maturazione | Frutta | Filiera corta/complementare, crescita diversa dall'Orto |
| Deposito | Deposito alimentare | Pesce / Pane eccedente / Carne / Frutta / Verdura | Cibo pronto | Capacità e lavoratori, ricette multiple |

Le due origini di Carne **restano distinguibili** (`cattle` / Bovini e `game` / Selvaggina). Il Macellaio necessita di almeno due ricette alternative, con scelta automatica e leggibile: niente combobox nel dettaglio edificio. Nessuna resa o durata delle nuove filiere è ancora approvata.

## Contratto di stoccaggio, trasformazione e consumi

1. **Unica titolarità dei prodotti finali:** l'inventario `FoodDepotStock.RawFood` conserva le cinque materie alimentari in ingresso. Non si mantiene una seconda copia spendibile nell'inventario generale. L'accettazione di una consegna deve essere atomica e vincolata allo spazio per materia prima.
2. **Cibo preparato distinto:** `FoodDepotStock.ReadyFood` rappresenta esclusivamente la razione ordinaria già lavorata; un deposito senza lavoratori non può convertirla, anche se i produttori agricoli lavorano.
3. **Capacità di processo:** i lavoratori e il tier dei Depositi forniscono un numero massimo di lotti *interi* per tick; le ricette sono ordinate con priorità esplicita e spareggio alfabetico (`Ordinal`). Nessuna conversione parziale o produzione se il Cibo pronto è pieno.
4. **Riserva Pane:** il chiamante comunica `minimumBreadReserve` calcolata dai residenti avanzati e da una scorta di sicurezza temporale configurabile; solo le unità di Pane superiori alla riserva sono convertibili. Il successivo consumo speciale attinge alla quota rimasta nel deposito, mai al Cibo già prodotto con le stesse unità.
5. **Popolazione:** Cibo pronto soddisfa la razione ordinaria, Pane riservato quella aggiuntiva per residenti avanzati; il controllo di crescita richiede entrambe e non può operare sulla stessa unità due volte.
6. **Primi minuti:** le 30 unità di Cibo iniziali della v14 saranno collocate logicamente nel piccolo stoccaggio alimentare iniziale del Municipio, già disponibili come Cibo pronto. La presenza di queste scorte NON autorizza produzione di nuovo Cibo senza il Deposito e il relativo personale.
7. **Nessuna perdita da migrazione:** scorte storiche di Pesce/Pane/Cibo verranno trasferite una sola volta al nuovo modello; se le capacità risultano inizialmente inferiori, conservare la sovracapacità esistente senza produrre altro finché non diminuisce. Mai scartare o duplicare risorse.
8. **Ricette e numeri:** quantità e coefficienti del cibo vengono definiti con `FoodConversionRecipe` configurabili; le ricette 1:1 usate nei test di F1-D01 sono **fixture tecniche**, non valori di bilanciamento approvati.

## Codice prodotto in F1-D01

`src/SimRisorse.Core/Food/FoodDepotModel.cs` introduce:

- `FoodConversionRecipe`, `FoodConversionPolicy`: contratti con validazione di input/output positivi, fonti distinte e `food` vietato come input.
- `FoodDepotStock`: riserva di alimenti grezzi e Cibo pronto separate dalle scorte cittadine.
- `FoodDepotPlanner.Receive`: consegna totale o nulla nel limite della capacità materia prima.
- `FoodDepotPlanner.Plan`: calcolo **puro** e deterministico del tick, rispettando input reali, riserva Pane, lotti possibili e capacità Cibo pronto.
- `FoodDepotProcessingResult`: consumi e produzione registrati insieme, utile per audit, salvataggi e diagnostica.

**Non integrato:** né `CivilGrowthScenario`, né `V14CampaignSession`, `PopulationPlanner`, GUI, v13, SQLite, tier, catalogo tecnologico o benchmark attuali richiamano ancora questi nuovi tipi. `civil-growth-13` e gli identificativi dei salvataggi restano intatti. Non aggiungere nuove righe Risorse prima del passo F1-D02/D03 con versione di regole e migrazione esplicita.

## Dipendenze tecniche prima dell'attivazione

1. Il modello `BuildingDefinition.Recipe` e `TieredProductionFamily` presuppongono una ricetta unica; per Macellaio e Deposito servono un catalogo di ricette alternative e una policy deterministica di selezione (la prova pura F1-D01 ne stabilisce già il principio).
2. Lo storage attuale è per risorsa e condiviso; il dominio `FoodDepotStock` richiede l'integrazione di capacità per ingredienti e cibo pronto, senza sommare inventario cittadino e deposito.
3. `PopulationPlanner` controlla i livelli `food`/`bread` nel bundle generale: deve leggere il nuovo deposito dopo **il passaggio atomico della produzione**, senza cambiare ordine dei tick accidentalmente.
4. La persistenza SQLite v14 richiede una versione di regole/schema **nuova**, migrazione una tantum degli stock con backup e regressioni su salvataggi storici. Non riutilizzare il vecchio `rulesVersion` per significati diversi.
5. Le 16 famiglie tier e le tecnologie `Alimentazione I/II` sono mappate staticamente: aggiungere le nuove filiere preservando i vantaggi reali delle ricerche, senza inventare nuove tecnologie acquistabili.
6. GUI: nuove righe Risorse in ordine alfabetico, stessa tabella a sette colonne; il dettaglio del deposito deve mostrare grezzi, quota Pane protetta, Cibo pronto, capacità e produzione effettiva con gli stessi stili v14.

## Rischi, alternative, limiti di F1-D01

- La priorità rigida delle ricette può affamare una filiera meno prioritaria: osservare negli scenari E4a ed eventualmente introdurre una politica equa senza alterare il determinismo.
- L'allevamento compete con Forno e Scuola (attraverso la domanda di Cibo) per risorse e forza lavoro; confrontare **Cibo netto per lavoratore e per risorsa di base**, non solo Bovini prodotti.
- Una quota Pane troppo bassa può far oscillare la crescita: riserva minima e orizzonte protettivo devono essere misurati prima di E4b.
- La Caccia non deve aggirare la capacità del Macellaio: il suo prodotto è Selvaggina, non Carne pronta.
- Le vecchie misurazioni E4a sono confronti *storici* del modello precedente, non baseline direttamente equiparabile alla nuova economia.

## Criteri di uscita

F1-D01: API pure + 7 test aggiunti + documento e ADR, senza cambiare le partite. F1-D02/D03/D04/D05 seguono la roadmap ufficiale. La chiusura dell'intera riforma F1 richiederà salvataggi storici preservati, suite Windows verde, verifica end-to-end delle filiere e approvazione della nuova economia prima di E4b.
