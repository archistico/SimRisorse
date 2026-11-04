# G1-D38 / RC23 — Audit navigazione e selezione v14

**Baseline Windows confermata:** RC20, 490 test. RC21 (493), RC22 (496) non convalidate. **RC23: 499 test attesi (+3), non eseguiti in questo ambiente.**

## Ambito e metodo

La matrice di parità G1 evidenzia ancora il gate di UI-01 (navigazione) e UI-19 (selezione ai refresh). Ispezione statica: la sidebar v14 contiene cinque voci, legate in due direzioni con `SelectedIndex` alle cinque schede; la pagina Risorse usa gli stessi oggetti `ResourceRow` nei refresh, il dettaglio Edifici aggiorna `V14BuildingEntry` senza sostituirli e la lista tecnologie aggiorna righe persistenti.

**Esito dell'ispezione:** nessuna regressione certa che imponga di modificare XAML o ViewModel. Una modifica di layout priva di prova rischierebbe di reintrodurre i difetti già corretti. Perciò D38 introduce esclusivamente protezioni automatiche, non codice applicativo.

## Copertura aggiunta

- `V14NavigationSelectionAuditTests.SidebarAndTabsStaySynchronizedWithoutLosingSelections` ripetuto su viewport 1040×740 e 1240×850: menu → scheda e scheda → menu, cinque pagine, selezione persistente di un edificio, Matite e una tecnologia anche dopo due secondi di simulazione, effettiva visibilità della pagina selezionata.
- `ExplicitSaveAndLoadKeepAllThreeSelectedRowsAndLiveReadModels`: salvataggio volontario e caricamento nello stesso ViewModel; mantiene identità/selezione dei tre oggetti, riaggancia dettagli di capacità e stato della ricerca, riparte in pausa.
- Test sempre isolati su percorsi temporanei, senza toccare i salvataggi utente. Nessun file di gioco v13, XAML, motore, regola economica o schema SQLite modificato.

## Rischi e gate

I test headless non certificano leggibilità, colori, scrollbar o continuità visiva su Windows reale. La matrice UI-01/UI-19 resta **candidata automatica, controllo visivo pendente**. Per chiudere P0 restano 22 esiti e confronto alle due viewport con verifica manuale. RC21–RC23 si possono testare cumulativamente sulla RC23; non dichiararli validati prima di `validate.cmd`.

[Validazione RC23](../validation/G1_D38_RC23_TEST_PLAN.md) · [Roadmap](../ROADMAP.md) · [Matrice parità](../validation/G1_D29_AUDIT_GUI_V13_V14.md).
