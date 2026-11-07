# ADR 0104 — Checkpoint composito della campagna v15 e versamenti Curve A (RC94)

**Data:** 07/11/2026  
**Stato:** ACCETTATA per il solo esperimento; NON autorizza l'attivazione nel gameplay.

## Contesto

RC92 prevede un escrow progressivo con preventivo Curve A congelato. RC93 lo rende transazionale e idempotente in un database SQLite autonomo, ma lo stock di quel database non è l'inventario autorevole della città. Collegare direttamente un completamento RC93 al comando v15 `BuildCityProducer()` causerebbe un secondo addebito, perché il comando corrente scala sempre il prezzo intero. Inoltre il salvataggio v15 `user_version=6` non contiene cantieri né importi depositati.

## Decisione

Introdurre `E4bV15FundingShadow`, un **checkpoint composito immutable e non giocabile** che associa:

- il **checkpoint v15 completo** (città, ricerca, personale, industria alimentare, depositorie);
- l'**escrow Curve A** e il preventivo per il tier scelto;
- il **contatore storico acquisizioni della famiglia `mint`** e la capacità ricavata dai veri edifici della città;
- una **revisione monotona** delle operazioni in memoria.

`Open` valida il checkpoint completo; `TryBeginMint` congela il preventivo; `TryDeposit` sottrae gli stessi materiali dall'inventario della copia di città e li accredita nell'escrow; `TryCancel` ripristina integralmente l'inventario se il modello lo consente. Ogni passo accettato è validato con `V15FoodCampaignCheckpoints.Validate()` e con riconciliazione di capacità, contatore, stock ed escrow. `Restore` rifiuta incoerenze o consegne ipotetiche/edifici completati provenienti dal sandbox indipendente.

Una nuova `Readiness()` verifica presenza del pagamento, prerequisiti edifici e ricerche di livello. Anche quando restituisce `ReadyForFutureAtomicCommit` **non costruisce nulla**: prima del collegamento al gameplay serve un'unica transazione di commit del salvataggio/autorizzazione senza doppio addebito.

## Non obiettivi

- Nessuna modifica a `V15FoodIndustryCampaignSession`, prezzi live, GUI, SQLite v15, clock, lavoratori, costruzioni o capienza.
- Non è possibile importare come salvataggio v15 il database separato RC93.
- Non si possono rifornire artificialmente i materiali della shadow; i test di versamento usano l'inventario iniziale reale. Non sono ancora supportati i cambiamenti di inventario causati dal clock durante un finanziamento.
- Non è una dimostrazione di progressione naturale e non autorizza la Curva A.

## Prove e criteri

12 xUnit Application per apertura, quota congelata, pagamento sul medesimo stock, restore, storno, contatore, revisioni obsolete, abuso delle consegne ipotetiche, violazione di inventario/capienza/contatore e readiness. CLI `--study-e4b-v15-shadow` + gate F1 verificano su un checkpoint v15 nuovo che versamento e storno si riconcilino e che la città originale non cambi.

## Passo successivo

RC95: decidere formato/versione del checkpoint giocabile con escrow, sincronizzazione con produzione naturale, verifica di idempotenza e transazione unica fra versamenti e costruzione completata; l'operazione di completamento deve sostituire il prezzo atomico, non affiancarlo. Validare integralmente prima dell'attivazione del regime sperimentale.
