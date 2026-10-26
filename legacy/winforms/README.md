# Prototipo Windows Forms

Questa cartella conserva la bozza originale, trasferita qui il 2026-10-26. Soluzione, sorgenti, risorse dei form e test sono mantenuti senza modifiche al contenuto. Il nuovo progetto è descritto nel [README principale](../../README.md).

## Contenuto

- [Gestionale.sln](Gestionale.sln): soluzione storica.
- [Gestionale/Gestionale.csproj](Gestionale/Gestionale.csproj): applicazione Windows Forms su `net6.0-windows`.
- [TestProject1/TestGestionale.csproj](TestProject1/TestGestionale.csproj): test NUnit storici.

I riferimenti relativi tra soluzione e progetti sono rimasti gli stessi. Gli output `bin/` e `obj/`, le preferenze `.csproj.user` e la cache Visual Studio sono stati raccolti in `.local-cache/` alla radice, ignorata da Git. Gli output si devono rigenerare quando necessario.

## Apertura ed esecuzione

Aprire `legacy/winforms/Gestionale.sln` in un ambiente Windows con strumenti compatibili con il target storico. Il target .NET 6 appartiene alla bozza e non alla proposta per la nuova applicazione.

Comandi dalla radice del repository, se l'ambiente dispone di SDK, pacchetti di targeting e runtime richiesti:

```powershell
dotnet build .\legacy\winforms\Gestionale.sln
dotnet test .\legacy\winforms\TestProject1\TestGestionale.csproj
dotnet run --project .\legacy\winforms\Gestionale\Gestionale.csproj
```

Questi comandi documentano i percorsi aggiornati; build e test non sono stati eseguiti durante il riordino documentale. Il funzionamento su Linux o macOS non è previsto per questa applicazione Windows Forms.

## Limiti osservati

- `Costruzione` usa l'orologio del sistema per calcolare i cicli totali dalla creazione.
- I pulsanti di `Form1` aggiungono edifici senza verificarne o pagarne il costo.
- `Magazzino` può accreditare output anche se gli input non sono sufficienti e non applica coerentemente la capacità.
- La simulazione viene coordinata dal timer dell'interfaccia.
- I due test controllano valori assegnati e nomi delle risorse, senza verificare le regole economiche.
- `Form2` e il controllo `ElementoRisorse` restano conservati come parte della bozza; non sono componenti già scelti per la nuova UI.

Il prototipo è una fonte di idee e contenuti, non la specifica del comportamento corretto. La nuova soluzione non dovrà dipendere dai suoi assembly.
