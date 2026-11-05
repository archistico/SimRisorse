# ADR 0078 — Confronto semantico completo prima della promozione SQLite v15

**Data:** 05/11/2026 · **Stato:** adottato nel codice RC70, **test .NET pendenti** · **Perimetro:** F1-D05e.2b.2b.2e

## Problema

La generation 6 aveva già salvataggio atomico su file temporaneo e verifica `LoadFrom(temp)`. Ma un checkpoint scritto e riletto può essere formalmente **valido** pur differendo dallo stato richiesto (per omissione o cambiamento accidentale di un campo). La sola validazione della forma non è prova di preservazione dei dati.

## Decisione

1. Prima di promuovere il file temporaneo, confrontare integralmente il checkpoint v15 **canonico di ingresso** con quello riletto: città, civici e produttori, ricerca, tecnologie, Scuola, progressi, militari, crescita, risorse, deposito e filiere.
2. Confrontare come **mappe per ID** edifici, famiglie, contatori e tecnologie: l'ordine delle righe SQL non ha valore semantico. Confrontare invece **posizionalmente** i progressi individuali.
3. Se differiscono, rifiutare il salvataggio **prima** del backup e della sostituzione. Non modificare il file precedente o la partita v14.
4. Calcolare la capacità delle Matite sulla città v15 direttamente, con la formula storica, senza `V14CampaignSnapshot` temporaneo. Conservare il simbolo `V14SchoolSupplies` solo per costante e identificativo condivisi, non come rappresentazione dello stato.
5. Non modificare DDL, schema, versione `user_version=6`, GUI o formule produttive.

## Evidenza e limiti

RC70 aggiunge 4 test xUnit: equivalenza completa di una partita con scorte distribuite e tempo frazionario, due salvataggi con backup conservati, equivalenza della capienza con vari Magazzini/Scuole e assenza di effetto del produttore storico vuoto. **818 test attesi, non eseguiti qui**. Le prove non dimostrano tolleranza universale a interruzioni elettriche o modifiche simultanee da due processi.

## Prossimi passi

RC71 eliminerà lo slot `food-store` ancora iniettato nel `ProductionPlanner` v14; RC72 chiuderà il gate della milestone F1, comprese regressioni GUI e stress combinati.

[Stato](../CURRENT_STATE.md) · [Roadmap](../ROADMAP.md) · [ADR 0077](0077-radice-civica-v15-nativa.md).
