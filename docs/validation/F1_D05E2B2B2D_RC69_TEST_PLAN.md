# RC69 — Piano di verifica su Windows

**Stato:** non eseguito. **Attesi 814 test cumulativi** = RC68 806 + Application 6 + Infrastructure 2.

1. ` .\validate.cmd ` da radice progetto (in PowerShell ` .\validate.cmd `). Registrare TRX, errori `CSxxxx`, test riusciti/falliti/skipped e tempi.
2. Avviare ` .\run-game.cmd ` (v15) e ` .\run-game.cmd --v14 ` (v14). Confrontare tutti i punti GUI ADR 0071; la RC69 non modifica nessun sorgente Desktop.
3. Test Application: radice civica realmente nativa; alias dello stesso oggetto; famiglie/contatori v15; errore atomico; conversione storica esplicita; rifiuto di eliminare un `food-store` ancora occupato.
4. Test Infrastructure: round-trip al millesimo e generazione fisica 6 con 45 righe produttive e 0 `food-store` persistiti.
5. Stress combinato: `dotnet run --project tools/SimRisorse.Simulation -c Release --no-restore -- --audit-v15 --seconds 120 --profile combined`. `ImbalancedSeconds` deve essere 0, `LegacyProducedFoodMilliunits` deve essere 0; registrare tutti i valori.
6. Ricontrollare importazione opt-in v14, errori su schema non supportato e salvataggi v14 separati. Non spostare, sovrascrivere o eliminare file v14.

**Gate RC69:** nessun errore .NET, suite completa verde, equivalenza degli stock e della GUI. Se test rossi, correggere la RC69 prima di avviare RC70. [Stato](../CURRENT_STATE.md).
