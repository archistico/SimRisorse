# F1-D01 / RC40 — Piano di validazione

**Baseline verde:** RC38 / 541 xUnit. **RC39:** 545 attesi, non testati. **RC40:** 552 attesi, non testati (7 nuovi Core.Tests). Non dichiarare pass o tempi di benchmark senza output Windows.

## Verifica automatica

```powershell
.\validate.cmd
```

I sette casi `FoodDepotFoundationTests` devono verificare:

1. Nessun Cibo pronto dal nulla: solo input realmente ricevuti dal deposito.
2. Consegne atomiche e capacità per alimento, senza riempimento parziale silenzioso.
3. Limite lavorazione e capacità Cibo pronto, senza spreco di materia prima.
4. Riserva Pane intoccabile dalla conversione in Cibo ordinario.
5. Selezione ripetibile delle ricette, non influenzata dall'ordine delle definizioni.
6. Ricette duplicate, nulle o `food` come input rifiutate.
7. Stock e consegne estranei al modello rifiutati esplicitamente.

## Controlli di regressione

- `CivilGrowthScenario.Create().Catalog.RulesVersion` non cambia.
- V14 e v13 devono continuare ad avere gli stessi salvataggi e le stesse risorse della RC39: **F1-D01 non è ancora attivo**.
- `V14CampaignSession`, `PopulationPlanner`, `V14SqliteSaveStore`, GUI e tutti i benchmark E4a esistenti non devono cambiare.
- Nessun risultato E4a deve essere inventato o marcato `pass` senza esecuzione reale.

## Gate F1-D02

Prima di integrare la carne: definire le nuove risorse e versioni di catalogo, gestire le due ricette del Macellaio senza ambiguità, testare la produzione con input limitati e predisporre la compatibilità SQLite. Questa milestone da sola **non** modifica la filiera alimentare del gioco.
