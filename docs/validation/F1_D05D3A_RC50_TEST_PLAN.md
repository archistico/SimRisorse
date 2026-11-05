# RC50 / F1-D05d.3a — Piano di validazione

**5 novembre 2026.** RC47 620 test confermati dall'utente. RC48 +11, RC49 +17 e **RC50 +19 nuovi casi `[Fact]`** non ancora eseguiti in .NET 10. Conteggio indicativo **667** (il numero esatto va letto dall'esito `dotnet test`, inclusi `[Theory]`).

## Da fare su Windows quando sarà possibile

Dalla radice del progetto:

```powershell
.\validate.cmd
```

Per isolare i test nuovi:

```powershell
dotnet test tests/SimRisorse.Application.Tests/SimRisorse.Application.Tests.csproj -c Release --filter FullyQualifiedName~F1D05d3FoodIndustryConstructionTests
dotnet test tests/SimRisorse.Infrastructure.Tests/SimRisorse.Infrastructure.Tests.csproj -c Release --filter FullyQualifiedName~F1D05d3FoodIndustrySessionPersistenceTests
```

Non si è eseguita alcuna compilazione/test RC50 in questo ambiente: SDK .NET 10 assente. **Non dichiarare validato lo ZIP** fino al riscontro Windows. La GUI storica non è modificata; avviandola con `.\run-game.cmd` apparirà ancora la v14.

## I 19 nuovi test coprono

- Sei soli edifici canonici, assenza della duplicazione `food-store`, prerequisiti esistenti e senza cicli.
- Acquisti negati se mancano Fattoria/Pozzo/Pescatore/Deposito/Orto, nessuna modifica di stato.
- Controllo `CheckBuild`/`Build` sullo **stesso prezzo** e costo progressivo arrotondato; nessun acquisto parziale se mancano risorse.
- Posti derivati, personale insufficiente, rilascio oltre gli assegnati, budget di popolazione, nessuna mutazione al rifiuto.
- Magazzini distinti per Bovini, Selvaggina, Carne, Verdura, Frutta, 5 grezzi e Cibo pronto; eccedenze preesistenti conservate.
- Step esplicito: il Cacciatore produce Selvaggina, il Macellaio può usarla solo il tick successivo, il Deposito produce Cibo; non avanza il clock storico.
- Round-trip SQLite generation 5 di edifici acquistati, risorse già pagate, lavoratori e capienze ricostruite; persistenza inalterata dopo un rifiuto.

## Gate di uscita prima di F1-D05d.3b

Tutti i test xUnit compresi quelli storici devono passare. Controllare la corretta distinzione dei job e la validità dei salvataggi di prova. Non avviare alcuna migrazione o conversione sul file dell'utente `%LOCALAPPDATA%\SimRisorse\experiments\civil-growth-v14.sqlite3`.

[Design](../design/F1_D05D3A_RC50_COSTRUZIONI_FILIERA.md) · [ADR 0061](../adr/0061-edifici-alimentari-diretti-e-capacita-derivate.md) · [Stato](../CURRENT_STATE.md).
