# RC83 — Audit statico della candidata (06/11/2026)

**Baseline:** l'utente ha confermato tutta la suite RC82: **1002 test superati**. La RC83 aggiunge 18 nuovi casi xUnit (14 InlineData per due teorie e 4 Fact), quindi il gate prevede **1020**. L'SDK .NET 10 non è installato in questo ambiente: **nessuna compilazione, nessun test xUnit e nessuna CLI RC83 eseguiti**.

## Controlli effettivamente effettuati

- Confronto SHA-256 file-per-file RC82→RC83: i sei alberi `src/SimRisorse.Core`, `src/SimRisorse.Infrastructure`, `src/SimRisorse.Desktop` e rispettivi test sono byte-identici alla RC82. Nessuna alterazione di engine, XAML, persistenza o catalogo base.
- Tre nuovi sorgenti C#: delimitatori bilanciati su verifica statica semplificata, senza sostituire il compilatore.
- Nuova CLI `--study-e4b-mint-scale` collegata al dispatch e al gate; default `ExpectedTests = 1020`, controllo 9 righe/3 curve/3 tier/Mode analitico e verifica dei tre output.
- Conteggio delle dichiarazioni xUnit RC83: 9 combinazioni curve-tier, 5 soglie target e 4 Fact = **18 casi dichiarati** (il conteggio reale dipende dal runner).
- Tutti i collegamenti Markdown relativi nei quattro nuovi documenti risolti in file esistenti.

## Non verificato qui

Compilazione Roslyn/SDK .NET 10, analizzatori, test, CLI, benchmark naturali completi, prove visuali interattive e correctness end-to-end dell'esportazione. Le verifiche statiche non costituiscono esecuzione; la release resta **candidata**.

Accettazione su Windows: `validate-f1.cmd`, **1020/1020**, test falliti o ignorati zero, audit e screenshot; poi run naturale MintTen completo, distinto dallo studio RC83.
