# G1-D18 — Ripristino di funzioni e grafica v13 nella v14 (RC3)

**Stato:** patch candidata non compilata in questo ambiente. **Ultimo gate Windows confermato:** RC1, 427 test. La RC2 è stata segnalata come regressione dall'utente: non è una base grafica approvata.

## Causa e soluzione

La RC2 aveva replicato i colori e il layout generale della v13, ma aveva sostituito le schermate originali con viste ridotte e ViewModel differenti. La regola correttiva è **riusare la logica di presentazione originale** e, dove non sia ancora tecnicamente condivisa, riportare per intero la struttura della scheda. Non far sparire funzioni durante il passaggio da v13 a v14.

| Area | Regressione RC2 | Correzione candidata RC3 | Verifica necessaria |
| --- | --- | --- | --- |
| Risorse | Solo 3 colonne | 7 colonne, intestazioni, allineamenti e stili presi da `MainWindow.axaml`; `ResourceRow` condiviso | UI e larghezza colonne |
| Quantità | DTO semplificato | Quantità arrotondata, numero esatto al hover | Controllo tooltip |
| Capacità | Nessun dettaglio | Tooltip completo con capacità iniziale, edifici e quantità per Base/Grande/Avanzato | Pane, risorse speciali |
| Produzione/Consumo/Saldo | Assenti | Proiezione read-only con gli stessi `ProductionPlanner` e `PopulationPlanner` del clock v14 | Produzione bloccata e consumi avanzati |
| Si riempie tra | Assente | Lo stesso calcolo `hh:mm:ss` del `ResourceRow` v13 | Zero e valori negativi |
| Popolazione | Solo numero | Ritorno della barra di crescita, dei fabbisogni e dei posti abitativi | Rapporto con Pane |
| Esercito | Sola lettura | Pagina storica e comandi arruola/annulla (congedo vietato se addestramento individuale) | Cicli reclute e Spade |
| Edifici bloccati | «Costruisci» visibile | Azioni nascoste con requisiti mancanti, prezzi consultabili | Visibilità/disabled |
| Materiali insufficienti | Diagnosi debole | Costi con icone, risorse disponibili/necessarie, pulsanti disabilitati | Stato selezione |
| Livelli | Righe distinte, correttamente | Mantiene le 54 righe; ordini +1/+5/+10 senza ComboBox; upgrade espliciti | Scorrimento e dettagli |
| Salva/Nuova partita | Manca nuova partita | Conferma per nuova partita, autosalvataggio ogni minuto, DB v14 separato | Salvataggio alla chiusura; uscita non bloccata con DB incompatibile |

## Modifiche principali

- `V14EconomyPreview.cs`: proiezione non mutante di quantità e flussi per secondo riutilizzando i pianificatori originali.
- `V14CampaignSession.cs`: proiezione e ordini militari atomici con regole v13 già consolidate; non modifica il `GameSession` della campagna.
- `V14PreviewViewModel.Parity.cs`: proiezione in `ResourceRow`, capacità condivisa, popolazione e comandi militari.
- `V14PreviewViewModel.Buildings.cs`: blocchi vs materiali insufficienti; costruzioni +1/+5/+10; diagnosi.
- `V14PreviewWindow.axaml`: tabella Risorse e pannello militare ripresi dalla vecchia finestra; cinque sezioni nella sidebar; dettaglio senza ComboBox.
- Test di parità nelle suite Desktop e Application.
- La finestra si chiude normalmente anche se il salvataggio è vietato per database corrotto o v13; non tenta di sovrascriverlo.

## Non confondere livelli di verifica

- **Codice:** modifiche introdotte; compilazione .NET non eseguibile in questa sessione.
- **Controlli statici:** XML, JSON, integrità ZIP, struttura e differenze dei file.
- **Test .NET:** da eseguire su Windows; numero previsto 439 (430 RC2 più 9 test nuovi); se il numero effettivo cambia verificare test discovery.
- **Audit visivo/manuale:** ancora da fare, non dichiarare il restauro grafico approvato senza schermata reale.

## Aspetti ancora da controllare

La diagnosi avanzata della filiera del Pane della v13 comprende spiegazioni specifiche del collo di bottiglia; la RC3 ne ripristina crescita e consumo, ma il dettaglio diagnostico per ogni stato di `BreadSupplyProjector` deve ancora essere messo a confronto. La UI v14 mantiene un XAML distinto, per cui va pianificato un fattore di condivisione delle viste dopo il gate.

Riferimenti: [ADR 0033](../adr/0033-preservazione-funzionale-gui-v13-v14.md), [test plan RC3](../validation/V60_E3B4_RC3_PARITY_TEST_PLAN.md), [roadmap](../ROADMAP.md).
