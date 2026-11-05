# ADR 0082 — Analisi basata su evidenze dei benchmark naturali v15

**Data:** 05/11/2026 · **Stato:** decisione implementata, test pendenti · **Release:** RC74.

## Contesto

La RC73 registra esiti e log dei benchmark naturali v15, ma non dispone di uno strumento per compararli. Il termine `blocked` è relativo alle azioni di una politica scripted; `timeout` non equivale a partita strutturalmente impossibile.

## Decisione

- Inserire un **postprocessore** dei JSON RC73, che non esegue il clock e non modifica stato, risorse o salvataggi.
- Separare outcome, diagnosi e grado d'evidenza. I risultati `diagnostic` non possono essere presentati come tempi a orizzonte pieno.
- Misurare attese e spese soltanto quando i dati sono presenti: RC73 registra costi in planks/stone, non l'intero paniere delle risorse.
- Validare ordine degli eventi, tempo e milestone; rifiutare dati incompleti, contraddittori o non naturali.
- Esportare JSON, CSV e Markdown e rifiutare output preesistenti.
- Rinviare la diagnosi di softlock strutturale ai benchmark avanzati e alle strategie alternative; nessun cambio dei parametri E4b in questa RC.

## Conseguenze

Si possono confrontare i tempi **osservati** e i vincoli **segnalati** dalle tre strategie semplici, senza scambiare la mancata esecuzione o un limite dello script per un problema economico dimostrato. Il costo completo di una Zecca e la profondità dei colli di bottiglia richiedono ulteriori dati e scenari E4a.2b.

[Specifiche RC74](../design/E4A_V15_RC74_ANALISI_PROGRESSI.md) · [Piano RC74](../validation/E4A_V15_RC74_TEST_PLAN.md) · [Roadmap](../ROADMAP.md).
