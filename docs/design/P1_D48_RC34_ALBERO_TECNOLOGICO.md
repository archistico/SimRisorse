# P1-D48 / RC34 — Audit delle tecnologie e dei benefici reali

**Data:** 2026-11-04  
**Stato:** implementato, **non compilato/testato qui** (SDK .NET 10 non disponibile).  
**Baseline Windows confermata:** RC30, 511 test. RC31–RC33 ancora da validare cumulativamente.

## Inventario effettivo della v14

| Ricerca | Costo punti | Prerequisiti di ricerca | Disponibilità | Beneficio reale nella v14 |
| --- | ---: | --- | --- | --- |
| Lavorazioni I | 100 | — | Sì | Sblocca **Grande**: Taglialegna, Segheria, Tagliapietra (3 famiglie) |
| Lavorazioni II | 350 | Lavorazioni I | Sì | Sblocca **Avanzato** per le stesse 3 famiglie |
| Alimentazione I | 100 | — | Sì | Sblocca **Grande**: Pescatore, Deposito alimentare, Fattoria, Pozzo, Mulino, Forno (6) |
| Alimentazione II | 350 | Alimentazione I | Sì | Sblocca **Avanzato** per le stesse 6 famiglie |
| Estrazione I | 150 | — | Sì | Sblocca **Grande**: Miniere di Ferro, Carbone e Oro (3) |
| Estrazione II | 400 | Estrazione I | Sì | Sblocca **Avanzato** per le stesse 3 famiglie |
| Industria I | 200 | Estrazione I | Sì | Sblocca **Grande**: Fonderie di Ferro/Oro, Forgia, Zecca (4) |
| Industria II | 450 | Industria I | Sì | Sblocca **Avanzato** per le stesse 4 famiglie |
| Logistica I | 120 | — | **No** | Nessun beneficio implementato |
| Logistica II | 360 | Logistica I | **No** | Nessun beneficio implementato |
| Navigazione | 250 | Lavorazioni I; edificio Zecca richiesto dal prototipo | **No** | Nessun beneficio navale implementato |

> I nomi degli edifici in tabella sono descrittivi; la GUI utilizza i nomi esatti del catalogo di gioco. I prezzi restano candidati e saranno valutati in E4a/E4b.

**Verifica di copertura:** 16 famiglie produttive, ciascuna con un sblocco Grande e uno Avanzato. Tutte le 8 tecnologie acquistabili hanno un effetto implementato; le 3 differite non possono essere acquistate. I livelli Base, Grande e Avanzato rimangono costruzioni distinte, con acquisto diretto e upgrade; nessuna ComboBox introdotta nel dettaglio Edifici.

## Problema riscontrato e interventi

La definizione delle tecnologie indicava `UnlockedFamilies`, mentre il motore utilizzava indipendentemente la mappatura di `TieredProductionCatalog`. In caso di divergenza era teoricamente possibile pagare punti per una ricerca il cui effetto fosse assente dalle vere regole dei tier. Inoltre il dettaglio dell'Albero tecnologico mostrava costi e prerequisiti, ma non i benefici concreti.

- **Nuovo `TechnologyTierEffects`:** proietta i benefici da `TieredProductionFamily` (non dalle etichette del catalogo), distinguendo il tier Grande dal requisito **addizionale** che sblocca Avanzato.
- **Gate in `V14CampaignSession`:** prima di una sessione nuova o ripristinata, controlla che ogni ricerca acquistabile abbia un beneficio effettivo, che `UnlockedFamilies` corrisponda esattamente alle famiglie coperte e che una ricerca Avanzato dipenda da quella Grande. Un catalogo contraddittorio viene rifiutato prima di qualsiasi spesa.
- **GUI Albero tecnologico:** sotto costi e prerequisiti vengono elencati i nomi degli edifici Grande/Avanzato effettivamente sbloccati; per Logistica e Navigazione viene indicata l'assenza di effetti e rimane disabilitato Acquista ricerca.
- **Invarianti:** spesa volontaria di ricerca, precisione del saldo a 1/1000 con display a 2 decimali, costi, progressi, ricette, salvataggi SQLite e GUI v13 **invariati**.

## Test aggiunti

- **4 Application (TechnologyTierEffectTests):** copertura completa delle otto ricerche, catalogo con effetto dichiarato falso, tecnologia differita resa acquistabile senza effetto, dipendenza Grande→Avanzato mancante.
- **1 Application (V14TechnologyBenefitGateTests):** rifiuto di avvio sessione con ricerca acquistabile ma senza effetto.
- **4 Desktop (V14PreviewTests):** tre dettagli tecnologici distinti (Grande/Avanzato/alimentazione) e stato non acquistabile di Logistica.

**Totale:** 9 nuovi casi; **533 attesi cumulativi** (RC33: 524), da confermare con `validate.cmd` su Windows. Il gate della sessione copre anche i caricamenti, poiché una sessione ripristinata passa dallo stesso costruttore.

## Limiti e prossimo step

- Non sono stati aggiunti bonus produttivi a Logistica/Navigazione né modifiche ai numeri economici: richiederebbero gameplay e bilanciamento deliberati.
- La proiezione degli effetti riguarda gli **sblocchi tier attualmente implementati**; quando arriveranno bonus non-tier occorrerà estendere esplicitamente il contratto dei benefici prima di renderli acquistabili.
- **P1-D49:** test end-to-end del percorso Matite → iscrizione → ricerca → tecnologia → costruzione/upgrade e della persistenza; **P1-D50:** gate finale e preparazione misurazioni E4a.

[Roadmap](../ROADMAP.md) · [Test plan](../validation/P1_D48_RC34_TEST_PLAN.md) · [ADR 0045](../adr/0045-tecnologie-effetti-verificati.md)
