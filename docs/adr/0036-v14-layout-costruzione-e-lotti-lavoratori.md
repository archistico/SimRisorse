# ADR 0036 — Due righe di comandi Edifici v14 e lotti lavoratori

**Data:** 03/11/2026  
**Stato:** decisione di UI richiesta dall'utente, implementazione RC11 **candidata**.  
**Ambito:** esclusivamente GUI sperimentale v14, nessuna migrazione.

## Contesto

La RC8 esponeva «Costruisci +1» su una riga, ma la riga successiva mescolava `−1/+1 lavoratore` e `+5/+10 costruzioni`; non era evidente a quale operazione si riferissero i lotti. L'utente chiede due righe separate con etichette «Costruisci» e «Lavoratore», ciascuna con pulsanti `+1`, `+5`, `+10`.

## Decisione

1. **Costruisci** a sinistra, **+1 / +5 / +10** a destra; tutti e tre i pulsanti costruiscono la quantità indicata e non sono mostrati quando mancano prerequisiti. Se manca solo materiale, sono presenti ma **disabilitati**. Si conserva il calcolo progressivo e il tooltip dei lotti.
2. **Lavoratore** sotto, **+1 / +5 / +10** a destra, visibile per produttori posseduti. Il precedente `−1` resta compatto accanto all'etichetta per **non rimuovere** una funzione di gestione personale.
3. Tutti i lotti lavoratori sono singole chiamate atomiche a `V14CampaignSession.AssignProducerWorkers(familyId, tier, count)`; disabilitare i pulsanti se la popolazione libera o le postazioni residue sono inferiori alla quantità richiesta. Ricalcolare `CanExecute` e `IsEnabled` quando cambia lo stato o la selezione.
4. Mantenere palette, font, hover/stato disabilitato e geometria della GUI classica. Lo stile `primary build` già presente si applica ai tre pulsanti Costruisci (non agli addetti).

## Conseguenze e vincoli

- Nessun cambiamento al motore di assegnazione, ai conteggi dei residenti, ai magazzini, alle risorse, allo schema SQLite o alla v13.
- I nuovi lotti di personale +5/+10 risultano disabilitati su edifici con pochi posti; sono utili nei livelli grandi/avanzati o con più edifici.
- I test headless dimostrano struttura e comportamento dei comandi, ma **la verifica visiva Windows rimane obbligatoria**.
- Nessun nuovo parametro di bilanciamento E4 viene approvato.

[Rapporto RC11](../design/G1_D27_RC11_LAYOUT_COSTRUZIONE_LAVORATORI.md) · [Piano test](../validation/G1_D27_RC11_TEST_PLAN.md) · [Stato](../CURRENT_STATE.md).
