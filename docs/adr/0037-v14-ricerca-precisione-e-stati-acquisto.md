# ADR 0037 — Presentazione dei punti e abilitazione dell'acquisto ricerca (v14)

**Stato:** decisione UI approvata dall'utente, implementata nella candidata RC12/G1-D28; validazione xUnit/Windows e audit visivo ancora pendenti. **Data:** 03/11/2026.

## Contesto

La schermata Albero tecnologico esponeva tre decimali per il saldo ricerca e lasciava verde «Acquista ricerca» quando un elemento era selezionato ma non pagabile. La logica di spesa Core era già corretta; il problema era di rappresentazione e di guard del comando.

## Decisione

1. Nel pannello ricerca, nei prezzi e nel riepilogo laterale presentare i punti con **due decimali** secondo `it-IT` (incluso il tasso punti/s). Non arrotondare gli importi persistiti.
2. Consentire l'acquisto soltanto quando la tecnologia è disponibile, non acquistata, con prerequisiti e edifici soddisfatti e **saldo effettivo in millipunti** sufficiente.
3. Un unico stato `CanBuySelectedTechnology` deve governare comando e abilitazione esplicita del pulsante; aggiornamento al cambio selezione e dopo ogni refresh.
4. Pulsante non acquistabile grigio, acquistabile verde, hover ocra senza modifiche geometriche. L'elemento rimane selezionabile e la motivazione di blocco è leggibile.
5. **Nessuna modifica** alla campagna v13, al catalogo tecnologico o al motore di acquisto; il bilanciamento E4 resta sospeso.

## Conseguenze e casi limite

Se il valore vero è `99,999` e il costo `100,000`, l'interfaccia a due decimali può visualizzare `100,00` in entrambi; il testo «Punti insufficienti» e il pulsante disabilitato riflettono il confronto esatto. Non usare un saldo arrotondato per autorizzare acquisti.

## Validazione

Due nuovi casi Desktop previsti per RC12: ViewModel/salvataggio e controllo Avalonia Headless del pulsante (stati, hover, geometria). Suite obiettivo **468 test**. [Report](../design/G1_D28_RC12_ALBERO_TECNOLOGICO_PUNTI_E_ACQUISTO.md) · [Piano](../validation/G1_D28_RC12_TEST_PLAN.md).
