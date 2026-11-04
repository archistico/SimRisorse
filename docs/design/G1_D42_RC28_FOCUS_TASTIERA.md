# G1-D42 / RC28 — Audit focus e tastiera v14

**Data:** 04/11/2026 · **Baseline confermata:** RC27 con 507 test superati (comunicazione utente). **RC28 candidata:** 4 nuove esecuzioni headless (2 scenari × 2 viewport), **511 test attesi**. Test .NET 10 e verifica GUI su Windows **non eseguiti in questo ambiente**.

## Riscontro statico e perimetro

La v14 utilizza `ListBox` nativi per menu, Risorse, Edifici e tecnologie; il contenuto delle schede è presentato da `TabControl` con intestazioni nascoste, mentre la navigazione principale avviene nel menu sinistro. Gli stili originari distinguono selezione e hover ma non prevedevano un marcatore di focus specifico per le liste v14. Ciò rende poco evidente, con la tastiera, quale riga detenga il focus quando una selezione rimane ocra. **Questa è una lacuna di indicazione del focus rilevata nel codice; non un difetto visivo confermato con screenshot Windows.**

### Modifica mirata

- `V14PreviewWindow.axaml`: quattro stili **limitati alla finestra v14**: un bordo di spessore costante (trasparente senza focus) e la sola variazione del colore al focus per `ListBox.gutter-list` (ocra scuro `#9A6947`) e `ListBox.navigation` (ocra chiaro `#F2D49B`).
- La geometria del bordo è identica tra focus e non-focus: nessuna variazione di spessore al passaggio della tastiera; lo sfondo di selezione `#E6D5AC` resta gestito dagli stili esistenti. Nessuno stile globale di `App.axaml` e nessun controllo della GUI v13 modificati.
- `V14KeyboardFocusAuditTests.cs`: controllo headless a **1040×740** e **1240×850** su focus nel menu, freccia Giù con sincronizzazione menu/TabControl, focus su una risorsa selezionata, bordo e sfondo, identità della risorsa e bounds invariati.

Non sono stati cambiati Core, Application, Infrastructure, schema SQLite, produzione, ricette, valori di costo, puntatori di salvataggio o comandi di gioco. Non si anticipa E4.

## Rischi e limiti da confermare su Windows

1. TAB e Shift+TAB attraverso le cinque sezioni, inclusi ComboBox Scuola/Esercito: **manuale pendente**.
2. Enter e Spazio su pulsanti abilitati; verifica che i pulsanti non pagabili non ricevano attivazione: **manuale pendente**, oltre ai test dei comandi già esistenti.
3. Contrasto visivo del bordo di focus con tema e scaling Windows; presenza della selezione ocra e assenza di salti di 1 px; **manuale pendente**.
4. Scroll e focus dopo il cambio edificio e dopo un tick: la protezione RC27 resta invariata, richiede conferma visiva manuale.

**Gate D42:** implementazione e test predisposti; chiusura subordinata al risultato Windows. **Gate P0/D45 ancora aperto:** 22 voci UI da documentare con prove alle due dimensioni.

[Roadmap](../ROADMAP.md) · [Piano test RC28](../validation/G1_D42_RC28_TEST_PLAN.md) · [Runbook P0](../validation/G1_D42_P0_RUNBOOK.md) · [Matrice](../validation/G1_D39_P0_UI_MATRIX.md).
