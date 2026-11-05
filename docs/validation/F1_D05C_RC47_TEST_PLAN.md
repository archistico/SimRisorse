# F1-D05c / RC47 — Piano di validazione

**RC45 confermata: 600 test. RC46 candidata: +9 test (609 attesi). RC47: +11 test (620 totali attesi).** Non è stata eseguita una suite .NET 10 in questo ambiente.

## Windows: verifica cumulativa

Estrarre lo ZIP RC47 integralmente in una cartella di sviluppo; dalla root:

```powershell
.\validate.cmd
```

**Atteso:** 620 test superati, 0 non riusciti, 0 ignorati. In caso di fallimento non promuovere il salvataggio v15 né modificare il file reale v14.

## Test aggiunti: `V14ToV15FoodMigrationTests.cs`

| Caso | Garanzia |
| --- | --- |
| `PreviewDoesNotCreateV15OrChangeSourceBytesOrBackups` | Preview senza effetti e deficit diagnostico |
| `CopyPreservesOriginalBytesAndEntireHistoricFoodInventory` | v14 byte identici; Cibo/Pane/Pesce e stato preservati |
| `RepeatedIdenticalMigrationIsIdempotentAndDoesNotCreateAnyBackup` | Seconda richiesta no-op e nessuna modifica |
| `DifferentValidV15CampaignIsNeverOverwritten` | Una v15 già evoluta o differente rimane intatta |
| `ExistingNonV15DestinationRemainsByteIdentical` | Un file v14 nel percorso target è rifiutato |
| `CorruptV14SourceFailsClosedWithoutDestination` | Nessuna promozione di file corrotti |
| `MissingV14SourceDoesNotProduceV15File` | Nessun file artificiale su origine mancante |
| `SameInputAndOutputPathIsRejectedBeforeAnyWrite` | Vietata migrazione in-place |
| `CopyToNestedDirectoryPromotesOnlyVerifiedV15AndLeavesNoStagingFiles` | Staging pulito e promozione esclusiva |
| `AnOriginalChangedAfterMigrationIsNotSilentlyUsedToReplaceV15` | Sorgente aggiornata non può sostituire una v15 esistente |
| `RollbackIsPossibleByKeepingTheOriginalAndDiscardingOnlyTheNewCopy` | La v14 rimane leggibile dopo l'abbandono della copia |

## Verifiche statiche

- Confronto RC46 → RC47: un solo nuovo file Infrastructure e uno di test, oltre ai documenti; no GUI, cataloghi, tick o code path di salvataggio v14 modificati.
- Parse XML/XAML e JSON del repository e collegamenti Markdown locali.
- Controllo ZIP con `testzip()` e manifest comparativo.
- La v14 non viene mai aperta in scrittura dal migratore; lo staging usa lo store v15 isolato.

[Audit statico RC47](F1_D05C_RC47_STATIC_AUDIT.md).

## Gate manuali differiti

- Avvio normale `run-game.cmd` **deve ancora usare il database v14**. Non eseguire manualmente conversioni su un database personale finché non esisteranno UI/CLI controllate, backup di attivazione e test su copie.
- Sperimentazioni future: processo terminato durante lo staging, SQLite WAL con writer concorrenti, permessi disco, collisioni simultanee, disconnessioni disco e recupero da crash dopo la promozione. Questi scenari **non sono stati collaudati** su Windows nella RC47.
- F1-D05d: attivare nuove risorse nel catalogo e nuove regole alimentari soltanto dopo un formato v15 aggiornato e sessione stabile. F1-D05e: stress test e recovery.

[Contratto](../design/F1_D05C_RC47_MIGRAZIONE_COPY_ON_WRITE.md) · [Roadmap](../ROADMAP.md).
