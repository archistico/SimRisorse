# ADR 0076 — Portafogli e contatori nativi nel contratto civico v15

**Data:** 05/11/2026  
**Stato:** adottata in RC68 candidata; compilazione e xUnit da confermare su .NET 10 Windows.

## Contesto

RC67 ha introdotto `V15CityCampaignSnapshot`, ma le sue collezioni `ProductiveFamilies` e `PurchaseCounters` erano ancora tipizzate `V14FamilySnapshot` e `V14PurchaseCounter`. Il validatore v15 dipendeva così da DTO storici anche quando operava sulla proiezione nativa.

## Decisione

- Introdurre `V15ProductiveFamilySnapshot` con ID e occupazioni Base/Grande/Avanzato, e `V15PurchaseCounter` con ID e totale storico acquisito.
- Tipizzare con questi record le collezioni nel `V15CityCampaignSnapshot` e usare direttamente tali dati nel `V15CityCampaignValidator` e in `V15CityStorage`.
- `FromInherited` e `ToInherited` rimangono **l'unico confine di traduzione** in questo incremento; sono conversioni 1:1 senza sort, reset, interpolazioni o eliminazione di produttori/contatori. Rifiutano le collezioni non inizializzate e gli elementi nulli.
- Confrontare i contenuti delle collezioni e non l'identità degli array immutabili nei test di round-trip; tutte le altre proprietà rimangono confrontate integralmente.
- Non cambiare `V15FoodCampaignCheckpoint.CityCampaign` o SQLite generation 6 in questa fase. Il contatore storico `food-store` è conservato perché è cronologia, non un edificio produttivo attivo.

## Sicurezza e limiti

Nessuna variazione a costi, ricette, livelli, lavoratori o politica delle capacità. Nessun tocco alla GUI RC63 approvata o alla v14. Non si dichiara eliminato l'uso di `V14CampaignSnapshot` da store, clock, diagnostica e altri raccordi. Le RC64–RC67 sono ancora da validare.

## Successivo

RC69: spostare in modo coordinato i consumer principali e lo store al modello civico v15, verificando il round-trip SQLite e la continuità del clock; poi eliminare lo slot fittizio nel planner. Successivamente E4a benchmark ed E4b economia quasi esponenziale / Zecca con 10 lavoratori minimi per edificio.

[Specifica](../design/F1_D05E2B2B2C_RC68_PORTAFOGLI_CONTATORI_V15.md) · [Piano test](../validation/F1_D05E2B2B2C_RC68_TEST_PLAN.md) · [Roadmap](../ROADMAP.md).
