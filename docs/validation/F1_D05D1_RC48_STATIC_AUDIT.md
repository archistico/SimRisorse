# RC48 — Audit statico e differenze rispetto alla RC47

**Data:** 5 novembre 2026. **Gate Windows:** RC47 **620 test confermati dall'utente**; RC48 **631 attesi, non eseguiti**. L'SDK .NET 10 non è disponibile in questo ambiente: non è possibile certificare compilazione o xUnit.

## Confronto dei sorgenti

- Archivio RC47 originale: **604 file**, ZIP verificato senza errori.
- File C# esistenti modificati: **zero** (confronto byte per byte sui 195 sorgenti C# precedenti).
- File C# nuovi: **due** — `src/SimRisorse.Core/Food/FoodIndustryTickPlanner.cs` e `tests/SimRisorse.Core.Tests/FoodIndustryTickPlannerTests.cs`.
- Nuovi test: **11** metodi `[Fact]`, da 620 passati in RC47 a **631 attesi**. La quantità complessiva dei casi parametrizzati rimane competenza di xUnit, non di un semplice conteggio di attributi.
- Nessun aggiornamento a Desktop/Avalonia, Infrastructure/SQLite, Application, cataloghi attivi, file di progetto, script di avvio, salvataggi o coefficienti economici.

## Controlli statici

- XML/AXAML/SLNX/CSPROJ/PROPS: **26 documenti analizzati**, nessuna anomalia.
- JSON: **39 file analizzati**, nessun errore di parsing.
- Markdown: collegamenti locali analizzati, **nessun link mancante**.
- Manifest SHA-256 completo dei file di progetto (`MANIFEST_RC48_SHA256.txt`) generato dopo la documentazione.
- Lettura integrale dello ZIP della RC48 e confronto del manifest da eseguire dopo la creazione finale dell'archivio (non sono verifiche .NET).

## Limiti della consegna

Il coordinatore è un dominio puro, non una nuova campagna giocabile. `Animals` e `Transit` non sono serializzati nel formato v15 RC47; il loro stato **non può essere salvato** nel formato corrente. Nessun percorso di produzione della v14 richiama il nuovo planner. Non viene eseguita nessuna migrazione SQLite reale.

[Contratto](../design/F1_D05D1_RC48_COORDINAMENTO_FILIERA.md) · [Piano Windows](F1_D05D1_RC48_TEST_PLAN.md) · [Stato](../CURRENT_STATE.md).
