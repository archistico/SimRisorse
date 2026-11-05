# F1-D05d.4a — RC52: comandi di città e catalogo v15

**Data:** 5 novembre 2026 · **Stato:** implementato headless, da compilare e testare su Windows .NET 10.

## Risultato

Il giocatore non deve poter acquistare, ampliare o assegnare addetti al vecchio produttore `food-store` mentre usa il motore v15. Ora la sessione `V15FoodIndustryCampaignSession` presenta:

- `CityCatalog`: proiezione in sola lettura di fabbricati e risorse; 15 famiglie produttive legacy visibili ai tre tier distinti, strutture civili, Scuola e 6 siti alimentari. `food-store` non compare; `food-depot` è l'unico Deposito acquistabile. È una **proiezione** v15, non ancora un catalogo persistente completamente nuovo.
- `BuildCityBuilding`, `BuildCityProducer`, `UpgradeCityProducer`, `AssignCityProducerWorkers`, `ReleaseCityProducerWorkers`: usano regole dei costi e tier già validate nella sessione v14 isolata, **mai** il clock v14. L'assegnazione prende in considerazione anche i lavoratori dei nuovi sei edifici, per evitare duplicazioni.
- `BuildSchool`, `CraftPencils`, `EnrollStudents`, `CancelStudents`, `AssignResearchers`, `UnassignResearchers`, `ReturnResearchersToCivilian`, `BuyTechnology`: la Scuola si sblocca con **Casa piccola e nuovo Deposito**, non con `food-store`; iscrizione e annullamento usano solo `Depot.ReadyFood`. Le Matite rimangono nell'inventario cittadino; rimborsi completi devono rispettare la capacità fisica del Deposito.
- `Enlist`, `CancelRecruits`, `DischargeSoldiers`: reclutamento con la stessa popolazione riservata alle industrie alimentari, Scuola e vecchie industrie. Le Spade e gli avanzamenti militari restano nel checkpoint della città.
- `RetireLegacyFoodStore`: decommissionamento **esplicito, una tantum e senza rimborsi**, che azzera edifici e addetti del vecchio produttore ma conserva contatori storici di acquisto, scorte, ricerca e tutti gli altri stati. Non avviene automaticamente.
- `V15CityStorage.Calculate`: il medesimo calcolo delle capacità cittadine viene ora usato dal clock v15, dalla cancellazione delle iscrizioni e dalle proiezioni; le capacità alimentari del Deposito non sono quelle del vecchio magazzino cittadino.

## Invarianti

1. La città **non** conserva mai Cibo pronto; per la compatibilità con i servizi puri della Scuola il Cibo è inserito **solo in una copia locale temporanea** del bundle. Al commit è rimosso dalla città e addebitato o restituito esclusivamente al Deposito.
2. Nessuna risorsa può sparire perché i magazzini sono pieni. Il rimborso non entra nel Deposito se ne supera la capacità. Uno stock già oltre capienza rimane intatto.
3. Ogni comando fallito lascia immutato l'intero checkpoint v15. Un successo valida entrambi i gruppi di addetti prima del commit.
4. Il vecchio `food-store` è ancora **presente nello snapshot interno v14** per soddisfare il vecchio validatore, le 16 famiglie e il catalogo di ricerca storico; è invisibile e non acquistabile dal nuovo catalogo, non produce nel nuovo clock. La cancellazione fisica del suo record richiede un nuovo modello di checkpoint, non un cambio superficiale del nome.
5. La produzione/consumo di Cibo continua a essere eseguita **esclusivamente** dal clock RC51. Non invocare `V14CampaignSession.Advance` né `StepFood` parallelamente.
6. Matite minime e regole dei tier continuano a dipendere dalla `SchoolResearchRules` iniettata. Nessun prezzo o coefficiente definitivo E4b è stato cambiato.

## Proiezione unica per la GUI futura

`V15CampaignCatalogProjection` somma, per ogni ID di risorsa, i quattro proprietari fisici (città, animali, transito, deposito), senza duplicare quantità; somma le capacità dei relativi compartimenti. **Eccezione voluta:** Cibo pronto legge esclusivamente `Depot.ReadyFood` e capacità del nuovo Deposito, ignorando la precedente capacità cittadina `food`. Questi dati saranno collegati alla classica tabella Risorse a sette colonne in D05d.4b; il proiettore non decide ancora pulsanti abilitati, preventivi o tooltip della UI.

Per i tier la GUI riceverà ID distinti `family`, `family-large`, `family-advanced`, mantenendo i comandi diretti e l'assenza di ComboBox.

## Salvataggi, compatibilità e confini

La versione SQLite sperimentale resta **generation 5**. I nuovi comandi modificano esclusivamente campi già persistiti: `CityCampaign` (Scuola, ricercatori, tecnologie, militari, fabbricati, risorse), `Industry`, `Depot`. Il test RC52 salva e rilegge un checkpoint dopo costruzione, iscrizione, ricerca e assegnazione e confronta il successivo clock. È lecito interrompere la compatibilità con i DB sperimentali in fase di sviluppo, ma la GUI v14, i suoi file personali e i test storici **non sono stati toccati**.

## Limiti e lavoro seguente

- Non sono ancora integrati con la GUI i nuovi comandi e i 6 siti: D05d.4b.
- Il vecchio modello di tecnologie e il validatore dello snapshot ereditano ancora `food-store`; serve consolidamento finale prima di eliminare fisicamente questa famiglia e i contatori dal formato v15.
- La proiezione catalogo esclude il vecchio produttore ma **non** è ancora un ruleset v15 completamente autonomo: è un adattatore transitorio dichiarato.
- Non ci sono stati build o test xUnit reali in questo ambiente. Vedere [piano RC52](../validation/F1_D05D4A_RC52_TEST_PLAN.md).

[ADR 0063](../adr/0063-comandi-v15-cibo-esclusivo-e-catalogo-di-presentazione.md) · [Stato](../CURRENT_STATE.md) · [Roadmap](../ROADMAP.md).
