# G1-D39 / RC24 — Messaggi Scuola e consolidamento gate P0

**Baseline confermata:** RC20 (490 test). **Candidata RC24:** 501 test attesi, da compilare/testare Windows; incorpora RC21–RC23 non validate singolarmente. P0 visivo ancora aperto.

## Difetto D39-01, UI-16/UI-20

Il ViewModel v14 bloccava correttamente i comandi `CraftPencilsCommand`, `EnrollCommand` e `CancelStudentCommand` durante la conferma **Nuova partita**, ma `SchoolCraftHint`, `SchoolEnrollmentHint` e `SchoolCancelHint` continuavano a indicare carenze o possibilità operative come se la conferma non fosse aperta. Il messaggio `SchoolResearchHint` era già coerente grazie alla RC18.

**Correzione:** le tre proprietà descrittive controllano `CanMutateColony` prima di descrivere lo stato di materiali, studenti e posti. In conferma annunciano la sospensione temporanea; con Annulla ripristinano le spiegazioni precedenti. Nessun cambiamento a `CanExecute`, regole Scuola, Matite, costo minimo 2 per studente, XAML, Core, SQLite o salvataggi.

**Test:** `V14SchoolConfirmationHintsAuditTests`:
- `AllSchoolActionHintsReportConfirmationAndRestoreAfterCancel` (test ViewModel): confronta blocco comandi, testo di quattro hint e reversibilità con Annulla.
- `SchoolCraftHintAndButtonReflectConfirmationInWindow` (Avalonia headless): controllo binding visibile e pulsante non attivabile nel tab Scuola.

## D39-02 — Registrazione gate P0

Creata la [matrice operativa UI-01…UI-22](../validation/G1_D39_P0_UI_MATRIX.md). Indica test disponibili, prove mancanti e scenari Windows alle dimensioni 1040×740 e 1240×850. **Nessuna voce è considerata visivamente chiusa** finché non è documentata l'esecuzione. È distinto il gate cumulativo RC24 dalla prova estetica P0.

## Vincoli

- Non modificare interfaccia v13, composizione della tabella Risorse, file XAML, simulazione, prezzi, ricerca o persistenza.
- Non dichiarare verde RC21–RC24 finché non arrivano risultati Windows; ultimo verde è RC20.
- Non avviare E4 prima della chiusura P0 documentata.

[Roadmap](../ROADMAP.md) · [Piano test RC24](../validation/G1_D39_RC24_TEST_PLAN.md) · [Matrice D39](../validation/G1_D39_P0_UI_MATRIX.md).
