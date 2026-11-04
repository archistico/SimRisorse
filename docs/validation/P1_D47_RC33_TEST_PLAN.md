# Piano validazione RC33 / P1-D47

**Stato:** da eseguire su Windows con SDK .NET 10. **Non eseguito in questo ambiente.**

## Gate automatico

Da cartella RC33 appena estratta:

```powershell
.\validate.cmd
```

Atteso: **524/524** test, zero errori di compilazione. La suite è cumulativa: include RC31, RC32 e RC33, quindi non è necessario convalidare i rispettivi archivi separatamente.

Controlli specifici:

- `V14SchoolSuppliesTests.CraftPreviewIsReadOnlyAndUsesTheSamePreconditionsAsTheCommand`
- `V14SchoolSuppliesTests.BatchCraftingIsAtomicWithInsufficientCoalAndPreviewNeverConsumesInputs`
- `V14SchoolSuppliesTests.BatchCraftingNeedsSpaceForFullOutputAndNeverPartiallyCommits`
- `V14SchoolSuppliesTests.CancellingNewestPrepaidStudentDoesNotRefundOlderLegacyStudent`
- `V14PencilWorkshopPreviewTests.FiveBatchHintReportsBothMissingMaterialsAndCommandRemainsDisabled`
- `V14PencilWorkshopPreviewTests.StorageHintMatchesBatchAndOneBatchCanStillBeCrafted`

## Gate manuale — usare una copia della partita

1. `run-game.cmd` apre la v14 senza `--experimental-v14`; controllare che la v13 resti disponibile con `--legacy-v13` su uno slot separato.
2. Con Scuola presente, scegliere +5 nella pagina Scuola: senza materiali sufficienti il comando deve essere grigio e il suggerimento deve indicare **Legno lavorato e Carbone**, quantità disponibili/necessarie e mancanze.
3. Con materiali sufficienti e spazio Matite inferiore a 20: +5 non deve produrre; passando a +1, se restano almeno 4 posti, la produzione deve essere consentita.
4. Verificare che il saldo dei materiali scenda esattamente di 1 per ciascuno e lo stock Matite salga di 4; testare anche +5/+10 con disponibilità e capacità adeguate.
5. Iscrivere uno studente: consumare 10 Cibo e 2 Matite; annullare prima della qualifica e controllare che le scorte tornino al valore iniziale se il deposito ha spazio.
6. Aprire una partita v14 antecedente alle Matite **solo in copia**: gli allievi già iscritti devono terminare la formazione senza ricevere Matite inesistenti a seguito dell'annullamento.
7. Salvare, riaprire e verificare Matite, disponibilità del laboratorio e diritto ai rimborsi. Ripetere i controlli grafici a **1040×740** e **1240×850**.

## Segnalazione anomalie

Indicare versione, comando, lotto selezionato, scorte e capacità, screenshot o TRX, atteso/riscontrato. Non modificare ricette o costi sulla base di una sola impressione visiva: le valutazioni economiche appartengono a E4a/E4b.

[Rapporto tecnico](../design/P1_D47_RC33_FILIERA_MATITE.md) · [Roadmap](../ROADMAP.md)
