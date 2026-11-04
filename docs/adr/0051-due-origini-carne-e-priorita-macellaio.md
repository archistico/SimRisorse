# ADR 0051 — Carne da allevamento e caccia, due ricette del Macellaio

Data: 2026-11-04. Stato: **approvata la direzione; implementazione Core isolata in RC41**.

## Contesto

La riforma alimentare richiede una via breve (Cacciatore → Selvaggina) e una via più onerosa ma potenzialmente più efficiente (Grano + Acqua → Stalla → Bovini), entrambe trasformate in Carne da Macellaio. L'economia v14 in produzione consente soltanto una ricetta per edificio; attivare oggi nuovi edifici senza deposito integrato e migrazione potrebbe modificare/sovrascrivere salvataggi esistenti.

## Decisione

- Mantenere `cattle` e `game` come risorse distinte, `meat` come prodotto grezzo destinato esclusivamente al deposito.
- Implementare un planner puro con due alternative per Macellaio, priorità esplicita, lavoratori condivisi, capienza e controllo dei lotti interi.
- Ogni tick usa gli animali disponibili all'inizio per la macellazione, senza anticipare la produzione dello stesso tick.
- Rese economiche configurabili; nessun valore definitivo introdotto prima dei benchmark E4a/E4b.
- Rinviare la registrazione di risorse/edifici nella partita e il versionamento SQLite alla fase integrata F1-D04/D05, preservando GUI v14 e v13 legacy.

## Conseguenze

La nuova logica è testabile e non può influire sulla colonia attuale, ma **la Carne non è ancora giocabile**. F1-D03 aggiunge Orto/Frutteto; F1-D04 unifica la conversione alimentare; F1-D05 completa cataloghi, livelli, salvataggi e migrazioni. La Caccia deve restare un percorso valido anche senza Stalla.

[Specifica](../design/F1_D02_RC41_FILIERA_CARNE.md) · [Test](../validation/F1_D02_RC41_TEST_PLAN.md) · [Roadmap](../ROADMAP.md)
