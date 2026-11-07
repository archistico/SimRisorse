# RC79 / E4b.2a — Liquidity e finanziamento delle costruzioni

## Problema e obiettivo

RC78 ha rilevato che l'acquisto atomico di 10 Zecche Base **non entra nel percorso di Magazzini dichiarato**, neppure per A; per B/C emergono limiti anche prima. La soluzione non può essere abbassare di nascosto i prezzi quasi esponenziali. Occorre confrontare strategie di pagamento senza cambiare l'economia giocata fino alla decisione dei coefficienti.

## Contratto del prototipo

`SimRisorse.Core.Construction.ProgressiveConstructionEscrow`:

- `Start(quote)` accetta solo una quotazione non nulla e non vuota.
- `TryDeposit(stock, payment, out escrow, out stockResiduo)` verifica **insieme** disponibilità cittadina e importo ancora dovuto. Rigetto invariato, successo con stock ridotto e pagamento accreditato una sola volta.
- `Outstanding = Quote - Paid`; `IsFullyPaid` non eroga edifici o capacità.
- `TryRefund(stock, capacity, out stockRimborsato)` restituisce gli importi versati solo se tutta la restituzione entra nello stock. Il prototipo non cancella la pratica né scrive in un DB.
- `MinimumInstallments(quote, capacity)` calcola `max_i ceil(quote_i / capacity_i)` sui materiali richiesti. Capacità zero per un materiale richiesto → impossibile nel vettore scelto.
- `Installment(quote, n, index)` divide esattamente le quantità per risorsa in millesimi, con resto assegnato alle prime rate; non crea una lista di rate e non arrotonda il preventivo.

**Invarianti di valore:** il vettore pagato + outstanding = quote; stock prima = stock dopo + deposito; somma rate = quote; una domanda respinta non muta stato. La quotazione totale deve essere congelata durante la pratica prima di una eventuale integrazione giocabile; qui è l'input di una funzione pura.

## Studio e perimetro

`E4bFinancingFeasibilityStudy` riusa **tutte e tre** le curve RC78, il loro motore di prezzo e `AuditWarehouseRoute` senza modificarli. Confronta 3 tier e lotti da 1, 5 e 10 Zecche, 27 combinazioni:

- `AtomicWarehouseRoute`: capienza del pagamento dell'intero lotto.
- `SequentialWarehouseRoute`: capienza del prezzo della **ultima** unità acquistata individualmente, nel percorso di Magazzini; lo storico progredisce durante gli acquisti.
- `StartingCapacityEscrowInstallments`: rate minime sul deposito **iniziale** (senza nuovi edifici).
- `SequentialCapacityEscrowInstallments`: rate minime sulla capienza raggiunta dal percorso sequenziale; anche se il percorso si ferma, è soltanto una stima geometrica su quella capacità.

Non sono stati simulati accumulo scorte, disponibilità input, attività delle squadre, interessi, cancello/rimborsi di pratiche giocabili, run naturali, o durata/ROI. Una fattibilità di pagamento **non dimostra un percorso economico percorribile**.

## CLI separata

```powershell
dotnet run --project tools/SimRisorse.Simulation -c Release -- --study-e4b-financing --output .local-cache/e4b-v15/rc79-financing-001
```

Produce `e4b-financing.json`, `e4b-financing.csv`, `e4b-financing.md` in una directory nuova, rifiutando opzioni non riconosciute e sovrascrittura di questi file. Senza `--output` scrive soltanto un breve riepilogo JSON su stdout. I cinque file di output e i replay RC78 non cambiano. Non apre o modifica alcun SQLite.

## Gate di decisione successivo

La comparazione va prima eseguita con .NET 10 e ispezionata. Per il gioco reale sono ancora indispensabili: scelta esplicita della curva (A raccomandata, non approvata); definizione dei moduli 1/3/10 e della squadra da 10; decisione pagamento atomico/sequenziale/escrow e trattamento Magazzini; test salvataggio/ricaricamento e vincitori della concorrenza del personale; benchmark naturali dopo l'implementazione. La GUI v15 mantiene vincolante il contratto v14.

[ADR 0087](../adr/0087-studio-finanziamento-progressivo-senza-attivazione.md) · [Piano test](../validation/E4B_RC79_FINANCING_TEST_PLAN.md).
