# G1-D32 / RC17 — Piano di validazione Windows

**Ultima baseline verde comunicata:** RC16, **476 test passati**. **RC17:** **481 test attesi** (+5 Desktop) e compilazione **non eseguita** in questo ambiente (assenza SDK .NET 10).

## Test automatici

1. Estrarre lo ZIP integrale RC17 in **cartella nuova**. Fare una copia del DB v14 prima di eventuali prove manuali.
2. Eseguire dalla root `./validate.cmd` (in PowerShell ` .\validate.cmd`). Obiettivo: **481 superati, zero falliti e zero ignorati**. Attenzione ai test `PendingNewGameBlocksSaveLoadAndRepeatNewGameWithoutMutatingColony`, `ConfirmedNewGameNeedsExplicitSaveBeforeItMayReplaceExistingCheckpoint`, `PendingConfirmationDisablesSaveLoadButtonsAndRestoresThemOnCancel`, `ClosingWhileNewGameConfirmationIsOpenSavesTheOldColony`, `ClosingAfterConfirmingNewGameWithoutSaveKeepsPreviousCheckpoint`.
3. In caso di fallimento registrare il **primo** errore di compilazione o test, contesto e TRX. Nessuna attestazione anticipata di test verdi.

## Prove manuali UI-20/UI-21 (solo su copia di un checkpoint v14)

- Avviare `./run-game.cmd --experimental-v14`. Costruire un edificio o accumulare risorse, premere **Nuova partita**: la finestra deve mostrare la conferma, **Salva v14**, **Carica v14**, **Nuova partita** e i comandi mutanti devono risultare disabilitati.
- Premere **Annulla**: la vecchia colonia e le sue risorse rimangono intatte e i comandi tornano attivi; la partita resta in pausa fino a Riprendi.
- Ripetere **Nuova partita → Avvia nuova**: i vecchi progressi scompaiono dalla sessione in memoria, ma **il checkpoint sul disco non viene sovrascritto** dal timer o dalla chiusura; il messaggio deve indicare l'autosalvataggio sospeso. Chiudere e riaprire per verificare il vecchio checkpoint.
- Ripetere e usare invece **Salva v14**: il nuovo checkpoint deve essere caricato al successivo avvio. Da quel momento l'autosalvataggio periodico e alla chiusura è nuovamente attivo.
- Provare una **chiusura con la conferma ancora aperta**: deve salvare la **vecchia** colonia e chiudere normalmente, senza creare una colonia nuova.
- Ripetere i controlli visivi a **1040×740** e **1240×850**: tooltip, etichette, contrasto grigio/ocra, schede ancora selezionabili; annotare l’esito nella matrice UI-01…UI-22.

## Stop rule

Non dichiarare chiuso il gate P0 solo perché i 481 test risultano verdi. Non avviare E4 né il navale; v13 e database v13 devono rimanere intatti. Eventuali file SQLite personali non devono finire nell'archivio.

[Rapporto RC17](../design/G1_D32_RC17_PROTEZIONE_CHECKPOINT_NUOVA_PARTITA.md) · [Roadmap](../ROADMAP.md) · [Audit](G1_D29_AUDIT_GUI_V13_V14.md).
