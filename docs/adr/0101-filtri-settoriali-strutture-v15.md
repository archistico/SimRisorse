# ADR 0101 — Barra «Filtra» nella scheda Edifici v15

**Data:** 06/11/2026  
**Stato:** richiesta grafica approvata dall'utente; implementata come RC91 candidata, in attesa della suite Windows.

## Contesto

La scheda Edifici v15 usava un singolo pulsante «Filiere alimentari» che alternava il catalogo completo alle otto voci dell'industria alimentare. Non consentiva di isolare i produttori tradizionali di cibo né le filiere di estrazione, industria, lavorazioni o magazzino. La GUI pixel art v14 resta il riferimento, quindi non sono ammesse ComboBox dei tier o alterazioni dei dettagli approvati.

## Decisione

1. Sostituire il pulsante con la scritta **Filtra** (etichetta fissa, non cliccabile) e pulsanti compatti **Tutti, Alimentazione, Estrazione, Industria, Lavorazioni, Logistica, Abitazioni, Civici, Militari**. Il pulsante attivo è evidenziato; i pulsanti vanno a capo se necessario, senza coprire le intestazioni STRUTTURA/EDIFICI/PERSONALE.
2. La classificazione usa gli identificativi stabili delle famiglie, non parole nel nome, prezzi o sblocchi; ogni tier Base/Grande/Avanzato resta con la stessa famiglia. Le otto voci dell'industria alimentare, inclusi i Depositi Grande e Avanzato, sono in Alimentazione insieme a Pescatore, Fattoria, Pozzo, Mulino e Forno. Magazzino è in Logistica; Municipio e Scuola in Civici; Caserma in Militari.
3. Mantenere anche le voci **bloccate** nel gruppo assegnato e consultabili in grigio. Se il filtro contiene la selezione precedente, preservare edificio e dettaglio; altrimenti selezionare la prima voce visibile. Tornando a Tutti mostrare l'intero catalogo senza modificare il gioco o l'ordine alfabetico.
4. Un eventuale edificio futuro non classificato resta accessibile attraverso Tutti (fallback `Other` interno, senza pulsante vuoto). Il filtro è solo stato dell'interfaccia: nessuna modifica a cataloghi, livelli, salvataggi SQLite, costi, produzione o ricerca.

## Verifiche

Copertura dei gruppi senza duplicazioni, tier, filiera alimentare, edifici bloccati, selezione e refresh; screenshot e controlli di non sovrapposizione a 1040×740 e 1240×850. I nuovi casi xUnit RC91 sono **21** (8 fatti + teoria con 13 casi), per un totale **1188 attesi** a partire dai **1167 confermati dall'utente** in RC90.3. Compilazione e test .NET 10 non eseguiti in questo ambiente.

Si veda [piano di validazione](../validation/RC91_BUILDING_SECTOR_FILTERS.md).
