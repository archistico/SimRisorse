# RC78 / E4b.1 — Studio isolato di tre curve economiche

Stato: strumento di confronto implementato e verificato; **coefficienti proposti, non approvati né applicati**. Segue le misure naturali [RC77-M01](../validation/E4A_V15_RC77_M01_NATURAL_MEASUREMENTS.md) e la [specifica P00](E4B_P00_COSTI_PROGRESSIVI_E_SQUADRE_MINIME.md).

## Parametri dichiarati

| Curva | Indice della famiglia | Base elevata all'indice | Rincaro copie | Grande | Avanzato |
| --- | --- | ---: | ---: | ---: | ---: |
| A | Epoca produttiva | 2 | 1,08 | 2,6 | 6,5 |
| B | Profondità massima dei prerequisiti | 1,8 | 1,10 | 2,6 | 6,5 |
| C | Epoca produttiva, investimento aggiuntivo in metalli | 2,7 | 1,15 | 3 | 10 |

Per A/C: epoca 0 avvio; 1 Mulino, Panificio, Casa grande, Scuola e siti alimentari; 2 miniere industriali; 3 fonderie; 4 Forgia/Zecca. B ricava la profondità dal grafo corrente, Zecca = 6. La Segheria ha profondità 1 ma epoca 0: le alternative non sono semplici moltiplicazioni dello stesso vettore finale.

B aggiunge 40 Ferro/20 Oro alla Zecca e 40 Ferro alla Forgia; C aggiunge rispettivamente 120 Ferro/80 Oro e 80 Ferro. I metalli aggiunti partecipano ai moltiplicatori di tier/copie. Nessun materiale richiede la stessa Zecca per essere prodotto.

Il vettore d'ingresso è `ceil(costo corrente × base^indice)` per materiale, poi si aggiungono gli eventuali metalli. Il prezzo è `ceil(ingresso × tier × rincaro^storico)`, per edificio e risorsa. Questa doppia fase di arrotondamento è esplicita: non si usano frazioni di un materiale edilizio. Il motore `ProgressiveConstructionPricing` somma i prezzi delle copie per i lotti e calcola gli upgrade per differenza al medesimo storico. Gli acquisti incrementano lo storico; gli upgrade non lo incrementano e non restituiscono risorse.

## Confine dell'esperimento

`E4bEconomicCurveStudy` vive in Application/Benchmarks. Usa cataloghi e ricette correnti come riferimenti immutabili; non è richiamato da sessioni, clock, GUI o persistenza. La CLI è `--study-e4b-curves [--baseline FILE_O_CARTELLA] [--output CARTELLA_NUOVA]`. Le opzioni duplicate/sconosciute e gli output preesistenti sono rifiutati. L'input opzionale viene analizzato dal validatore E4a, deve contenere i sei abbinamenti I01/I02/I03 × due politiche, con obiettivi osservati e orizzonti integrali. Si registrano percorso e SHA-256; senza input, la baseline resta nulla. Non si simulano tempi delle proposte.

Il report contiene tre definizioni, **81 righe preventivo** (9 famiglie × 3 tier × 3 curve), **18 investimenti** (1/2 Zecche × 3 tier × 3 curve) e tre confronti di upgrade. Ogni preventivo espone prima/seconda copia e lotti 5/10, con audit di liquidità dei lotti. La griglia parte dallo storico zero della famiglia per confrontare prezzi di riferimento; gli investimenti partono invece dai contatori degli edifici già presenti nella partita iniziale. I dati sono esportati in JSON, due CSV e Markdown; un quinto file conserva la provenienza della baseline.

## Filiera, ricerca e capacità

La chiusura dei prerequisiti e dei produttori di materiali aggiunti deduplica gli edifici condivisi, sottrae quelli posseduti inizialmente e rileva cicli. Include Scuola, alimentazione, case, miniere/fonderie e supporto dimensionato sul personale. B/C includono anche il ramo Ferro, temporaneamente necessario ai pagamenti. Il budget a regime non mantiene occupati questi addetti dopo la costruzione. Tutti gli edifici di supporto sono Base; la Zecca usa il tier richiesto.

Si confrontano materiali edilizi lordi per risorsa, senza fingere un prezzo in moneta comune. I secondi-lavoratore nominali necessari ai materiali sono ricavati ricorsivamente dalle ricette, input compresi. Il riferimento Tagliapietre resta **30 tavole + 10 pietre**, senza rincaro di cento acquisti ipotetici; nelle ricette attuali equivale a 70 secondi-lavoratore. Rapporti tavole/30 e pietra/10 restano distinti dal rapporto di lavoro aggregato.

Il lavoro edilizio esclude Magazzini, formazione, consumo continuo di alimenti/ricette e scorte iniziali; non è tempo di partita. Il rapporto comparativo riporta separatamente il lavoro aggiunto dai Magazzini. La ricerca resta 350 punti Base/Grande, 800 Avanzato: 3500/8000 secondi-lavoratore teorici con un ricercatore a 0,1 punti/s, esclusa formazione. Gli attuali 3540s di attesa osservata non vengono sostituiti da queste cifre teoriche.

L'audit costruisce Magazzini Base uno alla volta, con rincaro e pagamento verificato contro la capacità già disponibile, prima di aggiungere 500 posti per risorsa. Cerca la capacità per ogni pagamento singolo, non per la somma dell'intera filiera. Per i lotti controlla invece l'intera somma atomica. Un percorso bloccato restituisce motivo, capacità, costo del Magazzino irraggiungibile e spesa parziale; il totale finanziabile resta nullo, mai zero.

Si assumono materiali rinnovabili e tempo illimitato; si ignorano spazio accessorio dei produttori, upgrade dei Magazzini e percorsi alternativi. L'esito non dimostra raggiungibilità nel clock né uno stallo globale. Gli upgrade della Zecca sono un confronto separato, senza confonderne i pagamenti con l'acquisto diretto.

## Squadre: solo proiezione numerica

Principio approvato: 10 persone per unità produttiva Zecca. Ipotesi ancora candidata: 1/3/10 moduli per Base/Grande/Avanzato, quindi piena occupazione 10/30/100. Si verificano moduli interi, personale riservato, input e spazio congiunti, consumo nullo per moduli bloccati. Il clock attuale continua a usare la regola precedente.

Per ogni modulo il supporto industriale è 1 addetto Oro grezzo, 1 Fonderia Oro, 1 Carbone; quest'ultimo copre due consumi da 1/s. Si aggiungono tre addetti iniziali e un ricercatore. Un calcolo iterativo dimensiona Pescatori/Deposito e filiera Pane per alimentare anche il personale aggiunto, con due Case piccole e Case grandi sufficienti. È un budget a regime senza personale libero di riserva, popolazione fissa e nessuna promessa di crescita o avvio sostenibile. Concorrenza reale, save/load, migrazione e GUI delle squadre appartengono all'implementazione futura.

[Risultati e raccomandazione](../validation/E4B_RC78_CURVE_COMPARISON.md) · [Verifiche](../validation/E4B_RC78_TEST_PLAN.md) · [ADR 0086](../adr/0086-confronto-curve-economiche-isolato.md).
