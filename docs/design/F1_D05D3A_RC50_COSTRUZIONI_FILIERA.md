# F1-D05d.3a / RC50 — Costruzione e personale delle nuove filiere alimentari

**Data: 5 novembre 2026. Stato: implementata, non compilata né testata su .NET 10.**

## Confini dell'incremento

RC50 rende i sei edifici della filiera **costruibili tramite un'API di sessione v15 headless**, senza intervenire su GUI, database storico o orologio v14. Le azioni sono persistibili dal `V15SqliteFoodSaveStore` RC49, generation **5** (non cambia schema). Questa è **F1-D05d.3a**, non l'attivazione della campagna giocabile.

Il catalogo `FoodIndustryConstructionCatalog.Candidate` ha esattamente sei voci, tutte edifici **base direttamente costruibili**. Nessuna combo dei livelli o modulo artificiale. Le seguenti quantità e capacità sono **candidate tecniche**, da rivedere dopo E4a. L'aumento per acquisto della stessa famiglia è del **15%**, con arrotondamento dei singoli costi come nelle altre costruzioni v14. Non è ancora il bilanciamento economico E4b.

| Edificio (`id`) | Costo base, Legno lavorato + Pietra | Prerequisiti (tutti) | Posti | Magazzino per edificio |
| --- | --- | --- | ---: | --- |
| Stalla (`stable`) | 110 + 65 | Fattoria, Pozzo | 1 | Bovini +20 |
| Cacciatore (`hunter`) | 70 + 35 | Casa piccola | 1 | Selvaggina +20 |
| Macellaio (`butcher`) | 120 + 80 | **Nuovo** Deposito alimentare | 1 | Carne in transito +20 |
| Orto (`garden`) | 65 + 40 | Pozzo | 1 | Verdura in transito +20 |
| Frutteto (`orchard`) | 85 + 50 | Orto | 1 | Frutta in transito +20 |
| Deposito alimentare (`food-depot`) | 60 + 30 | Pescatore | 1 | Pesce/Pane/Carne/Frutta/Verdura +50 ciascuno, Cibo pronto +100 |

I prerequisiti hanno controllo di esistenza nel catalogo vecchio o nuovo e rifiuto dei cicli. Controllo di possesso durante l'acquisto (anche su un edificio nuovo). Non viene introdotto un duplicato del **vecchio `food-store`**: è escluso dal catalogo RC50. Tuttavia, il suo stato storico rimane in `CityCampaign` e **non è ancora disabilitata la sua ricetta nel clock precedente**. La riconciliazione del catalogo v14/v15 va completata **prima** di avviare il nuovo gioco, non dopo.

## Contratto di sessione

`V15FoodIndustryCampaignSession` carica e convalida lo snapshot integrale; espone:

- `CheckBuild(id, quantity)` — stessa valutazione di `Build`, ma nessuna mutazione: base per disabilitare il pulsante di acquisto finché mancano i prerequisiti o le risorse.
- `Build(id, quantity)` — acquisto atomico, costo progressivo unità per unità, aggiornamento `Industry.Buildings.Counts` e detrazione risorse della città; nessuna perdita di risorse se l'azione fallisce.
- `AssignWorkers(id, quantity)` / `ReleaseWorkers(id, quantity)` — un posto per edificio base, lavoro non duplicabile e rispetto di popolazione impegnata in industria storica, scuola e militari.
- `Capacities` — derivate dai conteggi, **non salvate** come fonte alternativa; produzione bloccata dal pieno ma eccedenze preesistenti non eliminate.
- `StepFood(meatPolicy, horticulturePolicy, meals)` — calcolo alimentare deterministico esplicito con capienze dei **veri edifici**, che aggiorna tutti e quattro gli inventari e non il tempo della partita. Le rese sono ancora input di policy, non coefficienti approvati.
- `Snapshot()` — checkpoint completo v15 scrivibile con lo store SQLite generation 5 esistente.

**Non è stato introdotto il clock globale**: `StepFood` non incrementa `ElapsedTicks`, non produce da Fattoria/Pozzo/Pescatore/Forno, non esegue ricerca/militari, né collega le razioni alla progressione della popolazione. Collegare in questo momento il vecchio clock produrrebbe `food` da `food-store` e duplicazioni. La priorità del passo **D05d.3b** è un unico catalogo di produzione e un unico clock che faccia avanzare tutte le filiere, la popolazione e il personale, **senza simulare contemporaneamente il vecchio deposito**.

## Salvataggi

Il formato SQLite RC49 resta `user_version=5` / 16 tabelle. Gli edifici e i lavoratori sono già rappresentati in `food_industry_site`. Il numero di edifici è l'unico contatore di acquisti **perché non esiste la demolizione**; in caso di introduzione di demolizioni, la storia degli acquisti andrà aggiunta al contratto. Nessuna migrazione retrocompatibile obbligatoria.

[ADR 0061](../adr/0061-edifici-alimentari-diretti-e-capacita-derivate.md) · [Test RC50](../validation/F1_D05D3A_RC50_TEST_PLAN.md) · [Roadmap](../ROADMAP.md).
