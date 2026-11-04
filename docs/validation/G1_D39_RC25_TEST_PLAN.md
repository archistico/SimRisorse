# G1-D39-F1 / RC25 — Piano di validazione Windows

**Baseline confermata:** RC20, 490 test passati. **RC24:** build Desktop.Tests fallita per CS1061 alle righe 71 e 136 di `V14BuildingUpgradeAuditTests.cs`. **RC25:** corretto l'import `using Avalonia.Headless;` nello stesso file; **501 test attesi**, non ancora eseguiti.

1. Estrarre lo ZIP RC25 in una nuova directory senza sovrascrivere le RC precedenti. Effettuare una copia del salvataggio v14 prima delle prove manuali.
2. Eseguire `./validate.cmd` dalla root. Obiettivo: compilazione `SimRisorse.Desktop.Tests` senza errori `MouseMove`, quindi **501/501**. Copiare il TRX e i log se compaiono altri errori: la correzione non equivale a una validazione Windows.
3. Controllare specificamente `V14BuildingUpgradeAuditTests`: i pulsanti di upgrade tecnologicamente sbloccati ma non pagabili restano visibili, grigi e non cliccabili, e non si spostano all'hover alle viewport 1040×740 e 1240×850; con risorse sufficienti risultano verdi, hover ocra, acquisto e selezione coerenti.
4. Eseguire il confronto visivo completo delle [22 voci P0](G1_D39_P0_UI_MATRIX.md) e includere i flussi Nuova partita/Salva-Carica RC17 su salvataggi di prova; xUnit verde non chiude da solo P0.

**Invariati:** GUI v13, GUI v14 e motore, SQLite v2/v3, regole economiche, contatori di test. **Roadmap:** P0 → P1 Scuola e tecnologie → E4a → E4b → E4c → M5.4–M7.

[Report fix](../design/G1_D39_RC25_FIX_CS1061_HEADLESS.md) · [Roadmap](../ROADMAP.md).
