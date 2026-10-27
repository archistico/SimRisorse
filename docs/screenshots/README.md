# Screenshot dell'applicazione

Questa cartella contiene screenshot del progetto, versionati per essere visibili anche in un clone senza `.local-cache/`. Le immagini del gioco usato come riferimento rimangono separate in [references/screenshots](../references/README.md).

## Immagine del README

[`colonia.png`](colonia.png) è un render **1240×850** della vera finestra Avalonia, prodotto dal test `SidebarNavigationAndSummaryCardsKeepConstructionDetailsInReadingOrder` durante la validazione del **2026-10-27**.

- Run: `899f6ff3e8ff41288330c3d26b4c4ea7`, **206/206 test superati**.
- Sorgente: `.local-cache/m4/runs/899f6ff3e8ff41288330c3d26b4c4ea7/screenshots/structure-details-1240.png`.
- Scena di test: crescita civile in pausa, 8 abitanti su 16 posti, 4 liberi, Taglialegna selezionato dopo costruzione e assegnazione.
- Mostra menu da 240 px, card con icona abitanti, costi e produzione con icone. Copia integrale del PNG, senza ritagli o ritocchi.
- SHA-256 verificata: `d1f1f212df75141d0415cf1ba7b1b6109da3939502a6f1d007512466862ed386`.

È incorporata subito dopo il titolo del [README principale](../../README.md). I dati mostrati descrivono la scena di test, non la dotazione di una nuova partita.

## Come aggiornarla

1. Eseguire `.\validate.cmd` dalla radice e verificare il codice di uscita e `.local-cache/m4/validation.json`.
2. Leggere `testResults` nel riepilogo: la cartella sorella `screenshots` contiene i render dello stesso run. `structure-details-1240.png` è un render aggiuntivo e non compare nell'elenco dei 17 screenshot obbligatori del manifest.
3. Aprire il PNG e controllare che rappresenti l'interfaccia corrente senza testo sovrapposto o comandi tagliati.
4. Copiarlo qui mantenendo il nome `colonia.png`, così il collegamento nel README resta stabile.
5. Aggiornare data, run, scena e [rapporto di verifica](../validation/DESKTOP_UI.md). Non sostituire o modificare le immagini storiche di riferimento.
