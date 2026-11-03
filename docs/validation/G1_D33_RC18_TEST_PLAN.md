# G1-D33 / RC18 — Piano validazione

**Ultimo esito Windows confermato:** RC16, 476 test. **RC17:** 481 attesi, non verificata. **RC18:** 484 attesi = RC17 + 3 Desktop. Compilazione e xUnit non disponibili nell'ambiente di preparazione.

## Validazione automatica su Windows

1. Estrarre **RC18 in una cartella nuova**; non testare su database personali senza una copia di sicurezza.
2. Eseguire `./validate.cmd` (in PowerShell ` .\validate.cmd`). Gate: compilazione senza errori e **484 test superati, zero falliti/ignorati**.
3. Controllare `ResearchSeatHintUsesTheSelectedBatchSizeAndAgreesWithCommandState`, `SchoolResearchHintDistinguishesMissingSchoolFromMissingQualifiedResearchers` e `ResearchHintReportsMissingQualifiedPeopleWhenSeatsAreSufficient`. La suite completa comprende i cinque test di protezione dei salvataggi RC17 che non sono ancora stati confermati su Windows.
4. In caso di errore, inviare **il primo errore** completo e TRX; non attribuire un pass alla RC18 senza output effettivo.

## Controlli manuali

- Aprire `./run-game.cmd --experimental-v14` (con salvataggio v14 di prova). Nella Scuola scegliere +5/+10 per assegnare ricercatori quando esiste una Scuola ma meno posti ricerca disponibili: il pulsante deve restare disabilitato e il testo deve spiegare quanti posti mancano.
- Con lotto +1, qualificati sufficienti e almeno un posto, pulsante attivo; dopo l'assegnazione la disponibilità indicata deve aggiornarsi. Durante «Nuova partita?» il suggerimento deve indicare il blocco temporaneo, non suggerire un'azione disponibile. Con posti sufficienti ma pochi qualificati, deve apparire il motivo preciso.
- Controllare UI-16 e gli stili disabilitati a **1040×740** e **1240×850**. Conservare le 22 voci della matrice come gate P0 ancora aperto.
- Ripetere i flussi checkpoint RC17 prima di dichiarare sicuri salvataggio e chiusura della v14.

## Limiti

Nessuna variante economica, nessun cambio SQLite. La GUI classica v13 resta invariata. **E4 sospeso**.

[Rapporto](../design/G1_D33_RC18_AUDIT_SCUOLA_LOTTI.md) · [Roadmap](../ROADMAP.md).
