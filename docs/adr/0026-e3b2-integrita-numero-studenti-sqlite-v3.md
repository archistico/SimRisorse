# ADR 0026 — Integrità del numero di studenti in SQLite v3 sperimentale

- **Stato:** adottato nel ramo E3b/2 sperimentale; validazione .NET pendente.
- **Data:** 2026-11-03.

## Contesto

In v60-E3b2-RC2 l’elenco `progression_training` non aveva un conteggio indipendente delle righe attese. La perdita dell’ultima riga rendeva ancora continua la sequenza delle posizioni e non era rilevabile.

## Decisione

Aggiungere il contatore non negativo `training_students` alla riga padre `progression_school_state`. Scriverlo a ogni checkpoint e verificarlo durante il caricamento confrontando il numero esatto di progressi. Un file v3 sperimentale privo del contatore non viene reinterpretato né sovrascritto; il formato v13 rimane invariato.

## Motivazione

I progressi sono dati applicativi persistenti; il salvataggio deve poter identificare la perdita dell’ultima posizione anche quando SQL non presenta errori strutturali. Il conteggio esistente `WaitingStudents` non è equivalente e non deve essere usato come sostituto.

## Conseguenze

- +1 colonna nella tabella della Scuola v3; nessuna tabella aggiunta.
- +3 test mirati; target 380 test totali.
- Incompatibilità non distruttiva con i file **sperimentali** v3 delle RC1/RC2, documentata. Non è un cambio della campagna giocabile.
- Validazione Windows `validate.cmd` da confermare prima di passare a E3b/3.
