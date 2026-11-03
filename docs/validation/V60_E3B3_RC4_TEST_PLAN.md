# Piano di validazione v60-E3b3-RC4

**Baseline RC3:** 396/396 test Windows passati (segnalati dall'utente). **Candidata RC4:** 408 test attesi (12 nuovi).

## Copertura

- Acquisto Base con K storico preservato, conteggi e saldo; prezzi progressivi multipli.
- Costruzioni Grande/Avanzato solo dopo ricerca; categorie non produttive escluse; prerequisiti fisici ancora richiesti.
- Upgrade Base→Grande→Avanzato e Base→Avanzato con costo cumulativo equivalente; nessun nuovo contatore.
- Workforce conservata su upgrade, nuova capacità senza lavoratori gratuiti; staffing e rilascio con vincoli di popolazione e posti disponibili (anche studenti/reclute).
- Fallimenti senza mutazione e senza addebiti, inclusi parametri invalidi e fondi insufficienti.
- Ripristino SQLite v3 di livelli misti, assegnazioni, prezzi storici, tecnologie e prosecuzione del clock v14.

## Esecuzione

Su Windows, estrarre lo ZIP in cartella nuova ed eseguire `.\validate.cmd` in PowerShell. Non utilizzare file SQLite personali come destinazione di test. Allegare eventuali log completi dei test falliti.

**Controlli statici di preparazione:** sintassi e presenza dei nuovi file, XML/JSON, link locali, confronto differenziale tecnico RC3 e integrità ZIP. Nessuna esecuzione reale di .NET 10 in container.
