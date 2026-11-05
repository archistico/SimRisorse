# ADR 0084 — Evidenze di vincolo senza diagnosi automatica di stallo

**Stato:** proposta implementata in RC76, da validare.

## Contesto

RC75 registrava fallimenti generici (`InsufficientResources` ecc.) e le spese delle sole azioni riuscite. Non era possibile individuare con affidabilità i materiali mancanti o osservare la saturazione cittadina.

## Decisione

- Integrare il ledger dei tentativi falliti con i deficit ricavabili dalla quotazione effettiva e dalle scorte possedute al momento del tentativo.
- Campionare separatamente quantità e capacità dei sette materiali cittadini più rilevanti per la progressione industriale.
- Distinguere `measured-shortage`, `failure-code-only` e `capacity-sample` nei report; i report privi delle nuove proprietà restano leggibili, ma senza dati inventati.
- Non alterare la politica scripted del runner, i coefficienti o la GUI.

## Conseguenze

Fornisce dati utili per i prossimi scenari controllati e per la revisione delle strategie, senza qualificare come deadlock un timeout o un magazzino pieno isolato. I dati non misurano ancora la produzione persa per saturazione né il costo energetico/materiale complessivo delle filiere a monte.

[Specifiche](../design/E4A_V15_RC76_ANALISI_VINCOLI.md) · [Validazione](../validation/E4A_V15_RC76_TEST_PLAN.md).
