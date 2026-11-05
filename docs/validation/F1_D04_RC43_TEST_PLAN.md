# Piano di validazione — RC43 / F1-D04

Baseline confermata dall'utente: **RC42 577/577 test** e Salva operativo. RC43 introduce **13** test Core, **590 attesi**. Nessuna compilazione o suite xUnit eseguita nell'ambiente di consegna.

## Windows / .NET 10

Da una nuova cartella estratta:

```powershell
.\validate.cmd
```

Risultato richiesto: **590/590**, zero warning/errori di compilazione non previsti. Conservare log/TRX e segnalare l'intero stack trace di qualunque fallimento. La RC43 comprende cumulativamente tutte le RC precedenti.

### Test di dominio prioritari

- Catalogo completo Pesce/Pane/Carne/Frutta/Verdura, nessun input improprio (`cattle`, `game`, `food`).
- Acquisizione nel deposito e decurtazione dell'origine senza perdita o duplicazione; risorse non alimentari invariate.
- Deposito senza lavoratori o con Cibo pronto saturo: nessuna conversione; razioni già disponibili ancora servibili.
- Pane riservato ai residenti avanzati: non convertibile oltre la quota disponibile; razione Pane e Cibo preparato consumate entrambe o nessuna.
- Capacità grezze e sovracapacità storica: consegne limitate, residui mantenuti, nessuno scarto.
- Priorità deterministiche, stessi risultati indipendentemente dall'ordine delle ricette.
- Orto/Frutteto e Stalla/Caccia/Macellaio alimentano il deposito soltanto dopo la produzione reale dei loro output intermedi.
- Vecchi test di salvataggio e backup RC42 ancora verdi: nessuna modifica a SQLite o regole della partita.

## Controllo manuale non distruttivo

```powershell
.\run-game.cmd
```

La UI v14 corrente **non deve ancora** mostrare né permettere di costruire Stalla/Cacciatore/Macellaio/Orto/Frutteto. La nuova filiera resta isolata dal gameplay fino alla migrazione F1-D05. Verificare che la partita e il Salva continuino a funzionare su una copia del salvataggio; non cancellare vecchi backup.

## Exit

RC43 si considera validata solo quando la suite Windows è verde. Il gate di **attivazione della riforma alimentare F1** resta aperto fino alla migrazione e ai test end-to-end F1-D05. Le rese economiche restano provvisorie.
