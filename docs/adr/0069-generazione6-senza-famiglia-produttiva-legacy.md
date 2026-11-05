# ADR 0069 — Gen. 6: non salvare la famiglia produttiva legacy `food-store`

**Stato:** accettata per sviluppo, da confermare con test RC61. **Data:** 2026-11-05. **Milestone:** F1-D05e.2b.1.

## Contesto

La v15 usa il nuovo Deposito come unico produttore di Cibo ma la parte civile in-memory riutilizza `V14CampaignSnapshot` e `V14CampaignSession`. Fino alla RC60 SQLite serializzava anche la sedicesima famiglia `food-store` pur non essendo acquistabile nella v15. Lo sviluppatore ha autorizzato la rottura della compatibilità con salvataggi *sperimentali*, preservando v14 e GUI storica.

## Decisione

1. Il formato sperimentale diventa **generation 6**; conserva tutte le 16 tabelle ma `progression_family` registra solo 15 famiglie attive (45 tier). Aggiungere un vincolo SQL contro `food-store`.
2. `V15LegacyProducerAdapter` ricrea un unico slot vuoto *in memoria* per la validazione e i comandi v14. Il residuo verrà eliminato con comandi civici v15 autonomi.
3. Se un vecchio `food-store` è ancora occupato, il writer fallisce prima di aprire o creare un DB; non si perdono edifici/lavoratori implicitamente. Il ritiro è azione esplicita, già disponibile in sessione.
4. Il contatore d'acquisto rimane storicamente persistito. Nessun cambio a clock, formule economiche o UI. DB gen. 5 non supportati né migrati in automatico.

## Conseguenze

Riduzione del modello **persistito** senza destabilizzare la GUI. Una fase successiva dovrà eliminare dipendenze civiche e contatore legacy *in memoria*; questa ADR non dichiara il nuovo checkpoint interamente indipendente da v14. Validazione obbligatoria `validate.cmd` con **761 test attesi**. Conservare la separazione fisica dal DB personale v14.
