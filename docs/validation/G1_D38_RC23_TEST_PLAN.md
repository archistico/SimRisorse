# G1-D38 / RC23 — Piano di validazione Windows

**RC20:** ultimo checkpoint confermato, 490 test. **RC21** (+3, 493 attesi), **RC22** (+3, 496 attesi) e **RC23** (+3, **499 attesi**) non ancora testate su Windows. Una sola validazione su RC23 copre tutta la serie.

## Compilazione e test

1. Estrarre RC23 in cartella nuova, senza copiare database personali; conservare una copia separata dei salvataggi esistenti.
2. Eseguire `./validate.cmd` (`.\validate.cmd` in PowerShell), conservando il log e il TRX. Verificare che il progetto Desktop.Tests compili senza errori e che tutti i 499 test siano verdi; nessun risultato è stato eseguito dal confezionamento.
3. Individuare `V14NavigationSelectionAuditTests`: due casi alle dimensioni 1040×740 e 1240×850, uno Salva/Carica; controllare anche i test introdotti in RC21/RC22 (upgrade e scorrimento diagnostica, addestramento Esercito).
4. In caso di problemi, riportare metodo, messaggio d'errore, riga, stack trace e conteggio effettivo; fermarsi al gate automatico prima di E4.

## Checklist visiva P0

- A entrambe le viewport, aprire le cinque sezioni dalla sidebar; verificare che la pagina corrispondente sia aperta, senza barra schede duplicata. Tornare a Risorse/Edifici/Albero: l'elemento selezionato non cambia e la scrollbar non interferisce con le colonne.
- Eseguire una produzione in gioco e una prova manuale Salva/Carica **su copia** del database v14; verificare selezioni, valori aggiornati, stato dei comandi e la pausa iniziale dopo caricamento.
- Ripetere i gate RC17 (nuova partita non sovrascrive checkpoint fino al Salva manuale), RC21 (upgrade grigi/hover), RC22 (diagnosi scrollabile e training); niente test su database personali senza backup.
- Registrare nel rapporto le voci UI-01…UI-22 con esito e screenshot/note a 1040×740 e 1240×850. La sola suite verde non chiude P0 grafico.

[Rapporto RC23](../design/G1_D38_RC23_AUDIT_NAVIGAZIONE_SELEZIONE.md) · [Roadmap](../ROADMAP.md) · [Matrice](G1_D29_AUDIT_GUI_V13_V14.md).
