# ADR 0046 — Verifica integrata Scuola, Ricerca e tier produttivi

**Data:** 2026-11-04  
**Stato:** adottato per la copertura test; implementazione RC35 **in attesa di validazione Windows**.

## Contesto

Le milestone P1-D46–D48 hanno aggiunto test separati per formazione, laboratorio Matite, benefit delle tecnologie e persistenza. Un difetto di integrazione può comunque emergere solamente nella sequenza reale fra questi sottosistemi.

## Decisione

Aggiungere prove di integrazione sullo **stesso checkpoint v14** che seguano creazione Scuola, fabbricazione Matite, iscrizione, qualifica, assegnazione ricercatore, maturazione punti, acquisto volontario, upgrade, salvataggio e ripresa SQLite v3. La fixture può finanziare le costruzioni **entro capienza**, ma non può attribuire punti o tecnologie artificialmente nella prova principale. Il ripristino in pausa, i blocchi atomici e il mantenimento della storia degli acquisti sono invarianti.

## Alternative e motivi

- Soli test unitari: insufficienti per lo stato condiviso e gli aggiornamenti a più sottosistemi.
- Scenario UI end-to-end con mouse e timer reali: fragile e dipendente dall'ambiente grafico; mantenerlo come validazione manuale distinta.
- Nuovo simulatore o cambiamenti al clock: non necessari per l'integrazione e prematuri prima di E4a.

## Conseguenze

Tre nuovi test di infrastruttura e nessuna modifica al codice produttivo o allo schema di salvataggio. La suite esegue clock simulato con i numeri correnti, non introduce nuove regole. Ogni cambiamento futuro ai coefficienti dovrà aggiornare il contratto di test **solo dopo approvazione di E4b**, distinguendo comportamento invariato e valori candidati.

[Rapporto](../design/P1_D49_RC35_INTEGRAZIONE_PROGRESSIVA.md) · [Piano test](../validation/P1_D49_RC35_TEST_PLAN.md)
