# Piano di validazione RC61 — SQLite generation 6

**Baseline confermata:** RC60, 756 test passati. **RC61 candidata:** +5 test Infrastructure, totale previsto **761**. Nessuna compilazione .NET 10 eseguita durante la preparazione.

## Gate Windows

```powershell
.\validate.cmd
# Solo dopo i test verdi, diagnostica opzionale combinata:
dotnet run --project tools/SimRisorse.Simulation -c Release --no-restore -- --audit-v15 --seconds 120 --profile combined
```

Test nuovi in `tests/SimRisorse.Infrastructure.Tests/F1D05e2bV15Generation6Tests.cs`:

1. SQLite `user_version=6`, 45 righe di tier attivi, assenza di `food-store`, round-trip di città/deposito/industria senza alterazioni.
2. Salvataggio di `food-store` ancora occupato rifiutato **prima** di creare il DB.
3. `RetireLegacyFoodStore()` esplicito, round-trip successivo e conservazione del contatore storico degli acquisti.
4. Vincolo SQL che vieta di inserire `food-store` nella tabella tier.
5. `user_version=5` rifiutato, senza sostituire né modificare il file esistente.

## Regressioni cumulative

- Tutti i 756 test precedenti (incluse Scuola, militari, salvataggi, stress combinato, GUI v14/v15).
- Controllare manualmente `run-game.cmd` (v14 predefinito, database personale intatto) e `run-game.cmd --v15` con *un nuovo* DB sperimentale gen. 6.
- Il passaggio da un DB sperimentale di gen. 5 deve dare un messaggio di formato non supportato, non una cancellazione né una conversione silenziosa.
- Verificare che la GUI v15 mostri ancora le sette colonne Risorse e i livelli diretti degli edifici.

## Stato dei gate

Nell'ambiente di consegna si possono controllare parsing SQL SQLite, XML/XAML, JSON, link Markdown e integrità ZIP/SHA-256. Questi controlli **non equivalgono** a compilazione C# o all'esito xUnit. La RC61 rimane candidata finché non risulta verde su Windows .NET 10.
