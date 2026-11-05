# ADR 0054 — Deposito alimentare: unica titolarità e razioni atomiche

Data: 2026-11-04. Stato: **accettata come struttura di dominio; attivazione gameplay differita a F1-D05**.

## Contesto

Le precedenti RC hanno definito il deposito grezzo/Cibo pronto (F1-D01), le filiere Carne (F1-D02) e Ortofrutta (F1-D03), ma mancava un coordinatore che potesse trasferire tutte le risorse finali al deposito, effettuare la lavorazione e consumare entrambe le razioni evitando copie concorrenti.

## Decisione

Aggiungere `UnifiedFoodSupply.PlanTick` puro, con flusso: inventario cittadino transitorio → deposito grezzo con capienze → Cibo pronto per lotti e personale → consumo atomico di Cibo e quota Pane. Il coordinatore rifiuta `food` già presente come scorta cittadina spendibile. Le nuove cinque sorgenti sono un insieme chiuso; Bovini e Selvaggina devono prima passare dal Macellaio. Le rese sono configurabili.

Il Pane necessario agli abitanti avanzati rimane protetto e non può essere contemporaneamente convertito in Cibo. L'eventuale impossibilità di soddisfare una razione impedisce il consumo **di entrambe le risorse** per quella razione, senza annullare la lavorazione già effettuata nel tick.

## Conseguenze

La simulazione corrente v14, SQLite e GUI restano intatti. L'attivazione richiede migrazione versionata delle risorse, titolarità unica del Cibo pronto, mappatura di lavoratori/capacità e test integrati. Nessun coefficiente fissato in questa ADR. La scelta conserva compatibilità dei salvataggi RC42 e rende più preciso il gate F1-D05.

[Specifica F1-D04](../design/F1_D04_RC43_DEPOSITO_UNIFICATO.md) · [Piano di test](../validation/F1_D04_RC43_TEST_PLAN.md) · [Roadmap](../ROADMAP.md)
