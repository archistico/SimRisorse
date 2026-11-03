# G1-D22 — RC6: diagnostica produttiva degli Edifici nella GUI v14

**Data:** 3 novembre 2026 · **Stato:** patch P0 candidata, test .NET e controllo visivo Windows non eseguiti nell’ambiente di produzione. **Base:** RC5/G1-D21, anch’essa non ancora validata dall’utente (447 casi attesi). L’ultima conferma utente resta RC4.

## Difetto individuato

La GUI classica v13 descriveva, per gli edifici produttivi già posseduti, quanti addetti possono lavorare nel prossimo secondo e perché gli altri non lavorano: materie prime, spazio e priorità della filiera. Nella RC5 v14 `RefreshBuildingEntries` mostrava soltanto `N/M lavoratori`, con una diagnosi limitata a **nessun lavoratore** o **prezzo del prossimo edificio**, anche quando il motore aveva già rilevato produzione bloccata o parziale.

## Modifica effettuata

- `src/SimRisorse.Desktop/Presentation/V14PreviewViewModel.Buildings.cs`: leggere `V14EconomyPreview.Buildings` già calcolato da `ForecastEconomy()` prima del refresh Edifici. Evitare un secondo pianificatore e qualsiasi modifica alle scorte.
- Se un livello posseduto ha lavoratori, riportare lo **stato aggregato della filiera**: runnable/assigned, input mancanti o impegnati da produttori con priorità, output senza sufficiente capacità per il prossimo lotto, quantità posseduta/capienza/spazio libero e dimensione del lotto.
- La proiezione del Core riunisce **Base, Grande e Avanzato nella stessa famiglia** per le risorse. La GUI lo precisa chiaramente: non attribuisce a un singolo tier un numero di addetti effettivamente attivi che il motore non fornisce separatamente.
- Conservare contemporaneamente la diagnosi di **materiali mancanti per costruire il prossimo edificio** e quella di **produzione insufficiente negli edifici già costruiti**. Lavoratori mancanti indicati per il livello selezionato.
- Per il Pane: specificare che il Deposito alimentare immagazzina Cibo ma **non Pane**, e che capacità e lotti si risolvono con Forni, Magazzini o capacità del Municipio. Il Magazzino continua a servire ogni risorsa.
- Quattro test Desktop su materia prima iniziale della Segheria, capacità esaurita per Pane, doppio blocco costruzione/produzione, visibilità pannello di diagnosi Avalonia.

## Superficie modificata

**Codice modificato:** soltanto il partial ViewModel Edifici v14 e i test Desktop. **Invariati:** `MainWindow.axaml`, `MainViewModel.cs`, `V14PreviewWindow.axaml`, `ResourceRow.cs`, simulazione Core, clock, sessione, economie, risorse e salvataggi SQLite v2/v3. Nessun ADR nuovo: applica il requisito GUI già approvato da ADR 0033.

## Validazione / limiti

- RC4: test confermati **passati** dall’utente, numero non comunicato.
- RC5: **447 attesi**, nessuna esecuzione confermata.
- RC6: +4 test Desktop, **451 attesi** se la discovery non cambia; verifica `.\validate.cmd` e GUI Windows pendenti.
- Questa patch non prova la parità visiva v13/v14; la matrice resta aperta. Nessun test è stato dichiarato superato senza esecuzione reale.

[Prove D22](../validation/G1_D22_RC6_TEST_PLAN.md) · [Matrice P0](../validation/G1_D19_MATRICE_PARITA_GUI.md) · [Roadmap](../ROADMAP.md).
