# SimRisorse — G1-D5 / E1: primo incremento tecnico isolato

**3 novembre 2026 — candidato v60-E1-RC1.** Questo incremento **non attiva costi crescenti né ricerca nella campagna** `civil-growth-13`, già validata dall'utente nella v59. È codice reale nel Core e test inediti, non soltanto progettazione; manca il gate Windows sul nuovo codice.

## Decisione vincolante emersa nella discussione

L'utente ha approvato **accumulo libero di punti ricerca e acquisti volontari delle tecnologie**. Vedere [ADR 0021](../adr/0021-ricerca-saldo-libero-spesa-su-scelta.md). I coefficienti di costo e i valori di Scuola restano proposte.

## A. Prezzi progressivi puri

- `ProgressiveConstructionPricing.QuoteNewBuildings`: costo `ceil(B_r × tier × growth^(K+i))` per ogni materiale r e per ogni unità i, sommato in modo deterministico. Il numero K di acquisizioni è sempre relativo alla famiglia, non al tier. Il risultato è in unità intere, coerente con le risorse a millesimi del Core.
- `QuoteUpgrade`: differenza fra il prezzo corrente della nuova taglia e quello della vecchia, al **medesimo K**, senza rimborso implicito di scorte e senza incremento del contatore.
- `ConstructionAcquisitionHistory`: contatore monotono e immutabile per famiglia, inizializzabile dagli esemplari posseduti; non si decrementa con upgrade o futura demolizione. `RegisterPurchase` restituisce un nuovo valore.
- `ProgressiveConstructionPurchase.TryPurchase`: quotazione, verifica fondi e restituzione *congiunta* di nuovo saldo materiali e nuovo storico (nessuna mutazione parziale quando i materiali sono insufficienti). Non gestisce ancora prerequisiti, alloggi, capacità di stoccaggio o sblocco dei tier: il futuro aggregato della partita dovrà verificare tutto prima di impegnare i due valori.
- Coefficienti **parametrizzati**; nessun 15% o tier 2,6/6,5 codificato come prezzo operativo. Si utilizzano soltanto nei test esemplificativi.
- Aritmetica decimale e `checked`; quotazioni eccessive rifiutate per overflow, non trasformate in costi nulli.

## B. Saldo libero della ricerca

- `ResearchPoints`: valore immutabile non negativo con precisione di 0,001 punti, separato da `ResourceBundle`.
- `ResearchAccounting.Accrue`: accumula punti senza progetto selezionato in base ai ricercatori attivi, ai secondi di simulazione e a una tariffa passata dall'esterno, ancora non approvata.
- `ResearchAccounting.TrySpend`: se il saldo è insufficiente lo restituisce identico; altrimenti calcola il residuo. Non sblocca ancora tecnologie: prerequisiti, acquisto e persistenza restano E3.

## C. Validazione

Nuovo file test `ProgressiveConstructionPricingTests.cs`: prima/seconda Segheria, acquisti da cinque, indipendenza fra famiglie, upgrade, prezzi +1 e +5 coerenti, acquisti senza saldo, rigetto input errati, overflow. `ResearchAccountingTests.cs`: accumulo libero, spesa facoltativa, saldo insufficiente, inattività e overflow.

**Test locali .NET 10:** NON eseguiti (SDK non presente nell'ambiente di preparazione). **Gate Windows:** pendente. Controlli statici e esempi con aritmetica decimale eseguiti, ma non sostituiscono compilazione e `validate.cmd`.

## D. Quali file della campagna restano identici

`GameSession`, `GameSnapshot`, `GameScenario`, factory dei cataloghi, `ProductionPlanner`, UI Avalonia, conversioni SQLite, `scripts/validate.ps1` e benchmark hanno lo stesso contenuto della base v59-G1-D4. Non è stato introdotto `civil-growth-14` e non sono state modificate le 16 risorse o gli edifici.

## E. Avanzamento pianificato

1. Validare E1-Core su Windows quando possibile; non sostituire la v59 operativa senza il gate.
2. Definire e testare invarianti degli edifici per tier e dell'assegnazione dei lavoratori **senza** abilitare prezzi più alti nella campagna.
3. Progettare e implementare Scuola, ricercatori, tecnologie e snapshot/migrazioni SQLite in una versione distinta.
4. Solo con E1+E2+E3 coerenti introdurre il ruleset futuro `civil-growth-14` e l'interfaccia; poi bilanciamento E4 con i benchmark reali.
5. Navale M5.4 subordinato al nuovo equilibrio economico e al gate delle regole navali.

## Criterio fondamentale anti-softlock

La versione futura non può imporre un prezzo superiore alla capacità massima raggiungibile prima di avere tecnologia/logistica sufficiente. Un test sulla sola formula non dimostra la raggiungibilità dell'intera campagna: serve benchmark dal Municipio iniziale.
