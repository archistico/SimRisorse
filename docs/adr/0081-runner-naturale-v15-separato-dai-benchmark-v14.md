# ADR 0081 — Benchmark naturali v15 separati da v14 e audit sintetici

**Data:** 05/11/2026 · **Stato:** decisione di strumentazione implementata, validazione pendente · **RC:** 73.

## Problema

I benchmark naturali precedenti riguardano `V14CampaignSession`; gli audit v15 utilizzano scenari tecnici con scorte e strutture artificialmente preconfigurate. Nessuno dei due può documentare il tempo di progressione di una **nuova partita v15**.

## Decisione

- Creare una CLI distinta `--benchmark-e4a-v15-natural`, con partita iniziale identica al flusso di nuova colonia v15, senza concessioni fittizie.
- Usare soltanto comandi di costruzione, personale e il clock v15, con politica scripted **versionata** e orizzonti dichiarati prima di eseguire i risultati.
- Esportare dati grezzi, eventi e motivi di stop in JSON/CSV; nessuna durata per risultati `timeout` o `blocked`.
- Etichettare esplicitamente le esecuzioni abbreviate come `Diagnostic` e non confonderle con benchmark completi.
- Non modificare parametri di gioco, motore v14, GUI o salvataggi; il gate F1 e la conferma visiva restano necessari.

## Conseguenze

Gli esiti del runner sono misure di una strategia riproducibile, **non** la dimostrazione di un'ottimizzazione o della raggiungibilità in qualunque strategia. RC74 deve interpretare i blocchi e ampliare gli scenari prima che E4b proponga prezzi finali.

[Specifiche RC73](../design/E4A_V15_RC73_NATURAL_RUNNER.md) · [Piano test](../validation/E4A_V15_RC73_TEST_PLAN.md) · [Roadmap](../ROADMAP.md).
