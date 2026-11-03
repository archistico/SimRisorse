# Regressione DesktopTests — v38 → v39 (28 ottobre 2026)

## Evidenza ricevuta

Comando eseguito su Windows dall'utente:

```powershell
dotnet test tests/SimRisorse.Desktop.Tests/SimRisorse.Desktop.Tests.csproj -c Release --logger "console;verbosity=normal"
```

- Restore e compilazione di `Core`, `Application`, `Infrastructure`, `Desktop` e `Desktop.Tests`: **riusciti**.
- Riepilogo **della sola suite Desktop**, non di tutta la soluzione: **53 test, 20 superati, 33 falliti, 0 ignorati**, durata test circa 5,1 secondi.
- Tutti e 33 i fallimenti sono `System.InvalidOperationException` con messaggio `The calling thread cannot access this object because a different thread owns it.`
- Lo stack comune attraversa `Avalonia.Media.Brush.get_Transform()` → `Brush.OnReferencedFromCompositor()` → `RenderDataDrawingContext.DrawGlyphRun()` → `TextBlock.RenderCore()` → `Dispatcher.RunJobs()`.
- Gli errori toccano schermate, navigazione, dettagli edifici, salvataggio e risorse: non sono 33 difetti indipendenti, ma un incidente condiviso di rendering.

## Causa individuata nel codice

`src/SimRisorse.Desktop/Presentation/ResourceRow.cs` aveva due campi statici:

```csharp
private static readonly IBrush SurplusBrush = new SolidColorBrush(Color.Parse("#176858"));
private static readonly IBrush DeficitBrush = new SolidColorBrush(Color.Parse("#A44732"));
```

L'oggetto `SolidColorBrush` rimane **mutabile e dipendente dal Dispatcher** che lo ha creato. Il campo `static` ne estende la vita all'intero processo, mentre `Avalonia.Headless.XUnit` (isolamento predefinito **PerTest**) ricrea il Dispatcher per ogni test UI. Le celle saldo usano questi stessi oggetti come `TextBlock.Foreground`, così il compositor prova a leggerli dal Dispatcher sbagliato.

Correzione v39: i campi statici sono `IImmutableBrush`, le istanze sono `ImmutableSolidColorBrush` con **gli stessi colori**. Si mantiene il riuso efficiente, eliminando la dipendenza dal thread. Lo stesso principio era già stato applicato in precedenza alla palette di `PixelGlyph`; una ricerca nell'intero codice Desktop non ha identificato altri `SolidColorBrush` creati in campi statici.

Nuova regressione `RateBrushesAreImmutableAndSafeAcrossHeadlessUiThreads`: verifica entrambi i pennelli, i loro valori cromatici e la leggibilità delle proprietà da un altro thread. **Il test non è stato ancora eseguito in .NET**, non essendo presente l'SDK in questo ambiente.

## Gate Windows da eseguire

Per una diagnosi rapida:

```powershell
dotnet test tests/SimRisorse.Desktop.Tests/SimRisorse.Desktop.Tests.csproj -c Release --logger "console;verbosity=normal"
```

Esito atteso **se la correzione è sufficiente**: 54 test Desktop superati, zero falliti. Non attribuire un esito prima del test.

Poi la validazione completa:

```powershell
.\validate.cmd
```

Totale della soluzione **atteso, non misurato**: 222 casi xUnit (221 in v38 + il nuovo test). L'esito del singolo progetto non equivale alla validazione dell'intera soluzione, che comprende anche verifiche di scenario, benchmark, render e smoke test.

Se compare ancora un'eccezione simile, conservare il **primo stack completo** e il riepilogo per identificare altri oggetti `AvaloniaObject` condivisi tra Dispatcher. Nessuna modifica al motore, alle regole della Forgia, alle capacità, ai salvataggi o alla versione delle regole è stata necessaria in v39.
