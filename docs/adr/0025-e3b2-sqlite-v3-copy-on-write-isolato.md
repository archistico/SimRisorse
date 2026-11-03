# ADR 0025 — Persistenza v14 separata e scrittura copy-on-write

**Data:** 2026-11-03  
**Stato:** PROPOSTO, implementazione sperimentale E3b/2 — test .NET non eseguiti

## Contesto

La campagna `civil-growth-13` utilizza `SqliteGameSaveStore` e `GameSnapshot` formato v2. Le famiglie produttive a livelli, i ricercatori e i progressi individuali richiedono una struttura v3 che non può essere interpretata dal lettore v2.

## Decisione progettuale

Implementare un `V14SqliteSaveStore` **distinto** dall'interfaccia di salvataggio del gameplay, che accetta soltanto snapshot validati da `V14CampaignSnapshots.Validate` e file `application_id=SIMR`, `user_version=3`, `rules_version=civil-growth-14`. Conversione v13 eseguita esclusivamente in copia verso un percorso diverso. Nuovo file scritto con transazione SQLite e promozione copy-on-write; backup SQLite coerente S1 prima di sostituire un v3 esistente. File preesistente non valido o in generazione v2: rifiuto senza sovrascrittura.

## Conseguenze

- Manteniamo il comportamento e i test precedenti di v13.
- La migrazione non può danneggiare per errore un file v2 sullo stesso percorso.
- Costi I/O e numero di backup potenzialmente elevati: da definire la retention e la strategia finale per autosave prima del rilascio pubblico.
- I nuovi test Infrastructure devono essere validati su Windows prima di integrare la classe nella sessione e nella UI.

**Non considerare questa ADR un'approvazione dei costi +15%, dei moltiplicatori tier, delle caratteristiche della Scuola o delle ricerche candidate.** Resta accettato soltanto l'accumulo libero e la spesa volontaria dei punti ricerca (ADR 0021).
