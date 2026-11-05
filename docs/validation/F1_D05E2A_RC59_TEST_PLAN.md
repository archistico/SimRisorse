# RC59 — Piano di validazione F1-D05e.2a

**Baseline validata:** RC58, **744 test passati**. **Nuovi casi RC59:** 11 (Application: 4 `[Fact]` + 5 casi `[Theory]`; Infrastructure: 2 `[Fact]`). **Totale atteso: 755**. Conteggio da confermare eseguendo la suite; nessun test .NET eseguito durante la preparazione del pacchetto.

## Gate ordinario

```powershell
.\validate.cmd
```

Il passaggio richiede build completa, tutti i **755 test** verdi, zero test saltati e assenza di regressioni Desktop. Se il conteggio differisce, raccogliere il riepilogo effettivo invece di considerarlo confermato.

## Scenari mirati

- Verifica del budget occupazionale (corretta nella RC60): 3 lavoratori iniziali + 13 della filiera storica + 6 delle nuove filiere = 22 occupati, 2 persone disponibili su 24.
- Produzione lorda e riconciliazione di Bovini, Selvaggina, Carne, Verdura, Frutta, Pane, Cibo entro quattro tick.
- Invarianza dell'inventario dopo 90 secondi in un solo avanzamento rispetto a 90 avanzamenti da 1 secondo.
- `food-store` storico con lavoratore assegnato: nessun Cibo cittadino, unica produzione del Deposito.
- Cinque ricette del Deposito, esercitate isolatamente (Pesce, Carne, Verdura, Frutta, Pane).
- Round-trip SQLite v15 con tutti i compartimenti, popolazione, esercito, checkpoint e scorte al millesimo.
- Round-trip SQLite in tre intervalli frazionari (20,25 s + 10,5 s + 30 s) contro 60,75 s continui.

## Gate diagnostico facoltativo

```powershell
# Prima prova limitata
dotnet run --project tools/SimRisorse.Simulation -c Release --no-restore -- --audit-v15 --seconds 120 --profile combined

# Prova estesa, separata da validate.cmd
dotnet run --project tools/SimRisorse.Simulation -c Release --no-restore -- --audit-v15 --hours 24 --profile combined
```

Atteso: JSON con `ImbalancedSeconds=0`, `LegacyProducedFoodMilliunits=0`, `SimulatedSeconds=RequestedSeconds`; produzione positiva per ciascuna delle sette voci. Non inventare i valori quantitativi prima dell'esecuzione reale. In presenza di difformità, riportare l'eccezione e la risorsa coinvolta.

## Esclusioni

Nessuna modifica a GUI, prezzi, ricette, schema SQLite generation 5 o migrazione dei vecchi salvataggi. La rimozione fisica di `food-store` dal checkpoint verrà affrontata in D05e.2b dopo la validazione.

[Specifica](../design/F1_D05E2A_RC59_STRESS_FILIERE.md) · [Roadmap](../ROADMAP.md).
