# G1-D19 — Matrice di parità funzionale/visiva GUI v13 → v14

**Obiettivo:** bloccare regressioni della v14 rispetto a `MainWindow` v13. **Stato della matrice:** checklist documentale; **nessuna casella è considerata validata su Windows per RC3–RC8** finché l'utente non conferma il confronto. Vale a 1040×740 e 1240×850.

| ID | Area | Comportamento della GUI v13 da preservare / estensione consentita v14 | Evidenza RC3–RC5 | Gate |
| --- | --- | --- | --- | --- |
| UI-01 | Navigazione | Edifici, Risorse, Esercito invariati; Scuola e Albero aggiunti nella **sidebar**, non tabs nuove | Codice candidato | [ ] |
| UI-02 | Risorse colonne | **Sette** colonne con ordine/intestazioni originali, senza sostituzioni | Codice RC3 usa `ResourceRow` | [ ] |
| UI-03 | Risorse numeri | Quantità arrotondata in tabella, valori precisi disponibili; numeri allineati a destra | Da confrontare | [ ] |
| UI-04 | Capacità | Tooltip con totale e ripartizione Municipio/Magazzino/edifici, Pane incluso | RC13 corregge tooltip Matite e aggiunge 2 test; **screenshot necessario** | [ ] |
| UI-05 | Flussi e tempi | Produzione/Consumo/Saldo; `Si riempie tra` `hh:mm:ss`, casi capacità piena e saldo ≤ 0 | Codice candidato | [ ] |
| UI-06 | Risorse layout | Ordinamento alfabetico, selezione ocra, hover stabile, colonna finale non coperta da scrollbar | Audit manuale necessario | [ ] |
| UI-07 | Edifici elenco | Selezione e stato, quantità/personale, icone, stessa spaziatura e scroll | Audit manuale necessario | [ ] |
| UI-08 | Livelli | Base/Grande/Avanzato sono **edifici distinti** nell'elenco, nessuna ComboBox livello | Codice candidato | [ ] |
| UI-09 | Costruzione bloccata | Prerequisiti mancanti ⇒ **nessun pulsante Costruisci visibile**, ma requisiti leggibili | RC8: bloccati attenuati, selezionabili con requisiti leggibili; test predisposti | [ ] |
| UI-10 | Mancanza materiali | Pulsanti visibili ma **disabilitati**, elenco richiesto/disponibile; nessun addebito al clic | RC8: disabilitazione `IsEnabled` esplicita per +1/+5/+10 e testo deficit; Windows da confrontare | [ ] |
| UI-11 | Multipli | +1/+5/+10, prezzo **cumulativo crescente** per acquisto di famiglia | RC8: +5/+10 sulla riga secondaria con lavoratori; prezzo cumulativo e tooltip; Windows pendente | [ ] |
| UI-12 | Dettaglio | Costi, produzione, consumo, capacità con icona e font uniforme; upgrade separato | RC5: upgrade sbloccato senza materiali resta visibile/disabilitato; audit manuale necessario | [ ] |
| UI-13 | Pane e produzione | Diagnosi di filiera, input, saturazione e deposito corretto; non confondere Deposito alimentare e Magazzino | **RC4: Pane; RC6: diagnostica produttiva da planner condiviso, codice/test predisposti, Windows pendente** | [ ] |
| UI-14 | Crescita | Residenti/capienza, velocità di crescita, fabbisogni Cibo/Pane, blocchi | Codice ripristinato, audit mancante | [ ] |
| UI-15 | Esercito | Caserme, Spade, slot, reclute/soldati, progresso individuale, arruola e annulla | Codice ripristinato, audit mancante | [ ] |
| UI-16 | Scuola | Iscrizione, coda, formazione, ricercatori, progressi senza doppia occupazione | RC8: costruzione solo da Edifici; tab Scuola senza Costruisci, iscrizione/ricercatori mantenuti; Windows pendente | [ ] |
| UI-17 | Albero tecnologico | Saldo libero, acquisto volontario, costo/prerequisiti, tecnologie senza effetto bloccate | Estensione v14 candidata | [ ] |
| UI-18 | Input visivi | Hover pulsanti non muove layout, disabilitato grigio distinto, focus leggibile, testo non tagliato | Audit manuale necessario | [ ] |
| UI-19 | Stato e selezione | Dopo tick/ri-render la risorsa, edificio e pagina selezionati restano coerenti | Test previsti, audit necessario | [ ] |
| UI-20 | Footer e comandi | Salva/Carica/Nuova partita con conferma/pausa-riprendi; messaggi e allarmi | Audit manuale necessario | [ ] |
| UI-21 | Salvataggi | V13/v14 separati; rifiuto di file errati, chiusura sempre possibile, round-trip stato | Test automatici + manuale | [ ] |
| UI-22 | Invarianti | Nessun lavoratore, studente, ricercatore o soldato duplicato; prezzi/scorte coerenti | Test Core esistenti, verifica UI | [ ] |

## Protocollo di prova

1. Da due avvii separati (`./run-game.cmd` e `./run-game.cmd --experimental-v14`) acquisire **coppie di screenshot** della stessa scheda/stato dove confrontabile; usare gli stessi valori di scala e dimensioni finestra.
2. Completare la matrice con **pass/fail, versione, scenario, azioni riproducibili, screenshot** e log; un comportamento diverso per una meccanica nuova va spiegato e approvato, non nascosto.
3. Prima i gate Risorse / Edifici / Esercito / Pane, poi quelli esclusivi Scuola / Albero. Non usare dati personali dell'utente come fixture automatica.
4. Se un gate fallisce, aprire un caso riproducibile e aggiungere un test di regressione; fermare E4.

## Debito noto

Le due finestre usano ancora XAML e ViewModel separati: un cambio di stile/funzionalità fatto solo su una può riaprire divergenze. **La RC8 non certifica ancora la parità**. La diagnosi approfondita Pane con `BreadSupplyProjector` è stata collegata nella RC4, corretti in RC5 preventivi/upgrade e aggiunta in RC6 la diagnostica produttiva della GUI v14, ma i comportamenti reali e il layout devono ancora essere confrontati su Windows. Gli altri gate non sono dichiarati passati.

[Roadmap](../ROADMAP.md) · [Handoff](../HANDOFF.md) · [Piano originale RC3](V60_E3B4_RC3_PARITY_TEST_PLAN.md) · [contratto pixel art](../PIXEL_ART_UI.md).
