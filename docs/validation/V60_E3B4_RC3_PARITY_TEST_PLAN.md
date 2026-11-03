# Validazione RC3 — ripristino delle funzioni storiche nella GUI v14

## 1. Compilazione e regressioni

Estrarre in directory nuova e usare `./validate.cmd`. La RC1 aveva 427 test Windows passati; RC2 aggiungeva 3 test; RC3 aggiunge 9 test dedicati: obiettivo **439**, oltre a tutti i controlli del validatore. Non indicare passati finché non viene ricevuto il log.

## 2. Confronto visivo, uno accanto all'altro

Avviare prima `./run-game.cmd` (v13) e poi `./run-game.cmd --experimental-v14` (DB sperimentale separato). Confrontare la scheda Risorse: sette intestazioni, ordinamento alfabetico, destra allineata, tooltip quantità e capacità, hover ocra riga selezionata, spazio per scrollbar, tempi `hh:mm:ss`, capacità Pane identica alle altre risorse. Verificare anche risorsa con saldo negativo e risorsa a capienza massima.

## 3. Edifici

Selezionare Segheria grande all'inizio: nessun pulsante Costruisci, mostrare «Ricerca richiesta: Lavorazioni I» e prezzo. Selezionare Segheria base: Costruisci +1 visibile e abilitato se vi sono materiali; +5/+10 presenti ma disabilitati se non sostenibili. Provare un edificio con prerequisiti civili mancanti. Eseguire più acquisti per verificare prezzo progressivo e stato di lavoratori, quantità e capacità. Il dettaglio non deve contenere ComboBox di livello né di lotto; solo pulsanti espliciti.

## 4. Esercito e popolazione

Verificare Esercito con stesso pannello e legenda della v13, caserme, posti, Spade, formazione individuale, ordini arruola/annulla e congedo disabilitato secondo regole v13. Verificare progressione della crescita e fabbisogno Pane senza salti. La Scuola riserva abitanti che non possono essere assegnati simultaneamente all'esercito.

## 5. Salvataggi

Pausa/riprendi, nuova partita con conferma, salvataggio manuale, autosalvataggio v14 ogni minuto, salvataggio alla chiusura e ripristino. Un database v13 rinominato come v14 deve essere rifiutato e non sovrascritto. All'avvio normale non devono comparire modifiche.

## 6. Comportamenti grafici

Dimensione minima 1040×740; scroll senza sovrapposizioni, selezione persistente, hover senza spostamenti, pulsanti disabilitati distinguibili, testo leggibile, nessun elemento coperto dal footer. Il test headless non sostituisce questo passaggio.

- [ ] Con file sperimentale v13/incompatibile, la GUI v14 non sovrascrive il file e **si chiude senza rimanere bloccata**.
