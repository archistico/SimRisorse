# RC68 — F1-D05e.2b.2b.2c · Portafogli/contatori v15 autonomi

## Cambiamenti verificabili

1. `Core/Food/V15CityEconomyContracts.cs`: due record nativi `V15ProductiveFamilySnapshot` e `V15PurchaseCounter`, senza dipendenze ai tipi v14 nel loro stato primario. I `TierOccupancy` continuano a usare il value type condiviso e le regole già approvate.
2. `Core/Food/V15CityCampaignSnapshot.cs`: `ProductiveFamilies` e `PurchaseCounters` diventano rispettivamente `ImmutableArray<V15ProductiveFamilySnapshot>` e `ImmutableArray<V15PurchaseCounter>`.
3. Ingresso `FromInherited`: valida intestazione legacy, collezioni inizializzate, converte 1:1 senza riordinare né ricalcolare. Uscita `ToInherited`: crea DTO per i consumer non ancora migrati, con lo stesso contenuto e la medesima posizione degli elementi.
4. `V15CityCampaignValidator.ValidateNative` e `V15CityStorage.Calculate` utilizzano direttamente i nuovi tipi a parità di proprietà. Non è stata duplicata alcuna formula economica.
5. Due test RC67 precedenti vengono adeguati: il loro confronto prima usava uguaglianza superficiale degli `ImmutableArray` sui record; ora controlla elemento per elemento le nuove collezioni e mantiene il confronto di tutte le altre proprietà.
6. Nove nuovi `[Fact]` verificano proprietà dei nuovi tipi, livelli con lavoratori, cronologia acquisti, ordine, contatore storico, collezioni non inizializzate, duplicati e regressione dei contatori.

## Invarianti importanti

| Argomento | RC68 |
| --- | --- |
| GUI v15 | Identica a RC67 / RC63 approvata |
| GUI / partita v14 | Invariata |
| SQLite v15 | Generation 6 invariata |
| Risorse e capienze | Formule invariate |
| Contatore acquisti `food-store` | Conservato come storia, non come produzione |
| Clock / industrie | Invariati |
| Confronto nel test | Per contenuto, non per identità degli array |

## Confini ancora legacy

Il checkpoint persistito `V15FoodCampaignCheckpoint.CityCampaign` e lo store `V15SqliteFoodSaveStore` hanno ancora la firma `V14CampaignSnapshot`. Il planner produttivo crea ancora uno slot **solo locale** per la ricetta storica disabilitata. Questi elementi sono in roadmap, non vengono dichiarati conclusi.

## Criteri di accettazione

`validate.cmd`: 806 test attesi dopo i 797 di RC67 e 9 nuovi. Verificare successivamente audit combinato e GUI di RC63. Nessun test .NET è stato eseguito nell'ambiente di preparazione.

[ADR 0076](../adr/0076-portafogli-contatori-v15-nativi.md) · [Piano test](../validation/F1_D05E2B2B2C_RC68_TEST_PLAN.md) · [Roadmap](../ROADMAP.md).
