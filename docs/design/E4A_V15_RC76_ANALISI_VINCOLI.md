# RC76 / E4a.2c — Evidenze di carenza e pressione sui magazzini

**Stato:** implementazione candidata; .NET 10 e benchmark completi ancora da verificare.

## Obiettivo

Arricchire i report del runner industriale RC75 senza introdurre risorse artificiali, alterare GUI, costi, ricette o storage. La diagnostica non equivale al bilanciamento E4b e non dimostra da sola uno stallo strutturale.

## Telemetria acquisita

1. Ogni tentativo di azione fallito per `InsufficientResources` o `InsufficientMaterials` può registrare il **deficit concreto** per risorsa, calcolato come `max(0, costo quotato - scorta cittadina al momento del tentativo)`. Nessun costo viene addebitato per un fallimento.
2. Per l'iscrizione alla Scuola, `InsufficientFood` confronta la quota richiesta con il **Cibo pronto nel Deposito**; `InsufficientPencils` confronta le **Matite cittadine**. Non vengono mescolate le proprietà fisiche delle scorte.
3. Ogni campione registra quantità e capacità cittadine al millesimo di `planks`, `stone`, `coal`, `iron-ore`, `iron`, `gold-ore`, `gold`, utilizzando `V15CityStorage.Calculate` e i dati del checkpoint. Non include i compartimenti alimentari del Deposito (perimetro esplicito).
4. `V15IndustrialConstraintAnalysis` classifica osservazioni di carenza per materiale, forza lavoro, ricerca, formazione, prerequisiti e potenziale pressione di capacità. `capacity-sample` è soltanto una **saturazione osservata** (`scorta >= capacità > 0`), non la prova che la produzione sia stata fermata.
5. I report storici RC75 possono essere riletti: gli eventuali campi mancanti sono considerati `unmeasured`, non quantità zero e non assenza di problemi.

## Output e compatibilità

La CLI `--benchmark-e4a-v15-industry` e `--analyze-e4a-v15-industry` aggiungono ai report preesistenti:

- `e4a-v15-industry-constraints.json`
- `e4a-v15-industry-constraints.csv`
- `e4a-v15-industry-constraints.md`

La struttura e i nomi dei report RC75 rimangono invariati. I campi opzionali aggiunti ai record JSON non rimuovono proprietà preesistenti. Una directory con risultati già presenti non viene sovrascritta.

## Limiti interpretativi

- I deficit sono rilevati all'istante di un tentativo; ripetere lo stesso deficit in due momenti **non** significa sommarli come costo totale.
- La pressione sulla capienza è campionata; non misura direttamente quanto output potenziale sia stato perso.
- `InsufficientPopulation` non distingue automaticamente domanda di abitanti per nuove case da personale per edifici. Il codice del fallimento è conservato, non amplificato.
- Un `timeout`/`blocked` di una strategia scripted non dimostra un deadlock nel gioco. Non è presente una politica autonoma di recupero: i suggerimenti sono verifiche successive, non azioni eseguite.
- L'analisi dei *costi indiretti completi* richiede ancora un ledger delle produzioni e dei consumi a monte, non soltanto le ricevute degli acquisti.

## Validazione richiesta

`validate-f1.cmd` su Windows con 867 test attesi, verifiche dei due audit v15 e conferma visiva della GUI. Solo dopo eseguire scenari a orizzonte pieno e confrontare il nuovo report di vincoli con le ricevute e la partita reale. [Piano test RC76](../validation/E4A_V15_RC76_TEST_PLAN.md).
