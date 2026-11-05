# RC53 / F1-D05d.4a-fix — Diagnosi e correzioni da TRX RC52

**Input:** validazione Windows del 05/11/2026 ore 13:48 circa: 703 test eseguiti, 695 riusciti, 8 falliti; Desktop.Tests superati. Il messaggio terminale "Compilazione non riuscito" proviene dall'errore di `dotnet test` nel wrapper `validate.ps1`, non prova di errori di compilazione C#.

## Cause individuate e interventi

| Test fallito RC52 | Diagnosi | RC53 |
| --- | --- | --- |
| `FoodDepotBuildDoesNotConstructOrAssignLegacyFoodStore` | `UnlockLegacy` modificava `OtherBuildings` invece della famiglia produttiva `fisher`; ulteriore assert leggeva erroneamente `food-store` in `OtherBuildings` | Fixture di `ProductiveFamilies` e controllo della famiglia storica corretti |
| `StableRequiresBothFarmAndWell` | `farm` e `well` sono famiglie produttive tier, non edifici civili | `UnlockLegacy` aggiorna anche `ProductiveFamilies` |
| `ButcherAndOrchardRequireTheirOwnIndustryPrerequisites` | Il Deposito non era costruibile nel test per `fisher` assente | Stessa correzione fixture, nessuna rimozione dei prerequisiti |
| `HunterButcherDepotDeliverWithoutDoubleUsingAnimalsOrPeople` | Lo scenario non costruiva il Deposito senza il prerequisito `fisher` | Stessa correzione fixture |
| `ConstructedStaffedHunterAndDepotCanBeRestoredWithCostsAlreadyDebited` | La fixture Infrastructure inseriva `fisher` in `OtherBuildings` | Aggiornata `ProductiveFamilies` e mantenuti contatori |
| `CannotAssignFoodWorkersWhoAreReservedElsewhere` | La fixture riservava 8 addetti Hunter con soli 5 posti di popolazione libera, invalida già nel costruttore | Calcolo dei posti liberi *dopo* i 3 lavoratori storici, assegna esattamente quelli e testa un'ulteriore richiesta |
| `ResourceSummaryAggregatesPhysicalInventoriesWithoutDoubleCountingFood` | Validator rifiutava il Pesce/Pane non ancora consegnati in città | Distinzione fra Cibo pronto e ingredienti in transito |
| `ReadyFoodIsNeverStoredInCityOrProducedByLegacyFoodStore` | Idem: città con Pesce in attesa e senza nuovo Deposito | Stessa correzione senza riabilitare `food-store` |

## Regola di proprietà delle scorte

- `food`: solo Deposito **pronto**; vietato in città.
- `fish`, `bread`: città **prima** della consegna + raw del Deposito **dopo** la consegna; conservazione e somma per inventari fisici, non doppio conteggio.
- `cattle`, `game`: solo `Industry.Animals`.
- `meat`, `fruit`, `vegetables`: `Industry.Transit` o raw del Deposito, non città.

Il precedente test `FoodCannotAppearInBothCityAndDepot` controllava una regola troppo ampia: viene corretto in `PreparedFoodCannotAppearInCityAndDepot`, con test aggiuntivi che preservano il legittimo doppio contenitore degli ingredienti. Non cambiano ricette, prezzi, crescita, clock, algoritmo SQLite o GUI.

## Limiti

Nessun SDK .NET 10 disponibile nel confezionamento Linux. I file modificati sono analizzati staticamente ma **non compilati né eseguiti** qui. I test attesi sono **705**: prima completare il gate Windows con `validate.cmd`, poi riprendere il lavoro GUI F1-D05d.4b. La RC52 rimane fallita (695/703); RC47 è l'ultimo checkpoint integralmente confermato (620/620).

[ADR 0064](../adr/0064-ingredienti-in-transito-citta-deposito-e-fixture-produttive.md) · [Test RC53](../validation/F1_D05D4A_RC53_REGRESSIONI_TEST_PLAN.md) · [Roadmap](../ROADMAP.md).
