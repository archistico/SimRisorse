# G1-D23 / RC7 — Audit statico e perimetro del pacchetto

**Data:** 03/11/2026. **Baseline:** ZIP integrale `SimRisorse-v60-E3b4-RC6-G1-D22-diagnostica-produzione.zip`. **Nuovo candidato:** RC7.

## Modifiche di codice

- **Unico file C# modificato:** `tests/SimRisorse.Desktop.Tests/V14UiParityRegressionTests.cs`.
- **4 asserzioni nella prova mirata:** mantenuti i controlli di visibilità; aggiunti due controlli sull'oggetto `Command` associato al pulsante; sostituiti i due controlli `.IsEnabled` con `.IsEffectivelyEnabled`.
- **Test xUnit aggiunti/rimossi:** 0. **Totale atteso: 451**, da eseguire nuovamente su Windows.
- **File runtime `src/`: 0 modificati**. Nessun intervento a `.axaml`, `.csproj`, database SQLite, salvataggi, cataloghi e scenari. **Nessun file eliminato**.

## Verifiche automatiche eseguite nell'ambiente di confezionamento

- Confronto byte-per-byte con lo ZIP RC6 delle sorgenti originali; nessun file runtime diverso.
- Parsing di **26** file XML/XAML/progetti e **39** JSON completato senza errori.
- Controllo dei link Markdown relativi nei documenti modificati e nuovi: nessun percorso mancante.
- Controllo sorgente sul test modificato: entrambe le asserzioni `IsEffectivelyEnabled` e i riferimenti ai due comandi presenti; vecchie asserzioni errate rimosse.

Queste verifiche **non equivalgono a compilazione o esecuzione dei test .NET**; il toolchain .NET 10 non è installato in questo ambiente. La conferma spetta al run Windows con `./validate.cmd` e alla matrice GUI P0.

[Rapporto D23](../design/G1_D23_E3B4_RC7_AVALONIA_TEST_ENABLED.md) · [Piano RC7](G1_D23_RC7_TEST_PLAN.md).
