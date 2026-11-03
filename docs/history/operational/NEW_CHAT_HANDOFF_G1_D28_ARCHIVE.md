# Nuova chat — SimRisorse G1-D28 / v60-E3b4-RC12

**Allegare lo ZIP integrale RC12.** Consultare [HANDOFF](../../HANDOFF.md), [CURRENT_STATE](../../CURRENT_STATE.md), [ROADMAP](../../ROADMAP.md), [AUTHORITY_REGISTRY](../../AUTHORITY_REGISTRY.md), [ADR 0037](../../adr/0037-v14-ricerca-precisione-e-stati-acquisto.md), [report RC12](../../design/G1_D28_RC12_ALBERO_TECNOLOGICO_PUNTI_E_ACQUISTO.md) e [matrice GUI](../../validation/G1_D19_MATRICE_PARITA_GUI.md).

## Messaggio per riprendere

> Continuiamo SimRisorse **RC12 / G1-D28**. **RC11: test dichiarati passati dall'utente, 466 attesi** (numero effettivo non comunicato); RC12 aggiunge **2 casi Desktop, 468 attesi**, non eseguiti nell'ambiente di confezionamento perché manca .NET SDK 10.
>
> **RC12 corregge Albero tecnologico v14:** saldo punti, costo ricerca e tasso punti/s resi con **due decimali** (`it-IT`); acquisto consentito solo a tecnologia disponibile, non ancora acquistata, con prerequisiti ed edifici soddisfatti e saldo effettivo sufficiente. Il pulsante `V14BuyTechnology` è **grigio/disabilitato** quando non acquistabile, verde e con hover ocra quando sì. `CanBuySelectedTechnology` governa sia `BuyTechnologyCommand.CanExecute` sia `Button.IsEnabled`. Il confronto è in **millipunti esatti**: 99,999 arrotondati a 100,00 non consentono di pagare un prezzo effettivo 100,000. Nessun cambio alle regole economiche.
>
> **RC11:** dettaglio Edifici v14 in due righe `Costruisci | +1 | +5 | +10` e `Lavoratore (−1) | +1 | +5 | +10`; batch lavoratori atomici, stati disabilitati chiari. Edifici bloccati grigi **ma selezionabili**; livelli Base/Grande/Avanzato come edifici distinti, senza ComboBox. Tabella Risorse classica a sette colonne; Esercito pieno; diagnostica Pane; Scuola e Albero in sidebar.
>
> **RC9/RC10:** Scuola come Esercito, barra progresso per studente; Matite da `1 Legno lavorato + 1 Carbone → 4 Matite` (fabbricazione manuale); iscrizione `10 Cibo + 2 Matite`. Salvataggi v14 retrocompatibili, separati dai v13; non reintrodurre «Esci senza salvare» né Costruisci nella scheda Scuola.
>
> **Stato:** Core v13 e SQLite v2 da preservare; ramo v14 SQLite v3 separato. RC8 confermata 456/456, RC9 fallita in compilazione, RC10 e RC11 test confermati dall'utente. RC12 attende `./validate.cmd` (**468 attesi**) e verifica visuale 1040×740/1240×850. **P0 di parità grafica v13/v14 resta aperto; E4 e navale non iniziati.**
>
> A ogni avanzamento: ZIP integrale, test e diagnostica, changelog, ADR, roadmap e handoff; non dare per passati test non eseguiti.

## Documenti

[Rapporto RC12](../../design/G1_D28_RC12_ALBERO_TECNOLOGICO_PUNTI_E_ACQUISTO.md) · [Piano RC12](../../validation/G1_D28_RC12_TEST_PLAN.md) · [ADR 0037](../../adr/0037-v14-ricerca-precisione-e-stati-acquisto.md) · [Rapporto RC11](../../design/G1_D27_RC11_LAYOUT_COSTRUZIONE_LAVORATORI.md) · [Roadmap](../../ROADMAP.md) · [Handoff completo](../../HANDOFF.md).
