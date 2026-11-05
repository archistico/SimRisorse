# Piano di validazione Windows — RC44 / F1-D05a

**Baseline confermata:** RC42 (577 test). **RC43:** 590 test attesi non ancora eseguiti. **RC44:** 10 nuovi test, **600 complessivi attesi**, non verificati in questo ambiente.

Dalla cartella del progetto estratto, con SDK .NET 10:

```powershell
.\validate.cmd
```

Verificare in particolare:

- `SimRisorse.Application.Tests.F1D05FoodMigrationTests`: 8 test su inventario v14 iniziale, valori frazionari, Pane grezzo, inventario senza alimento, capienza insufficiente e sufficiente, rifiuto alterazioni, rifiuto formato errato.
- `SimRisorse.Infrastructure.Tests.F1D05SqliteFoodPreviewTests`: 2 test con database temporaneo: anteprima letta da SQLite v14 identica allo stock sorgente, bytes e elenco file invariati; assenza di checkpoint non crea nulla.
- La suite RC43 `UnifiedFoodSupplyTests`: richiede i 13 nuovi casi per ricezione/lavorazione/razioni, inclusi Pane e alimenti di filiere nuove.

Non eseguire la pre-migrazione su un salvataggio reale per effettuare il passaggio di versione: `PreviewFoodMigration` è solo un'API di lettura per l'applicazione/test. Nessun comando di migrazione v15 è attualmente disponibile.

Controlli manuali facoltativi, usando una **copia** della partita: lanciare `run-game.cmd`, controllare Salva/Carica v14, confermare che Scuola, Ricerca, Cibo/Pane e lavoratori operino con le regole precedenti. Il file utente v14 non deve essere modificato dall'anteprima. La v13 `--legacy-v13` deve continuare ad avere DB separato.

**Gate:** 600 test superati, nessuna regressione del gioco, review della nuova versione SQLite prima di introdurre D05b. Un fallimento impedisce di dichiarare F1-D05a validata.
