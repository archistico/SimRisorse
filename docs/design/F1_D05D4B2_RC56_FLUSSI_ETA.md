# F1-D05d.4b.2 — RC56: flussi reali, capienze ed ETA v15

## Baseline e obiettivo

La RC55 è stata confermata sul runner Windows **713/713 test**. La RC56 collega l'ultimo gruppo di colonne della tabella Risorse v15 alle operazioni effettive del motore, mantenendo intatto tutto il percorso v14.

## Motore

- `V15UnifiedCampaignClock` restituisce nel `V15CampaignAdvancePlan` anche due `ResourceBundle`: `FirstSecondProduction` e `FirstSecondConsumption`. La misurazione avviene solo al **primo** secondo intero del piano, per evitare costi di registrazione a ogni secondo durante avanzamenti multipli.
- `V15EconomyProjector.Project` richiama lo stesso clock con `TimeSpan.FromSeconds(1)`. Il checkpoint di partenza è immutabile; la funzione restituisce la proiezione di inventari e due vettori di flussi, senza avviare/fermare la simulazione né scrivere salvataggi.
- Nei produttori storici le due grandezze derivano da `RunnableWorkers` **reali** di `ProductionPlanner`; `food-store` è soppresso (nessuna doppia conversione).
- Per Stalla/Cacciatore/Macellaio si rilevano direttamente `MeatChainTickResult.Produced/Consumed`; per Orto/Frutteto si usano `HorticultureTickResult.Produced/Consumed`.
- Per il Deposito si conta **solo** `Processing.ConsumedIngredients` e `Processing.ProducedFood`, insieme alle razioni effettivamente servite (`Meal.ConsumedReadyFood/ConsumedBread`). La consegna al Deposito non è un consumo né una produzione; il cibo utilizzato per il pasto viene sottratto una sola volta.
- Formula riga: `saldo = produzione − consumo`; le quantità sono fissate al millesimo (tipo `ResourceAmount`) e convertite in `decimal` per la visualizzazione.

## Interfaccia

La `V15ResourceRow` conserva il nome, l'identificativo, la quantità visiva arrotondata, la capacità, il saldo, l'ETA e la percentuale di riempimento. `Refresh` aggiorna gli oggetti esistenti e ne conserva la selezione. Le colonne restano in ordine **Risorsa, Quantità, Capacità, Produzione/s, Consumo/s, Saldo/s, Si riempie tra**, con numeri a destra e titolo capienza con hover. Quando il saldo è 0 si mostra `±0 /s`, non «—»; un saldo negativo viene mostrato con segno.

La `V15CampaignCatalogProjection` espone ora `CapacitySources`, con i componenti fisici che sommati forniscono sempre la capacità visualizzata. Il Cibo pronto ha solo la fonte **Deposito · Cibo pronto**. Le scorte reali restano separate in città, animali, transito e deposito.

L'ETA usa un **saldo del prossimo secondo considerato costante** e ignora eventi futuri (esaurimento input, saturazione di altre scorte, futuri nuovi lavoratori e cambiamenti dei consumi con la popolazione). Perciò è una stima locale, non un tempo certo. Valori non applicabili hanno «—», capienza già raggiunta `00:00:00`, con ore anche oltre 24.

## Cosa non è incluso

- La rimozione fisica dello slot storico `food-store` dal checkpoint v15 ereditato (F1-D05e).
- Diagnostiche approfondite per singolo lavoratore e prognosi su molti secondi.
- Modifica delle capacità, delle ricette, dei costi, della popolazione o della struttura SQLite `user_version=5`.
- Modifica della GUI v14, dello stile classico, dei tier diretti o dei salvataggi personali.

## Gate

**18 nuovi casi**: 9 Application test per contabilità, owner, carne/verdura, razioni atomiche e riconciliazione di tutti gli inventari; 9 Desktop test (3 fatti e 6 InlineData) per formati italiani, ETA e aggiornamento delle righe. **Totale previsto RC56 731**. Non eseguibili qui senza SDK .NET 10: seguire il piano Windows.

[ADR 0066](../adr/0066-flussi-v15-misurati-dal-clock-e-eta-locale.md) · [Test](../validation/F1_D05D4B2_RC56_TEST_PLAN.md).
