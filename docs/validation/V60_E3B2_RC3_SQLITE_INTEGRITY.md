# V60 E3b2 RC3 — Integrità della formazione nel salvataggio SQLite v3

**Data:** 3 novembre 2026  
**Base:** v60-E3b2-RC2 (validazione completa `validate.cmd` confermata dall’utente)  
**Stato:** implementato in ramo isolato, **non compilato né eseguito con .NET 10 in questo ambiente**.

## Difetto corretto

In RC2 `progression_training` conteneva le righe `(position, progress_seconds)`, ma la tabella padre `progression_school_state` **non conservava il numero atteso delle righe in formazione**. Se veniva rimossa l’ultima riga (es. rimane solo la posizione 0 dopo avere cancellato la 1), la contiguità della lista era preservata e il caricatore poteva accettare una partita con uno studente scomparso. È un difetto di integrità reale, non un problema di ordinamento dei test.

## Correzione

- La tabella `progression_school_state` contiene ora `training_students INTEGER NOT NULL CHECK(training_students >= 0)`.
- Lo store scrive `SchoolResearch.TrainingStudents` come valore indipendente dal contenuto di `progression_training`.
- Il caricatore richiede **esattamente** quel numero di righe; un buco, una riga in meno o una in più generano `GameSaveException(InvalidData)`.
- Le versioni sperimentali RC1/RC2 con `user_version=3` ma senza il nuovo campo vengono esplicitamente respinte con `UnsupportedFormat`, **senza tentativi di riparazione implicita né sovrascrittura**. Non hanno mai rappresentato il formato giocabile ufficiale. Il file v13 originale rimane sorgente per ricreare una copia v3 corretta.
- Backup S1, salvataggio tramite file temporaneo, schema militare e store v13 preesistente sono invariati.

> **Nota di versionamento:** si tratta di un irrigidimento dello *schema sperimentale v3 non ancora rilasciato in una campagna giocabile*. Un checkpoint sperimentale creato con RC1/RC2 non è leggibile dalla RC3. È una incompatibilità intenzionale e non distruttiva; se fosse necessario preservare file sperimentali v3 già valorizzati manualmente, servirà una migrazione esplicita dedicata prima di usarli.

## Test nuovi

1. `MissingFinalSchoolTrainingPositionIsRejectedAndCannotOverwriteCheckpoint`: coda di due studenti, eliminazione dell’ultima riga, caricamento e sovrascrittura respinti, nessun backup erroneo.
2. `InconsistentDeclaredStudentCountIsRejected`: modifica del numero dichiarato senza alterare le righe, caricamento respinto.
3. `PreIntegrityExperimentalV3IsNotSilentlyInterpretedAsCurrentSchema`: assenza del nuovo campo, `UnsupportedFormat` e file invariato.

**Target totale xUnit:** 380 casi (377 confermati nella RC2 + 3 nuovi), oltre ai benchmark e alle simulazioni in `validate.cmd`.

## Verifiche effettuate qui

- Archivio RC2 estratto e controllato, nessun file personale aperto.
- Schema SQLite estratto dal sorgente, creato con `sqlite3` Python in memoria e ispezionato.
- Verificata in SQL la differenza tra 2 righe previste e 1 rimasta dopo il taglio dell’ultima posizione.
- Simulata la rimozione della colonna `training_students` e controllata l’assenza nello schema precedente.
- **Non eseguiti** `dotnet build`, xUnit, test Avalonia o `validate.cmd`: .NET SDK non disponibile nell’ambiente.

## Vincoli che restano aperti

- Non attivare il ruleset candidato `civil-growth-14` in `GameSession` v13, SQLite v2 e GUI.
- E3b/3 creerà la sessione v14 isolata, riutilizzando E1/E2/E3 e `V14CampaignSnapshot` in modo atomicamente coerente.
- Tutti i coefficienti economici e i costi della Scuola restano **parametri sperimentali**; è approvato solo l’accumulo libero e la spesa volontaria dei punti ricerca.
- Prima dell’attivazione v14 saranno necessari round-trip completi, conversioni su copie, test anti-softlock e conferma Windows.
