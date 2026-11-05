# RC54 — F1-D05d.4b.1 · Collegamento GUI v15 (opt-in)

**Data:** 5 novembre 2026. **Base:** RC53, validata dall'utente con **705 test passati**. **RC54:** +8 test xUnit, **713 attesi**, non eseguiti qui (SDK .NET 10 assente).

## Perimetro realmente implementato

L'eseguibile Avalonia accetta `--v15` e crea `V15PreviewWindow` + `V15PreviewViewModel`, alimentati da `V15FoodIndustryCampaignSession`, `V15UnifiedCampaignClock`, `V15CampaignCatalogProjection` e `V15SqliteFoodSaveStore`. Senza opzioni, `run-game.cmd` continua a eseguire la GUI **v14 originale** e il suo checkpoint esistente. `--legacy-v13` rimane esplicito. Le opzioni sono reciprocamente esclusive e un percorso di salvataggio riservato a un altro motore è rifiutato.

La nuova finestra usa gli stessi colori, font monospaziato, sidebar, pannelli e stile pixel art. Navigazione: **Edifici, Risorse, Esercito, Scuola, Albero tecnologico**. Il catalogo presenta le sei filiere con sprite 8×8 specifici e i produttori Base, Grande e Avanzato come **righe indipendenti**, senza ComboBox. Nessun pulsante Costruisci è mostrato quando il prerequisito/ricerca manca; quando è sbloccato ma non ci sono materiali, è disabilitato. Costo e stato provengono da una **sessione di prova non persistente** (nessuna duplicazione delle regole).

Azioni collegate al checkpoint v15: costruzione +1/+5, avanzamento tier, personale +1/−1, Scuola, Matite, studenti, ricercatori, acquisto ricerca, reclute e annullamento. Tutti gli addetti usano un solo budget e i comandi fanno commit tramite il Core. Il clock v15 alimenta le nuove filiere e gli altri sistemi senza richiamare il vecchio gioco v14.

La pagina Risorse mantiene **esattamente sette colonne** nell'ordine: Risorsa, Quantità, Capacità, Produzione/s, Consumo/s, Saldo/s, Si riempie tra. La quantità è arrotondata visivamente all'unità (senza perdere precisione di salvataggio), le capacità sono aggregate dai contenitori fisici e il Cibo è una sola riga del Deposito. Le colonne produzione/consumo/saldo/ETA mostrano **«—»** perché **manca ancora una proiezione lorda affidabile dei flussi v15**. Questo è un limite deliberato e visibile, non una simulazione falsificata. L'implementazione delle quattro colonne di flusso è programmata per **F1-D05d.4b.2**.

## Persistenza e sicurezza

- `run-game.cmd --v15` usa per default `%LOCALAPPDATA%\SimRisorse\experiments\civil-growth-v15.sqlite3`.
- `run-game.cmd` senza argomenti conserva `%LOCALAPPDATA%\SimRisorse\experiments\civil-growth-v14.sqlite3`.
- La v15 non apre né migra automaticamente il DB v14; SQLite `user_version=5` invariato.
- Nuova partita: conferma esplicita; il vecchio file resta integro finché non viene eseguito **Salva v15**. L'autosave rimane sospeso dopo la conferma finché il salvataggio manuale non ha successo.
- File v15 danneggiato o con versione sconosciuta: caricamento bloccato e autosave disabilitato; non sovrascrivere silenziosamente.
- Apertura e recupero sono in pausa. I tick e le operazioni sono eseguiti nel thread UI, senza contemporaneità sullo stesso checkpoint.

## Invarianti non cambiati

Cibo pronto solo nel Deposito; Pesce e Pane possono transitare dalla città; Bovini/Selvaggina/Carne/Frutta/Verdura appartengono ai rispettivi compartimenti. `food-store` è nascosto nel catalogo e spento nel clock v15 ma **rimane nello snapshot ereditato**. La Scuola addebita Cibo pronto nel Deposito. Nessuna modifica ai costi +15%, requisiti minimi Zecca, schema SQLite, UI XAML v14 o regole del motore v14.

## Comandi Windows

```powershell
.\validate.cmd
.\run-game.cmd
.\run-game.cmd --v15
.\run-game.cmd --v15 --save-path "$env:TEMP\SimRisorse-v15-test.sqlite3"
```

Non usare direttamente il vecchio DB personale con `--v15`. I costi di filiera e le ricette rimangono valori candidati da testare e ribilanciare dopo E4a.
