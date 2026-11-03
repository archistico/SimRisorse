> **Aggiornamento decisionale 03/11/2026:** l’utente ha accettato *solo* il saldo di punti ricerca libero e la spesa su richiesta: [ADR 0021](0021-ricerca-saldo-libero-spesa-su-scelta.md). I settori, i prezzi delle tecnologie, Scuola e prerequisiti descritti qui restano proposti.

# ADR 0020 — Ricerche per settori e punti spendibili (PROPOSTA)

**Data:** 2026-11-03. **Stato: Proposto, NON accettato e NON implementato**. Dipende da ADR 0018 e 0019, anch'essi proposti.

## Contesto
La nuova economia deve rendere la ricerca una scelta di strategia, non un vincolo uniforme per costruire ogni edificio. Il catalogo attuale contiene 16 famiglie con ricetta e un motore di lavoratori aggregati.

## Decisione candidata
- Quattro settori con due livelli ciascuno: Lavorazioni (3 famiglie), Alimentazione (6), Estrazione (3), Industria (4). Dati in `docs/design/G1_D4_TECH_TREE_PROPOSTA.json`.
- Ricercatori qualificati formati nella Scuola, studenti e ricercatori occupano abitanti reali; punti globali spendibili prodotti a tempo simulato con ricercatori attivi, non immagazzinati nei Magazzini.
- Acquisto di ogni tecnologia atomico e permanente, controllo prerequisiti e saldo; livello I prima del II, Industria I richiede Estrazione I.
- Navigazione e Logistica rimangono voci **differite non acquistabili** finché non esiste una funzionalità collegata. La navigazione è prevista quale futuro requisito del Cantiere.
- Nessuna ricerca può disabilitare retroattivamente un edificio v13. La Scuola proposta aumenta i tipi-obiettivo della campagna a 23, se approvato.

## Alternative esaminate
Un progetto attivo a tempo con progressi separati per ricerca è più simulativo ma richiede cambio progetto, prenotazioni e salvataggi ulteriori. Ricerche per ogni edificio moltiplicano artificiosamente i clic e impediscono strategie settoriali.

## Rischi e gate
Softlock per aumento prezzi/stoccaggi, occupazione eccessiva dei ricercatori, obbligo di ricerca troppo lento, sblocchi falsi per contenuti futuri, doppia assegnazione delle persone, ripetizione di acquisti con saldo negativo. Tutti richiedono test e benchmark prima di rilasciare `civil-growth-14`.

## Stato
**In attesa di approvazione esplicita**. Il documento `docs/design/G1_D4_ALBERO_TECNOLOGICO_E_GAMEPLAY.md` è l'analisi di riferimento, non una specifica già operativa.
