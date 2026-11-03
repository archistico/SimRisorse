# Piano di test — RC13 / G1-D29

**Gate iniziale:** RC12 dichiarata verde dall'utente (**468 test attesi**, numero effettivo non allegato); RC13 aggiunge **2 test Desktop**, quindi **470 attesi**. In questo ambiente manca .NET SDK 10: xUnit **non eseguito**.

## Verifiche automatiche su Windows

1. Estrarre in nuova cartella, eseguire `./validate.cmd`; conservare riepilogo e `.trx`. Verificare 470 test con 0 failure.
2. `PencilCapacityTooltipShowsOwnedBuildingsAndAvailableSpace`: colonia iniziale ⇒ 1 Municipio, 0 Magazzini, 0 Scuole, capienza 1.000 Matite e spazio libero 1.000; non contare edifici non posseduti.
3. `PencilCapacityTooltipRecalculatesForWarehousesAndSchoolsWithoutLosingResourceSelection`: checkpoint 1 Municipio + 2 Magazzini + 1 Scuola ⇒ 2.050 capacità; con 10 Matite resta 2.040 spazio libero; `SelectedResource` preservato dopo Load.
4. Ripetere test preesistenti RC12 su ricerca, Scuola/Matite, Edifici e worker batch, Pane, SQLite e finestre headless.

## Verifiche manuali P0 (non sostituite da xUnit)

- Avviare `./run-game.cmd` e `./run-game.cmd --experimental-v14` con backup dei salvataggi.
- Risorse v14: selezionare Matite e passare sulla colonna **Capacità**; verificare totale e composizione reale. Aggiungere un Magazzino/Scuola in partita e confrontare la variazione del tooltip senza perdere la selezione.
- Verificare risorse originali (in particolare Pane) con la stessa tabella a sette colonne; nessuna differenza visiva indesiderata con v13.
- Aprire Edifici, Esercito, Scuola e Albero; seguire la [matrice D29](G1_D29_AUDIT_GUI_V13_V14.md) alle due risoluzioni **1040×740 e 1240×850**.
- Annotare per ogni voce UI-01…UI-22 **OK / FAIL / N.A. motivato** e screenshot o riproduzione; **non** chiudere P0 senza questo passaggio.

## Criterio

Suite verde su Windows + tooltip corretto e selezione stabile, senza regressioni di build. **RC13 è una patch mirata e una base di audit: non certifica da sola la grafica.** E4 non avviata.
