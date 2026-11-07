# RC93 — Piano di validazione persistenza del finanziamento isolato

**RC93.1:** corretto `CS0246` aggiungendo `using SimRisorse.Core.Construction;` al file `E4bCurveAFundingSqliteStore.cs`. Il primo tentativo di compilazione RC93 si è fermato in Infrastructure su quel riferimento. Verificare prima `dotnet build`, poi i 17 test RC93 e infine il gate integrale (1220 attesi); non dichiarare superata la build fino al run effettivo.

## Ambito

- Codice in `src/SimRisorse.Application/Benchmarks/E4bFundingCheckpoint.cs`, `E4bCurveAFundingSandbox.cs`, `src/SimRisorse.Core/Construction/ProgressiveConstructionEscrow.cs`, `src/SimRisorse.Infrastructure/Experimental/E4bCurveAFundingSqliteStore.cs`.
- **Nessuna integrazione** con SQLite v15 generation 6, runtime di gioco, GUI, prezzi attivi, produzione o politica `MintTen`.
- Lo store ha application_id diverso e rifiuta file estranei. Il database isolato deve risiedere in una directory esplicitamente creata e scelta per l'esperimento.

## Prove riproducibili su Windows (.NET 10)

```powershell
# Dalla radice del repository
 dotnet test tests/SimRisorse.Application.Tests -c Release --filter FullyQualifiedName~Rc93FundingCheckpointTests
 dotnet test tests/SimRisorse.Infrastructure.Tests -c Release --filter FullyQualifiedName~Rc93FundingSqliteTests
 .\validate-f1.cmd
```

**Atteso:** 5 test Application + 12 Infrastructure RC93 = **17 nuovi**, per un totale cumulativo di **1220**. RC90.3 1167 confermati; RC91.3 1188 e RC92 1203 da confermare. Non segnare verde prima del run effettivo.

## Invarianti critiche

1. Ogni richiesta riuscita aumenta la revisione una volta, scrive checkpoint e ricevuta nella stessa transazione; un crash prima del commit non lascia un deposito parziale.
2. Ritentare la stessa richiesta con payload identico (anche dopo un riavvio e dopo altre operazioni) non cambia scorte o contatori; una richiesta con ID riutilizzato e payload diverso non è accettata.
3. Una revisione obsoleta non sovrascrive una revisione più recente.
4. Le richieste respinte non prenotano l'ID, non mutano stock e non producono ricevute.
5. Quote congelate, deposito e contatore si ricostruiscono correttamente; quote/saldi falsificati o stock sopra capienza sono rifiutati.
6. Senza capienza per il rimborso il deposito resta nel database; con capienza il rimborso avviene una sola volta.
7. Lo store isolato rifiuta database v15 e file di altri formati senza migrare o cancellare dati.
8. Il completamento nella sandbox **non è** costruzione della Zecca nel gioco. Tutti i rifornimenti sono ipotetici.

## Evidenze da conservare

TRX, risultato F1, eventuali PNG per UI storica e primo errore di compilazione/xUnit. Non modificare test per aggirare regole reali. Prima della promozione economica controllare separatamente capienza, prerequisiti, popolazione, ricerca e produzione naturale.
