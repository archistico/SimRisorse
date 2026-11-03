# R0–R2 · Recupero del ramo militare v41 nella v57

**3 novembre 2026 · Rapporto storico di integrazione RC1.** Per il risultato Windows della RC2 (295/299 test) e le correzioni proposte in RC3 consultare [V58_RC3_299_TESTS.md](V58_RC3_299_TESTS.md).

## Input immutati

- `SimRisorse-v57.zip` — base ufficiale documentale (codice v55), 21 edifici, 16 risorse, regole `civil-growth-12`.
- `SimRisorse.zip` — ramo v41 modificato il 03/11, addestramento individuale ma senza gli incrementi successivi del Municipio, depositi e UI.
- Diagnosi completa degli input: [`R0_V41_V57_AUDIT.md`](R0_V41_V57_AUDIT.md), insieme all'inventario `DIFF_MANIFEST_V41_V57.json`.

## Modifiche sviluppate nella nuova copia

### Core
- Versione `MilitaryTrainingRules.IndividualBarracks` accanto alla storica `SimpleBarracks`.
- Stato `MilitaryTrainingState` esteso con `IndividualProgressSeconds` ordinato: campo assente/default = v10–v12 storico; array inizializzato (anche vuoto) = regole v13.
- `IndividualRecruitment`: arruolamento atomico, progressi individuali, completamenti scalati per posti, coda, cancellazione dalla coda e poi dall'addestramento meno avanzato, rimborso atomico e conversione storica.
- Vietato il congedo nel nuovo ruleset; v12 storico conserva la possibilità di congedare.
- Nessun cambiamento alle regole del Municipio, ai produttori o ai magazzini.

### Application/Desktop
- `CivilGrowthScenario.Create()` usa `civil-growth-13`; factory congelata v12 `CreateTownHallForBenchmarks`.
- Nuovo use case `CancelRecruitsHandler` nella scheda Esercito v57; azioni vecchie conservate per i client storici.
- ReadModel con progresso individuale e capacità del congedo; descrizioni della Caserma e della Forgia condizionate dalle regole attive.

### Simulazione/validazione
- Strategie `forge`, `mint`, `crafting` e `military` recuperate/adattate dal fork senza sostituire i benchmark avanzati della v57.
- Il gate Windows aggiunge un probe `--strategy military --hours 8`: verifica costruzione Caserma e almeno un soldato addestrato, registrando JSON. Il tempo effettivo per 20 soldati resta da misurare.

### SQLite
- File v2 mantenuto; colonna storica `military_state` mantenuta e tabella aggiuntiva per progressi individuali nel nuovo ruleset.
- Identificazione fisica dello schema v41 tramite `PRAGMA table_info`; caricamento con array di progressi, validazione conteggi e ordinamento.
- Sul primo salvataggio v13 di un database v41: backup S1 tramite `VACUUM INTO`, trasformazione delle tabelle nella transazione e ricostruzione coerente.
- Nessuna apertura o modifica automatica dei DB personali durante la preparazione.

## Politica di migrazione proposta

Per `civil-growth-12` → `-13`: ogni soldato resta soldato e ogni recluta resta recluta; il numero di posti passa da 2 a 1 per Caserma. La percentuale completata per il gruppo viene trasformata da `oldProgress/60` a `floor(oldProgress/2)` secondi su 30. Le persone eccedenti i nuovi posti confluiscono nella coda, senza conservare il loro tempo già svolto, dato che non esiste più un posto attivo per loro. Scorte, crescita civile, elapsed ticks, abitanti, edifici e assegnazioni civili non vengono cambiati. Salvataggio precedente archiviato con backup S1 prima della prima sovrascrittura.

Per il fork v41: identificare **schema**, non solo stringa `civil-growth-10`; mantenere i progressi individuali esistenti, aggiornare i cataloghi e aggiungere un unico Municipio senza risorse supplementari.

## Test aggiunti (non eseguiti)

- Core: costi/regole, 30 s individuali, arruolamento di più persone, addestramento sfalsato, annullamento, saturazione deposito, stati invalidi, conversione da v12.
- Application: scenario corrente e storico, Municipio e Pane preservati, rifiuto del congedo, avanzamento/pausa/salvataggio, razioni costanti e rimborso delle Spade.
- Infrastructure: checkpoint v41 sintetico con tabelle alternate, backup del vecchio schema, conversione, doppia lettura e controllo di progressi corrotti.
- Regressioni storiche v12 mantenute esplicitamente su factory v12; le prove della UI storica v12 rimangono così riproducibili.

## Gate bloccante / Limiti noti

- **SDK .NET non presente**: non eseguiti compilazione, test xUnit, validatore PowerShell, benchmark e screenshot Windows.
- **Non testato con un DB reale della partita personale**: le prove predisposte usano DB temporanei sintetici.
- I 48 test aggiuntivi previsti nel fork v41 non sono stati copiati meccanicamente: sono stati ricostruiti solo i casi principali sopra elencati; verificare altri casi e integrare un benchmark militare per la M5.3c.
- La regola di conversione del tempo parziale e il rinvio del congedo v13 restano da ratificare se necessario prima di un rilascio ufficiale.

**Istruzione:** su Windows eseguire `\.\validate.cmd` in una directory nuova, conservare log ed eseguire i test su copie dei save. Non sostituire le partite personali con questo candidato non verificato.

**Nota economica:** le reclute arruolate prima della conversione non pagano retroattivamente la differenza da 2 a 5 Spade. Le 5 Spade si applicano esclusivamente ai nuovi arruolamenti del ruleset v13; nessuna spesa nascosta durante il caricamento.
