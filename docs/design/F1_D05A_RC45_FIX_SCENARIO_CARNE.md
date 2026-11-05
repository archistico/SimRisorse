# RC45 / F1-D05a-F1 — Correzione scenario Carne → Deposito (4 novembre 2026)

## Evidenza Windows

Sulla RC44 sono stati eseguiti **600 test**: **599 superati e 1 fallito**.
Il test `SimRisorse.Core.Tests.UnifiedFoodSupplyTests.CattleAndGameAreNotFoodUntilButcheredAndDepotProcessed` fallisce in riga 200: attesi **4** unità di Carne, rilevate **6**. Tutte le altre suite del log sono passate. Non si tratta di un errore di compilazione.

## Diagnosi

La fixture originale prevedeva 2 Bovini, 1 Selvaggina e 2 macellai, con ricetta Bovini **1 capo → 3 Carne** e priorità 0 rispetto alla Selvaggina (priorità 1). Perciò entrambi i lavoratori consumavano un Bovino ciascuno, producendo correttamente **6 Carne**. La Selvaggina rimaneva inutilizzata: la previsione di 4 Carne e la copertura di entrambe le ricette erano incompatibili con la fixture.

## Correzione strettamente limitata al test

La fixture ora definisce **2 Bovini → 3 Carne**, mentre **1 Selvaggina → 1 Carne**. I due lavoratori svolgono quindi un lotto di ciascuna ricetta. Il test passa all'operazione di macellazione gli animali **effettivamente ottenuti dal tick precedente** (`noMeat.NextCityResources`), invece di costruire ex novo una scorta identica.

Nuove asserzioni specifiche: una lavorazione Bovini, una Selvaggina, entrambi i tipi di animali consumati, **4 Carne** prodotte, poi 4 Carne lavorate nel Deposito e **3 Cibo** consumati: resta **1 Cibo pronto**. Resta anche la prova che animali non macellati non possono diventare Cibo.

Nessun coefficiente reale del gameplay cambia: questi valori sono **solo ricette di prova**. Il Macellaio vero è ancora nel modello Core isolato e non nella partita v14.

## Confini e gate

- Sorgenti applicativi, GUI, SQLite, save, script, benchmark: **invariati** rispetto alla RC44.
- Unico file di test modificato: `tests/SimRisorse.Core.Tests/UnifiedFoodSupplyTests.cs`.
- **0 nuovi test**, totale **600 attesi**; RC45 non eseguita con .NET 10 nell'ambiente di confezionamento.
- Il precedente checkpoint completo confermato resta RC42 (577 test). RC44 è fallita in un test, quindi non è validata.
- La migrazione alimentare resta in anteprima di sola lettura. Prossima milestone dopo validazione: **F1-D05b**, persistenza separata e versionata per la futura economia v15.

[Validazione RC45](../validation/F1_D05A_RC45_TEST_PLAN.md) · [Roadmap](../ROADMAP.md) · [Handoff](../NEW_CHAT_HANDOFF.md).
