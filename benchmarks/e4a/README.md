# E4a — Benchmark e misure economiche

- `scenarios.csv`: manifest canonico dei 10 scenari. Gli stati `not-run` indicano correttamente che non vi sono ancora dati Windows verificati.
- `results-template.csv`: schema previsto per la raccolta generale E4a (metadati build/checkpoint da rilevare realmente).
- **E4a-D01 / RC37**: E4A-06 e E4A-07 dispongono del primo runner controllato nella CLI `SimRisorse.Simulation --benchmark-e4a-school`. Scrive una serie temporale diversa dal template generale e non finge di avere hash build o file SQLite.

Seguire [il rapporto](../../docs/design/E4A_D01_RC37_BENCHMARK_SCUOLE.md) e [il piano di validazione](../../docs/validation/E4A_D01_RC37_TEST_PLAN.md). Nessun report di risultati è incluso nel repository prima dell'esecuzione effettiva.
