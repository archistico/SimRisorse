# ADR 0086 — Confronto isolato delle curve economiche

- Data: 06/11/2026.
- Stato: strumento implementato RC78 / E4b.1; valori economici proposti, non approvati.

Le misure naturali RC77-M01 permettono di confrontare nuove ipotesi senza inventare tempi di completamento. Si introduce uno studio deterministico in Application/Benchmarks con tre curve esplicite, quotazioni del motore esistente, filiera deduplicata, investimento per risorsa, lavoro nominale dei materiali, ricerca e budget del personale alimentato/alloggiato.

La capacità è verificata lungo un percorso dichiarato di Magazzini Base pagati prima di ampliarla. I lotti sono pagamenti atomici; gli upgrade vengono confrontati separatamente. Un blocco di questo percorso non prova uno stallo globale.

Le squadre da dieci sono proiettate analiticamente. I moduli 1/3/10 sono candidati; prezzi e produzione delle partite esistenti restano quelli precedenti. La scelta numerica richiede la decisione prevista dal P00 dopo il rapporto, poi implementazione versionata e nuove campagne naturali. Nessuna modifica a Core, Desktop o SQLite.

[Specifica](../design/E4B_RC78_THREE_CURVES_STUDY.md) · [Confronto](../validation/E4B_RC78_CURVE_COMPARISON.md) · [Verifiche](../validation/E4B_RC78_TEST_PLAN.md).
