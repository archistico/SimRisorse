# G1-D4 — Audit documentale (non sostituisce i test .NET)

- Sorgente verificato: `CivilGrowthScenario.cs` + `EconomyFoundationScenario.cs` della v59-RC1.
- **16/16** ID delle famiglie con ricetta presenti nel catalogo sorgente, senza duplicati tra Lavorazioni (3), Alimentazione (6), Estrazione (3), Industria (4).
- **11 tecnologie candidate/differite**, ID unici, prerequisiti referenziati, albero aciclico.
- **8/8** tecnologie per i quattro settori hanno un livello I e II con dipendenza corretta.
- **3/3** voci differite non acquistabili: Logistica I, Logistica II, Navigazione.
- Confronto matematico, 25 progressivi e quattro basi di costo: upgrade Base→Avanzata = Base→Grande + Grande→Avanzata, a progressivo famiglia invariato.
- Versione eseguibile `civil-growth-13` non cambiata. Nessun SDK .NET nel container; test compilazione e regressioni runtime **non eseguiti** per questo checkpoint.
- Nota: i tempi e i punti sono simulazioni aritmetiche, non benchmark di campagna reali. La validazione della giocabilità non è stata svolta.
