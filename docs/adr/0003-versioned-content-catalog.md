# 0003 — Catalogo versionato e stato separato dalle definizioni

- Stato: Accettato
- Data: 2026-10-26
- Ambito: contenuti, bilanciamento e compatibilità delle partite
- Origine: catalogo ampio di edifici e necessità di misurare strategie diverse

## Contesto

Gli screenshot mostrano molti tipi e centinaia di strutture possedute. La bozza usa una sottoclasse per ciascun edificio, anche quando le differenze sono solo costi e tassi produttivi. Il bilanciamento cambierà durante lo sviluppo.

## Decisione

Separare definizioni di contenuto e stato della partita. Usare identificatori stabili e un catalogo versionato per costi, ricette, requisiti ed effetti degli edifici. Le regole generali rimangono codice; i valori di bilanciamento diventano dati validabili.

Non aggiungere una sottoclasse solo per modificare valori. Un comportamento effettivamente diverso può richiedere una regola specifica. Il formato del catalogo non è ancora deciso: JSON è una possibilità.

Salvataggi e risultati devono identificare le regole usate. Una versione non disponibile richiede un rifiuto esplicito o una migrazione definita; non si sostituiscono silenziosamente i valori correnti a quelli di una partita avviata.

## Alternative

- Costanti e sottoclassi per ogni edificio: semplice per pochi elementi, più costoso da bilanciare su un catalogo ampio.
- Tutte le regole in uno script o nel database: massima flessibilità, ma maggiore complessità prima di conoscere le esigenze reali.
- Catalogo unico non versionato: rende ambiguo confrontare i tempi prima e dopo un aggiornamento.

## Conseguenze

Servono validazione, gestione delle dipendenze e una politica di compatibilità. La prima versione può incorporare un piccolo catalogo; la separazione delle definizioni va mantenuta anche prima dell'adozione di file esterni.

## Verifica

Rilevare identificatori duplicati, riferimenti inesistenti, valori non validi e prerequisiti irraggiungibili. Verificare che un cambiamento di costo modifichi l'identità delle regole e non alteri una partita precedente senza una scelta esplicita.

## Collegamenti

- [Architettura](../ARCHITECTURE.md).
- [Riferimenti visivi](../references/README.md).
