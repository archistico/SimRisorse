# ADR 0098 — Spiegare Produzione e Consumo per fonte nella tabella Risorse

**Data:** 06/11/2026  
**Stato:** implementato RC89.1, candidato alla validazione Windows.

## Problema e scelta

L'utente richiede che, posizionando il puntatore sul valore della colonna **Produzione** o **Consumo**, venga mostrato chi causa il flusso, suddiviso per fonte. La tabella con sette colonne e la GUI pixel art v14 restano canoniche.

- Tooltip sulle **celle** di Produzione e Consumo in v15, v14 e finestra storica: nome della fonte, conteggio edifici e unità attive, quota /s e totale. Le fonti inattive non contribuiscono. Per flussi zero indicare che non vi sono attività. I tooltip cambiano con il refresh degli stessi oggetti di riga.
- In v15 **non** ricostruire una seconda simulazione: ogni contributo viene registrato durante il primo secondo di `V15UnifiedCampaignClock.Plan` sui medesimi report del `ProductionPlanner`, sulle `Runs` effettive della filiera carni/orticoltura e sugli esiti reali del Deposito (conversione e pasti). Trasferire una risorsa fra stock/Deposito non è consumo o produzione.
- La v14 usa lo stesso `ForecastEconomy` che valorizza le colonne; la finestra storica usa i `BuildingView` del medesimo `GameViewProjector`. Il formattatore unico ordina le fonti e verifica la riconciliazione con i valori lordi. In caso di differenza, questa viene indicata esplicitamente senza falsificare il totale.
- Nessuna modifica a prezzi, quantitativi di ricetta, workforce, database, checkpoint, layout, ordine delle righe o tooltip Capacità già approvato.

## Verifica e limiti

12 nuovi casi xUnit (9 Desktop, 3 Application), più controllo manuale su Windows con `./validate-f1.cmd` e mouse sulle celle. La proiezione descrive **soltanto il prossimo secondo**, non una velocità futura garantita; l'attività può cambiare per lavoratori, input, spazio e priorità. Test .NET non eseguiti nell'ambiente di preparazione.
