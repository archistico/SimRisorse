# RC70 — Audit dello store v15 e capacità Matite

## Confronto prima della promozione

`V15SqliteFoodSaveStore.Save` procede nell'ordine:

1. Convalida del checkpoint; rifiuto del produttore storico ancora occupato; canonicalizzazione **solo** dell'eventuale slot vuoto.
2. Verifica del file destinazione se già presente (schemi sconosciuti o danneggiati sono rifiutati).
3. Scrittura su SQLite temporaneo nella stessa directory della destinazione.
4. **Lettura e confronto semantico completo** con il checkpoint canonico. I singoli progressi di Scuola ed esercito hanno ordine posizionale; gli altri gruppi confrontati per ID.
5. Solo se identico: backup della versione precedente, poi promozione del temporaneo. Pulizia del temporaneo in caso di eccezione.

Non viene aggiunto un secondo database né modificato il contratto SQLite generation 6.

## Capacità di Matite

`V15CityStorage.Calculate` usa la formula originale: `baseline.Capacity["planks"]` + capacità `planks` del Municipio e dei Magazzini posseduti + `50 × scuole`. Non serializza né converte il checkpoint in DTO v14. Il valore viene ancora rappresentato come risorsa `pencils`.

## Limiti

Non è stata eseguita compilazione .NET 10 né simulazione di power-loss reale. I test verificano equivalenza, backup e proprietà logiche. RC71 interverrà sul planner, non su questa rappresentazione di capacità.

[ADR 0078](../adr/0078-verifica-semantica-salvataggio-v15.md) · [Piano di test](../validation/F1_D05E2B2B2E_RC70_TEST_PLAN.md).
