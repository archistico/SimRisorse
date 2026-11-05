# RC69 — Contratto tecnico della radice v15

- Unica radice civica: `V15FoodCampaignCheckpoint.CityCampaign : V15CityCampaignSnapshot`.
- Nessuna cache/copertura secondaria: `NativeCityCampaign` è un alias calcolato senza conversione.
- Il dominio opera su DTO civici v15 e mantiene la precisione in millesimi, i tick frazionari, le famiglie produttive, la ricerca, la Scuola, il reclutamento e la popolazione.
- I portafogli e i contatori dei comandi sono `V15ProductiveFamilySnapshot` e `V15PurchaseCounter`.
- `V15SqliteFoodSaveStore` legge i record v15 direttamente; schema fisico **generation 6 invariato**. Chi importa un `food-store` con addetti deve prima ritirarlo esplicitamente; solo gli slot vuoti sono scartati senza perdita.
- `V15UnifiedCampaignClock` crea ancora uno slot temporaneo `food-store` nel dizionario per il planner storico. È l'attività separata RC71.
- Interfaccia Desktop identica a RC68; parità GUI approvata in RC63 secondo ADR 0071.

## Limiti dichiarati

Non è stata eseguita la compilazione C# .NET 10. I nuovi test contano 8, ma non sono prova di successo fino al passaggio su Windows. Se falliscono, creare RC69-fix prima di RC70.

[ADR 0077](../adr/0077-radice-civica-v15-nativa.md) · [Piano test](../validation/F1_D05E2B2B2D_RC69_TEST_PLAN.md).
