# ADR 0075 — Contratto civico nativo v15 come passaggio di migrazione

**Data:** 05/11/2026  
**Stato:** Decisione adottata nella candidata RC67; test .NET 10 da eseguire su Windows.

## Contesto

RC66 valida direttamente il ruleset v15, ma i dati di città sono ancora trasportati da `V14CampaignSnapshot`. Cambiare in un solo incremento il tipo primario del checkpoint, il clock, lo store SQLite e tutte le API pubbliche avrebbe un rischio di regressione eccessivo mentre RC64–RC66 non sono ancora validate.

## Decisione

1. Introdurre `V15CityCampaignSnapshot`, che possiede i dati civici della nuova campagna senza campi di intestazione `FormatVersion`/`RulesVersion` v14.
2. `V15CityCampaignValidator.ValidateNative` lavora direttamente sul DTO v15. L'overload storico `Validate` converte attraverso `FromInherited`, che rifiuta versioni/regole estranee.
3. La proiezione `V15FoodCampaignCheckpoint.NativeCityCampaign` espone lo stesso stato senza riscriverlo. La validazione del checkpoint passa dalla proiezione v15.
4. `V15CityStorage.Calculate` accetta il DTO nativo e mantiene un ingresso temporaneo legacy per clock, comandi e UI esistenti. La sola formula relativa alla capacità delle Matite riutilizza ancora, tramite `ToInherited`, `V14SchoolSupplies.Capacity`: non raddoppiare le formule.
5. Non migrare ancora `CityCampaign` del checkpoint a un tipo differente: aspetta di convertire coordinatamente persistenza, sessione, clock e fixture, con test eseguibili.

## Invarianti

La trasformazione v14-shaped → v15 → v14-shaped conserva integralmente risorse al millesimo, scorte, tempo, crescita frazionaria, popolazione, edifici/livelli, cronologia, Scuola, ricerca e militari. Non crea `food-store`, Pane/Cibo, edifici o lavoratori; non tocca la GUI né il DB v14. SQLite v15 generation 6 invariata.

## Debiti noti e prossime milestone

Il DTO nativo usa ancora `V14FamilySnapshot` e `V14PurchaseCounter`, il checkpoint persistito è ancora `V14CampaignSnapshot`, resta il contatore `food-store` ed esiste lo slot temporaneo del planner. Sono dipendenze esplicite, **non** rimosse dalla RC67. Priorità RC68: rendere nativi i record delle famiglie/contatori e adeguare lo store mantenendo i test di confronto; poi rimuovere lo slot nel planner. Successivamente E4a/E4b.

[Specifica](../design/F1_D05E2B2B2B_RC67_CONTRATTO_CIVICO_V15.md) · [Piano test](../validation/F1_D05E2B2B2B_RC67_TEST_PLAN.md) · [Roadmap](../ROADMAP.md).
