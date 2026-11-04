# G1-D41 — RC27: selezione e scorrimento nel dettaglio Edifici

**Data:** 04/11/2026. **Baseline Windows confermata dall’utente:** RC26, 504 test superati. **RC27 candidata:** +3 test Desktop headless, **507 test attesi**; compilazione e xUnit non eseguiti in questo ambiente, privo di SDK .NET 10. **Gate P0 visivo ancora aperto.**

## Problema e comportamento desiderato

Il dettaglio Edifici v14 dispone di due aree scrollabili indipendenti: contenuto principale (nome, costi, requisiti, produzione/capacità) e diagnostica con altezza massima 104 px. I controlli erano privi di una sincronizzazione con la selezione dell’elenco. Navigando da una scheda edificio dopo aver letto un testo lungo, gli offset potevano restare sulla posizione della scheda precedente: il nuovo edificio non si apriva necessariamente dall’inizio.

La correzione deve **riportare in alto entrambe le aree soltanto al cambio effettivo di edificio**; aggiornamenti di scorte, produzione, tick e proprietà dello stesso edificio non devono riposizionare la lettura. Non forzare il focus sui pulsanti: il focus resta gestito da Avalonia, e la navigazione da tastiera nell’elenco non deve essere interrotta.

## Implementazione circoscritta

- `src/SimRisorse.Desktop/Views/V14PreviewWindow.axaml`: assegna il nome `V14BuildingDetailScroll` allo `ScrollViewer` principale preesistente e collega `SelectionChanged` alla finestra direttamente in XAML. Nessuna modifica a dimensioni, colonne, colori, testi, stili o binding dei dati.
- `src/SimRisorse.Desktop/Views/V14PreviewWindow.axaml.cs`: si collega a `V14Buildings.SelectionChanged` direttamente tramite l’evento XAML, anche se la scheda viene ricreata dal `TabControl`. Reimposta `Offset` a `(0, 0)` nei due viewer del dettaglio, e solo quando cambia la selezione del ListBox; non sottoscrive i refresh del ViewModel e non imposta `Focus()`.
- `tests/SimRisorse.Desktop.Tests/V14BuildingDetailSelectionAuditTests.cs`: tre casi nuovi: cambio tra due edifici con contenuto volutamente lungo a **1040×740 e 1240×850** (due ScrollViewer e selezione coerente); aggiornamento dello stesso edificio con offset non azzerato (un terzo test). I messaggi lunghi sono sintetici solo nei test; nessuna modifica economica.

## Gate e invarianti

1. Eseguire su Windows `./validate.cmd`: **507/507 test attesi**, distinguere build da test e raccogliere TRX se fallisce.
2. Aprire `./run-game.cmd --experimental-v14` a 1040×740 e 1240×850. Scorrere fino in basso un edificio con molti requisiti e/o avvisi, selezionare un altro edificio: dettaglio e diagnostica devono ripartire dall’alto; il titolo deve essere leggibile.
3. Restare su uno stesso edificio mentre la produzione aggiorna le quantità, verificare che l’area di lettura non si sposti all’inizio; navigare con frecce fra gli edifici senza perdita del focus sulla lista.
4. Verificare la visibilità della riga Costruisci e della riga Lavoratore, e il riquadro diagnosi indipendente. Non contrassegnare UI-07/UI-12/UI-18 come chiuse senza screenshot/prova Windows.

**Invariati:** GUI classica v13, Core/Application/Infrastructure, regole E4, SQLite e salvataggi, catalogo delle 17 risorse, layout della tabella Risorse, posizioni dei pulsanti Edifici. P0 resta aperto; prossima milestone G1-D42: rimanenti verifiche visive, controlli della tastiera e leggibilità; dopo gate P0 si passa a P1 Scuola/tecnologie.

[Roadmap](../ROADMAP.md) · [Matrice P0](../validation/G1_D39_P0_UI_MATRIX.md) · [Piano RC27](../validation/G1_D41_RC27_TEST_PLAN.md).
