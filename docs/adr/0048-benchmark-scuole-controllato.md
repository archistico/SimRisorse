# ADR 0048 — Benchmark E4a misurabili, fixture esplicite e output non falsificati

**Data:** 2026-11-04 · **Stato:** decisione di strumentazione per RC37; esecuzione pendente

## Contesto

L'analisi E4a deve iniziare prima di modificare i costi, ma l'utente non può ancora validare la RC36. Il catalogo E4a contiene dieci scenari con nessun risultato misurato. Una prova controllata è utile purché non venga presentata come raggiungibilità naturale.

## Decisione

Realizzare in `SimRisorse.Application.Benchmarks` un benchmark deterministico che utilizza la sessione v14 autentica, confrontando due studenti con una o due Scuole (E4A-06/07), 30 s per campione, stesso stock aggiunto e costruzioni/Matite/iscrizione attraverso comandi reali. Esporre l'esecuzione tramite il tool CLI già presente e fornire CSV/JSON senza risultati precompilati. Proteggere i file di output esistenti. Introdurre tre test automatici. Mantenere il catalogo scenari `not-run` fino alla prova effettiva.

## Conseguenze

Il benchmark documenta la differenza di coda e il costo aggiuntivo dell'edificio, non la sostenibilità autonoma della colonia. D02–D04 completeranno scenari naturali, altre filiere, persistenza e report; nessun valore E4b viene approvato implicitamente. RC37 resta candidata finché non viene testata su Windows.

[Rapporto](../design/E4A_D01_RC37_BENCHMARK_SCUOLE.md) · [Piano test](../validation/E4A_D01_RC37_TEST_PLAN.md)
