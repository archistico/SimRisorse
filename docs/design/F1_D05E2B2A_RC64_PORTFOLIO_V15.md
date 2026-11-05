# F1-D05e.2b.2a — Contratto del portfolio produttivo v15 (RC64)

## Confini

**Stato persistente:** `V15FoodCampaignCheckpoint.CityCampaign.ProductiveFamilies` ha 15 elementi per le nuove partite e per quelle lette da SQLite generation 6. Non viene introdotto un produttore pronto da utilizzare per creare Cibo dal Pesce. Il Cibo pronto continua ad appartenere soltanto al nuovo Deposito.

**Boundary di compatibilità:** `V15LegacyProducerAdapter.ForV14Rules` genera una copia della collezione con uno slot `food-store` vuoto. Questa copia entra nella validazione e nei comandi civici v14, ma viene scartata dopo l'operazione. `WithoutEmptySlot` canonicalizza la collezione all'ingresso da una migrazione, e a ogni commit della sessione. I contatori restano nel formato storico.

**Capacità città:** `V15CityStorage` salta esplicitamente il vecchio produttore quando attraversa le definizioni del catalogo, senza cercarlo fra gli edifici civici. Il Cibo pronto rimane sotto la capacità del Deposito.

**Clock:** `ProductionPlanner` indicizza le definizioni del catalogo v13 tramite ID e richiede la presenza di tutte. Il clock fornisce un `BuildingState` locale `food-store=0/0`, con la ricetta disabilitata; non lo scrive mai nel checkpoint. Non è un nuovo edificio costruibile.

**SQLite:** `V15SqliteFoodSaveStore.ReadFile` ora restituisce direttamente 15 famiglie. Lo schema resta generation 6, 16 tabelle e 45 tier. Un vecchio slot in memoria con lavoro e/o edifici è rifiutato dal writer: ritirarlo esplicitamente; non eliminare stock, popolazione o ricompense in modo nascosto.

**Interfaccia:** zero modifiche a `src/SimRisorse.Desktop`, salvataggi v14 o layout. In RC64 sono invariati il menu laterale, la barra della crescita, i dettagli con icone e controlli 1/5/10, gli edifici grigi consultabili, le sette colonne Risorse, Scuola/Esercito e i livelli costruibili direttamente.

## Cosa non è ancora concluso

Il record del blocco civico deriva ancora da `V14CampaignSnapshot`. I comandi `BuildCityBuilding`, `BuildCityProducer`, `UpgradeCityProducer` e `BuyTechnology` usano temporaneamente `V14CampaignSession`. Nella fase F1-D05e.2b.2b elimineremo progressivamente questa dipendenza, separando formula dei costi, costruzioni, ricerca e personale, con regressioni per ogni passaggio. La famiglia ritirata è assente dallo stato v15 ordinario, ma il contatore storico rimane.
