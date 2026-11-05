# RC49 — Controlli statici eseguiti (non sostituiscono .NET 10)

**5 novembre 2026.** Lo stato dichiarato è **candidata**, perché non è stato eseguito `dotnet build`/`dotnet test`.

| Controllo | Esito |
| --- | --- |
| Parsing XML/XAML/SLNX/CSPROJ/PROPS | 26 file, nessun errore |
| Parsing JSON | 39 file, nessun errore |
| Collegamenti Markdown locali | 1.650 verificati, nessun target mancante |
| DDL SQLite interpolato dalla sorgente C# | Eseguito in un SQLite in-memory tramite Python, senza errori |
| Tabelle risultanti | 16, comprese `food_industry_site`, `food_animals_stock` e `food_transit_stock` |
| `PRAGMA user_version` del DDL | 5 |
| Audit strutturale basilare dei sette C# aggiunti/modificati | Parentesi, quadre e graffe bilanciate dopo esclusione di commenti e stringhe |
| Nuovi metodi `[Fact]` | 17: 12 Infrastructure, 5 Core |
| Esecuzione test xUnit/compilazione .NET | **Non effettuata, SDK assente** |

**Limiti dichiarati:** i controlli Python provano la sintassi del DDL e la presenza dei file, non la corretta esecuzione di `Microsoft.Data.Sqlite` né la compilabilità C#. La prova `tick → save → load → tick` è implementata come test xUnit, **non è stata eseguita**. Il formato sperimentale di quarta generazione è intenzionalmente incompatibile.

[Contratto](../design/F1_D05D2_RC49_CHECKPOINT_FILIERA_SQLITE5.md) · [Piano Windows](F1_D05D2_RC49_TEST_PLAN.md) · [Stato](../CURRENT_STATE.md).
