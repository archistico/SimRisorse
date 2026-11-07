# Catalogo operativo — RC68 (05/11/2026)

Questo documento distingue gli elementi della GUI v15 dal vecchio catalogo v14. Per i dati esatti il codice resta la fonte primaria: `src/SimRisorse.Core/Food/V15CampaignCatalogProjection.cs`, `src/SimRisorse.Core/Food/FoodIndustryConstructionCatalog.cs` e `src/SimRisorse.Application/Scenarios/CivilGrowthScenario.cs`.

## Famiglie produttive

| Gruppo | ID tecnici |
| --- | --- |
| Lavorazione | `tree-feller`, `sawmill`, `stonecutter` |
| Filiera tradizionale | `fisher`, `farm`, `well`, `mill`, `bakery` |
| Minerali | `iron-mine`, `coal-mine`, `gold-mine` |
| Industria avanzata | `iron-smelter`, `gold-smelter`, `forge`, `mint` |

La **v15 persiste 15 famiglie**, ognuna con Base/Grande/Avanzato; `food-store` è un produttore storico **ritirato**, non acquistabile o persistito come tier. Il planner conserva un adattatore tecnico temporaneo, da eliminare.

## Edifici alimentari v15

**Sei strutture:** Stalla, Cacciatore, Macellaio, Orto, Frutteto, Deposito alimentare. Gli animali e i prodotti in transito hanno inventari propri; il Deposito raccoglie Pesce, Pane, Carne, Frutta e Verdura e prepara Cibo. Non confondere il Deposito nuovo con `food-store`.

## Altri edifici e sistemi

Municipio fisso; Casa piccola/grande; Magazzino; Caserma; Scuola. Progressi individuali degli studenti, ricerca e militari. Livelli produttivi autonomi, direttamente costruibili o migliorabili senza ComboBox.

## Risorse

Risorse tradizionali: Tronchi (`logs`), Legno lavorato (`planks`), Pietra (`stone`), Pesce (`fish`), Cibo (`food`), Grano (`grain`), Acqua (`water`), Farina (`flour`), Pane (`bread`), Minerale di ferro (`iron-ore`), Carbone (`coal`), Ferro (`iron`), Minerale d'oro (`gold-ore`), Oro (`gold`), Spade (`swords`), Monete (`coins`). La v14 aggiunge Matite come risorsa di Scuola. La v15 include inoltre stock fisici separati per Bovini, Selvaggina, Carne, Frutta e Verdura, senza duplicare la quantità di Cibo.

## Economia non ribilanciata

Prezzi e coefficenti del prototipo sono ancora quelli della v14 / RC68. Come riferimenti diagnostici: prima Zecca circa 210 Legno lavorato + 160 Pietra; primo Tagliapietre 30 + 10; crescita prezzo per acquisto `1,15`; tier `1 / 2,6 / 6,5`; personale previsto dai tier storici `1 / 3 / 10`. I prezzi della Zecca sono **giudicati troppo bassi**. I 10 addetti minimi per Zecca **non sono ancora implementati**. Non trattare questi numeri come obiettivi finali.

## Non ancora nel gioco

Flotta/Cantiere navale, viaggi di esplorazione, conquista e Castello non sono parte della campagna ordinaria v15. La relativa roadmap viene **dopo** E4a/E4b. [Roadmap](ROADMAP.md) · [E4b](design/E4B_P00_COSTI_PROGRESSIVI_E_SQUADRE_MINIME.md).


## RC90 — Deposito alimentare in v15

Il deposito unico `food-depot` mantiene il proprio stock di ingredienti e Cibo pronto. L'albero della ricerca mostra gli edifici **effettivamente sbloccabili**: Alimentazione I abilita `food-depot-large` (3 postazioni, 3× capienza Base); Alimentazione II abilita `food-depot-advanced` (10 postazioni, 10× capienza Base). I tre livelli sono edifici acquistabili autonomamente, hanno personale separato e concorrono alla stessa capacità reale, senza creare una seconda ricetta o recuperare il `food-store` v14. I costi dei livelli superiori seguono il moltiplicatore Grande 2,6× e Avanzato 6,5×, oltre al fattore progressivo 1,15× della famiglia alimentare; numeri da sottoporre a bilanciamento.
