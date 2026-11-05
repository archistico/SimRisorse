# ADR 0085 — Produzione non realizzata e recupero controllato

- Data: 05/11/2026.
- Stato: implementata in RC77 / E4a.3.

RC76 campiona scorte e capacità e registra deficit di acquisto. Una scorta piena non prova da sola che sia stata impedita una produzione; un timeout dello script non prova uno stallo strutturale.

RC77 espone, su richiesta, i report del planner realmente eseguito dal clock v15 nel primo secondo. Il benchmark usa tick singoli riconciliati, misura nominale e prodotto per addetto, distingue vincoli esclusivi e misti senza doppia attribuzione.

Le strategie di recupero sono esperimenti `controlled/declared-grants`: partono dallo stesso checkpoint immutabile e usano comandi reali con ricevute. I checkpoint iniziali finanziati sono esportati integralmente. Questi dati non confluiscono nei manifest delle partite naturali.

La diagnostica è opzionale; non modifica scheduling, prezzi, ricette, GUI, schema SQLite o crescita ×20. Il completamento tecnico RC77 non sostituisce il confronto interattivo della GUI richiesto per promuovere F1.

[Specifica](../design/E4A_V15_RC77_PRODUZIONE_PERSA_E_RECUPERO.md) · [Verifiche](../validation/E4A_V15_RC77_TEST_PLAN.md).
