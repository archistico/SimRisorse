# G1-D21 — v60-E3b4-RC5: preventivi cumulativi e visibilità upgrade

**3 novembre 2026. Stato: patch P0 candidata.** La suite di test della RC4 è stata riferita come **passata dall'utente** il 3 novembre; il numero effettivo dei casi non è stato comunicato (443 erano quelli attesi). **RC5 non è stata compilata né provata su Windows in questo ambiente**. La parità visuale con v13 resta aperta.

## Difetti effettivamente riscontrati nel sorgente RC4

1. In `V14PreviewWindow.axaml` sono presenti pulsanti **Costruisci +1, +5, +10** ma nei dettagli compare soltanto il prezzo dell'edificio successivo (+1). Il prezzo progressivo cumulativo, con arrotondamento a ogni unità, era calcolato dal motore ma non presentato. Un giocatore non poteva comprendere il costo di +5/+10 né perché il pulsante fosse disabilitato.
2. `CanUpgradeNext`/`CanUpgradeDirectlyToAdvanced` tenevano conto contemporaneamente di ricerca, possesso e materiali ed erano usati per **nascondere** gli upgrade. Con ricerca sbloccata e materiali insufficienti il comando scompariva anziché essere mostrato e disabilitato.

## Correzione strettamente mirata

- Una sola funzione `QuoteBuild` usata per il costo del prossimo edificio e per il controllo/computo dei lotti +1/+5/+10; non si moltiplica per 5 o 10 il prezzo del singolo. Il calcolo delega a `ProgressiveConstructionPricing.QuoteNewBuildings`, come la sessione v14.
- Nel pannello con scroll dei dettagli Edifici: **Costo complessivo ordini** +5 / +10, eventualmente con `Mancano: Risorsa (disponibile / richiesta)`; informazioni ripetute nel tooltip dei pulsanti. Queste informazioni sono nascoste solo se l'edificio stesso non è costruibile (bloccato). Nessuna ComboBox inserita. Layout, stile, scheda Risorse ed elenco Edifici restano invariati.
- Proprietà distinte **`ShowUpgradeNext` / `ShowUpgradeDirectlyToAdvanced`** per disponibilità logica (esistenza sorgente e tecnologie) e **`CanUpgradeNext` / `CanUpgradeDirectlyToAdvanced`** per disponibilità di materiali. Upgrade con prerequisiti soddisfatti ma troppo costoso: visibile, disabilitato, con costo e risorse mancanti. Upgrade tecnologicamente bloccato: non visibile, requisiti nel dettaglio.
- Notifiche di refresh aggiunte per selezione, tick e costruzione; nessuna modifica alle `GameSession` v13/v14, a schemi SQLite, costanti economiche, grafica classica o altri controlli.

## Test aggiunti (quattro, da eseguire)

| Test Desktop | Verifica |
| --- | --- |
| `BatchQuotesShowCumulativeProgressivePricesAndMissingMaterials` | Quote +5/+10 coincidenti con motore, maggiori di +5×singolo dove cresce il prezzo, mancanze esplicite, comandi disabilitati |
| `BatchQuotesUpdateAfterPurchaseWithoutChangingSelection` | Il contatore di famiglia aggiorna i preventivi dopo acquisto e la selezione resta invariata |
| `UpgradeWithResearchButWithoutMaterialsRemainsVisibleAndDisabled` | Snapshot v14 isolato con tecnologie sbloccate e scorte a zero: entrambi gli upgrade visibili, non eseguibili e con cause leggibili |
| `V14BuildPanelDisplaysBatchQuotesWithoutChangingSevenColumnResources` | Test headless XAML: preventivi visibili solo su edificio sbloccato, tabella Risorse sempre a sette colonne, blocco ricostruito |

**Conteggio indicativo:** RC4 443 attesi e riferiti come passati (senza log numerico) + 4 casi RC5 = **447 attesi**, da controllare tramite discovery `.trx`. I test di v13 non devono essere alterati.

## Rischi e verifiche manuali obbligatorie

1. Due risoluzioni 1040×740 e 1240×850, font/scalatura Windows: preventivi scrollabili, pulsanti stabili, nessuna sovrapposizione con footer o diagnosi. Selezione ocra conservata.
2. Segheria base: costo +1; +5 e +10 devono essere **prezzi cumulativi**; +5 e +10 disabilitati per stock mancanti; dopo +1 i prezzi aumentano e la selezione resta.
3. Segheria grande: ricerca inizialmente bloccata, nessun Costruisci esposto; ricerca acquistata e scorte insufficienti → Costruisci visibile ma disabilitato.
4. Con Segheria base posseduta e tecnologia Lavorazioni I sbloccata, azzerare o consumare materiali: **Potenzia 1 → Grande** resta visibile ma disabilitato, il testo segnala il materiale mancante. Senza ricerca, non visualizzare l'azione e mostrare il prerequisito.
5. Ripetere la matrice P0 completa: Risorse, Esercito, Pane, Scuola, Albero, stati, salvataggi, chiusura e differenti DPI.

**Non eseguire E4** o modifiche al bilanciamento finché l'audit di parità non è superato. [Piano RC5](../validation/G1_D21_RC5_TEST_PLAN.md) · [Matrice di parità](../validation/G1_D19_MATRICE_PARITA_GUI.md) · [Roadmap](../ROADMAP.md).
