# RC51 — F1-D05d.3b: orologio unico del ramo alimentare v15

**5 novembre 2026 · Implementazione candidata, senza compilazione .NET 10 in questo ambiente.**

## Obiettivo e confine

`V15UnifiedCampaignClock.Plan` e `V15FoodIndustryCampaignSession.Advance` gestiscono un solo tick, con commit finale atomico, della campagna headless v15. La vecchia GUI v14 e `V14CampaignClock` non sono cambiati. La vecchia ricetta `food-store: fish → food` è **disabilitata esclusivamente nel clock v15** tramite il parametro facoltativo `suppressedProducerIds` di `ProductionPlanner.Plan`. Gli altri produttori della vecchia città mantengono le ricette già valide: Pescatore, Fattoria, Pozzo, Mulino, Forno, risorse industriali, fabbricazione spade e monete.

Il nuovo `food-depot` è l'**unico produttore v15 di Cibo**, conserva Pesce/Pane/Carne/Frutta/Verdura come materie prime e il Cibo pronto in inventario dedicato. Il `food-store` rimane nel checkpoint storico e, se dotato di addetti, questi **occupano ancora popolazione ma non producono** nel ramo v15: è una temporanea passività che deve essere rimossa nella sostituzione definitiva del catalogo/UI, non una struttura da acquistare.

## Ordine deterministico di ogni secondo intero

1. Produzione legacy in ordine di priorità del catalogo, omettendo solo `food-store`. Stock di Matite preesistenti preservato dal planner storico.
2. Stalla, Cacciatore, Macellaio: la macellazione usa soltanto animali disponibili a inizio secondo; Grano e Acqua sono consumati una volta sola.
3. Orto e Frutteto con l'Acqua restante; scorte intermedie persistenti anche quando il deposito è pieno.
4. Consegna parziale nei cinque compartimenti di materia prima del Deposito; lavoratori del Deposito trasformano lotti interi in Cibo, senza superare la capacità di Cibo pronto.
5. Razione di Cibo per *tutti* gli abitanti, più razione separata di Pane per quelli avanzati. **Pasto atomico:** se manca uno dei due non viene consumato nulla; protezione del Pane necessario al possibile prossimo residente avanzato.
6. Crescita degli abitanti in base a case, disponibilità del pasto e Pane; progressione continua con remainder a ventesimi secondo la vecchia formula, ma senza la vecchia deduzione dalla città.
7. Avanzamento individuale esercito; avanzamento Scuola e punti ricerca nello stesso secondo; l'arruolamento e l'iscrizione non sono qui ri-implementati (restano future azioni del ruleset v15).
8. Validazione integrale del checkpoint v15; **solo dopo** aggiornamento della sessione. In caso di eccezione nessuna modifica a snapshot, deposito, intermedi o tempo.

## Tempo e persistenza

- Il tempo è `V14CampaignSnapshot.ElapsedTicks` nella busta v15, preservato al tick esatto. Gli intervalli sotto il secondo si accumulano e non producono prima del completamento del secondo.
- Ogni nuova sessione, anche ripristinata da SQLite, è inizialmente in pausa. `Resume`, `Pause`, `Advance` condividono la semantica della v14.
- Un anticipo massimo di 24 ore è accettato dalla API; negativa o superiore restituisce un `GameError` senza mutazione. Il planning puro non modifica file, DB o UI.
- Un salvataggio SQLite generation 5 contiene già tutte le quantità e i progressi necessari; **nessuna modifica allo schema** in RC51. `Save → Load → Advance` deve produrre lo stesso checkpoint di un anticipo continuo.
- `StepFood` rimane come API **esplicita isolata per diagnostica e test**, non deve essere chiamata nel loop della GUI insieme a `Advance`: introdurrebbe un secondo tick alimentare senza avanzare il tempo.

## Limiti ancora aperti

Il catalogo storico ereditato dalla v14 contiene ancora `food-store` e le tecnologie storiche che lo citano, ma **non** ha un secondo producer attivo nel clock v15. La sessione v15 non espone ancora i comandi completi su edifici/ricercatori/militari storici. Per il ruleset v15 definitivo bisognerà ricostruire il catalogo senza residui, adattare iscrizioni Scuola (consumo dal nuovo Deposito), rimborsi e i controlli GUI; ciò è diverso dalla sola esecuzione automatica del progresso della Scuola, già presente. Nessuna modifica ai costi E4b o alla Zecca. Nessuna compatibilità con vecchi database sperimentali richiesta.

## Verifiche

16 nuovi `[Fact]`: 14 Application + 2 Infrastructure. Coprono disattivazione `food-store`, input storici, accumulo secondi parziali, bilancio pasti, Pane avanzato, crescita, Scuola, addestramento individuale, atomicità di fallimento, save-load-continue e ricostruzione intermedi. **Tutti da eseguire su Windows con SDK .NET 10**, cumulativamente 683 test indicativi. Gli assert di nuove RC potrebbero richiedere correzioni dopo l'esecuzione; non dichiararli confermati.

[ADR 0062](../adr/0062-clock-unico-v15-e-produttore-cibo-esclusivo.md) · [Piano test](../validation/F1_D05D3B_RC51_TEST_PLAN.md) · [Stato](../CURRENT_STATE.md).
