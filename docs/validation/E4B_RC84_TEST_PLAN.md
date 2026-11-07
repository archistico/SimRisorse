# RC84 — piano di verifica (candidato, 06/11/2026)

**Checkpoint confermato:** RC82, **1002 test superati** dall'utente. **RC83:** 18 nuovi, non ancora confermati. **RC84:** +18 nuovi, quindi **1038 complessivi attesi**, non eseguiti qui per assenza dell'SDK .NET 10.

## Controlli xUnit aggiunti

`Rc84MintNaturalFeasibilityTests.cs` contiene 18 casi conteggiati (16 Fact e una Theory a due valori) sui seguenti aspetti: contratti `MintTen`/`Legacy`, diagnostica vs completamento, nove ipotesi isolate, export CSV/Markdown, determinismo e inventario iniziale, versioni sconosciute, scenario e obiettivo, mismatch scenario base, inventario e orizzonte, sole prove Legacy, duplicati, falso evento di completamento, input null/vuoto.

## Gate automatico

`validate-f1.cmd` usa `scripts/validate-f1-gate.ps1` con **1038 test attesi**. Dopo la CLI RC82 in diagnostica a 0 secondi, esegue `--analyze-e4b-mint-ten --input ... --output ...`; controlla **4** run, **4** diagnostiche, **0** run a orizzonte pieno, **0** milestone MintTen, **9** ipotesi non attive, SHA-256 e file JSON/CSV/Markdown. Conserva tutti i smoke precedenti. Non modifica la cartella dei salvataggi personali.

## Verifica manuale successiva

1. Da una cartella di lavoro pulita, eseguire `validate-f1.cmd` e verificare build, TRX, numero dei test, smoke desktop e assenza di warning/errori.
2. Generare output naturale completo RC82 con `--benchmark-e4b-mint-ten --compare-legacy --output DIR_NUOVA` (senza `--max-seconds`) e verificare `Diagnostic=false` nelle quattro esecuzioni, salvo eventuale fine anticipata prevista dal runner; non inferire il successo dall'assenza di eccezioni.
3. Importare `e4b-mint-ten-results.json` tramite CLI RC84 e verificare SHA-256, 4 run, 9 ipotesi e tempi di costruzione/squadra/Monete. Se vi sono timeout, valutarli come strategie non completate, non come softlock universale.
4. Ripetere input identico in nuova cartella: confrontare byte per byte tutti e tre i file. Provare input JSON alterato, duplicato o un file di output preesistente: rifiuto senza sovrascrittura.
5. Verificare manualmente la GUI pixel art v14/v15 in gioco (sette colonne Risorse, blocchi visibili ma attenuati, pulsanti, dettagli e icone). Nessun file Desktop è modificato in RC84.

## Esiti attuali

- Sorgenti e gate RC84 predisposti, solo controlli statici e verifiche archivio disponibili.
- **NON eseguiti:** compilazione .NET, 18 test RC84, 18 test RC83, CLI eseguita, run naturali completi.
- Non dichiarare 1038/1038 superati senza un nuovo riscontro reale.
