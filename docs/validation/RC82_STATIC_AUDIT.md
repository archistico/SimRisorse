# RC82 — Audit statico prima della consegna (06/11/2026)

## Verifiche effettivamente eseguite nell'ambiente di preparazione

- Archivio sorgente di partenza: `SimRisorse_RC81_E4b2c_zecca_runtime_opt_in_candidato.zip`, letto senza modificare il contenuto originale.
- Diff file per file contro RC81: nessun vecchio file mancante; nessuna modifica ai sorgenti `SimRisorse.Core`, `SimRisorse.Desktop`, `SimRisorse.Infrastructure` o ai loro test. È stato modificato soltanto il runner `V15IndustrialProgressionBenchmark` in Application, il launcher CLI `Program.cs`, il gate F1 e i documenti; aggiunti un comando dedicato e un file xUnit Application.
- Scanner lessicale locale: parentesi tonde, quadre e graffe coerentemente bilanciate nei file C# modificati, ignorando stringhe e commenti.
- Contratto delle azioni: percorso `MintTen` con `staff-mint-team` (assegnazione reale di `V15CrewRules.MintCrewSize`), `Legacy` con `staff-mint`; `ScenarioId` separato e `RulesVersion` esplicito. Il runner fallisce se la modalità salvata cambia o se un'assegnazione dichiarata riuscita non completa una squadra.
- Manifest degli esperimenti: due righe E4B-M01 `not-run` senza tempi; i tempi Legacy citati sono storici RC77, non trasferiti.
- Conteggio statico nuove prove xUnit (metodi Fact + righe InlineData): **17**; atteso 985 + 17 = **1002** nel gate.
- Il gate F1 usa l'API CLI a `--max-seconds 0 --compare-legacy` senza promuovere un run diagnostico a misura naturale. Verifica regime, assenza di completamento dichiarato e nove file di output.

## Non verificato

- **SDK .NET 10 non installato qui**: non sono stati eseguiti `dotnet build`, `dotnet test`, Windows smoke, benchmark completo, report TRX, controllo GUI e fixture SQLite con RC82.
- La validazione statica non dimostra l'assenza di errori C#, la riuscita dei test, la raggiungibilità della Zecca con dieci persone o l'adeguatezza delle risorse/magazzini.
- Ultima evidenza confermata: RC78, **926/926** test. RC79–RC82 restano candidate con **1002 test da verificare**.

**Riferimenti:** [piano](E4B_RC82_TEST_PLAN.md) · [ADR](../adr/0090-benchmark-naturale-zecca-squadre-complete-v15.md) · [roadmap](../ROADMAP.md).
