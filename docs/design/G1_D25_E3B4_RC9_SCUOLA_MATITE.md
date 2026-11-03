# G1-D25 — RC9: Scuola, progressi e Matite

## Richieste e soluzione

| Requisito | Implementazione | Stato |
| --- | --- | --- |
| Rimuovere «Esci senza salvare» | pulsante eliminato da `MainWindow.axaml` e `V14PreviewWindow.axaml`; API di chiusura headless per test conservata | codice pronto; GUI da vedere |
| Scuola simile a Esercito | schede abitanti, posti, coda, in formazione, qualificati, progresso del primo allievo, tempo residuo, ricercatori attivi | codice pronto; GUI da vedere |
| Matite da Legno e Carbone | laboratorio manuale nella Scuola; 1 `planks` + 1 `coal` = 4 `pencils`; stock con capienza propria nel magazzino v14 | candidato produttivo |
| Minimo 2 Matite per studente | consumo atomico di 10 Cibo + 2 Matite all’iscrizione, con requisito di scuola e popolazione | requisito confermato |
| Risorse classiche | stessa tabella a 7 colonne e medesimo stile; Matite in ordine alfabetico, icona pixel 8×8, breakdown capacità | da verificare visivamente |
| Compatibilità salvataggi | campo opzionale `pencil_paid_students` v3; lettura vecchia v3 con zero pagamenti; backup copy-on-write al primo salvataggio | test aggiunti; Windows pendente |

## Architettura

- `Core/Research/V14SchoolSupplies.cs`: ID risorsa, ricetta, capienza e fallimenti della fabbricazione.
- `Core/Research/SchoolResearch.cs`: costo iscrizione e nuovi rimborsi; con `pencilsPerStudent=0` il prototipo di ricerca puro mantiene il comportamento originario, mentre la UI v14 usa `2`.
- `Core/Sessions/V14CampaignSession.cs`: `CraftPencils`, iscrizione e annullamento con contatore pagamenti; controlli atomici senza scrittura ai DB.
- `Core/Sessions/V14CampaignClock.cs`: preservazione delle Matite nei tick del planner v13 e decrescita corretta del contatore al completamento degli allievi.
- `Core/Sessions/V14CampaignSnapshot.cs`: convalida della 17ª risorsa solo nella v14 e del contatore, con formato invariato (`3`).
- `Infrastructure/Persistence/V14SqliteSaveStore.cs`: colonna facoltativa nei checkpoint già esistenti, obbligatoria nei nuovi; lettura legacy prima di backup e riscrittura sicura.
- `Desktop/Presentation/V14PreviewViewModel.School.cs`: contatori/progressi, messaggi di costo, batch 1/5/10 e controllo dei comandi. `Views/V14PreviewWindow.axaml`: pannello Scuola e barra; `MainWindow.axaml`: rimozione del pulsante.
- `V14EconomyPreview`, `V14PreviewViewModel.Parity` e `PixelGlyph`: Matite visibili nella tabella senza modificare il catalogo base e senza falsi tassi di produzione automatica.

## Regole di compatibilità

L'utente ha confermato **456 test superati su RC8**. La RC9 è una nuova modifica e non eredita automaticamente tale esito. Il gioco v13 deve restare a 16 risorse, con salvataggi SQLite v2 e nessuna «Scuola» v14. I salvataggi v14 già in corso con studenti e senza campo `pencil_paid_students` devono poter essere caricati e completare la formazione; un eventuale annullamento rimborsa il Cibo e non crea Matite mai acquistate.

## Gate aperti

`./validate.cmd` su Windows: **464 test attesi**, ma **non eseguiti** qui, perché manca .NET 10. Eseguire il [piano RC9](../validation/G1_D25_RC9_TEST_PLAN.md), in particolare vecchi save RC8, barra di progresso e testi a 1040×740 e 1240×850. Resta aperta la [matrice generale P0](../validation/G1_D19_MATRICE_PARITA_GUI.md); E4 rimane sospeso.
