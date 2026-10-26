# 0001 — Monolite modulare e funzionalità verticali

- Stato: Accettato
- Data: 2026-10-26
- Ambito: struttura della nuova applicazione
- Origine: richiesta di rivedere la bozza e adottare una struttura manutenibile, valutando l'architettura verticale

## Contesto

La bozza unisce Windows Forms, dati e simulazione nello stesso progetto. Il nuovo gioco deve poter essere provato senza finestre, bilanciato automaticamente e collegato a un'interfaccia ancora da confermare. Le regole economiche sono condivise tra molte azioni.

## Decisione

Adottare un monolite modulare con un Core indipendente da interfaccia e persistenza. Organizzare l'applicazione per funzionalità verticali: ogni caso d'uso raggruppa richiesta, validazione, coordinamento ed esito. Le regole comuni restano nel Core.

Separare i confini Core, Application, Infrastructure e Desktop. Aggiungere uno strumento di simulazione quando necessario. Non creare un assembly per ciascun edificio o per ciascun modulo di dominio.

Questa è un'impostazione tecnica adottata per l'avvio, non la conferma di tutte le regole di gioco o dello stack candidato.

## Alternative

- Continuare nel progetto Windows Forms: basso costo iniziale, ma simulazione e test resterebbero legati alla UI.
- Suddivisione esclusivamente per livelli tecnici: separa responsabilità, ma disperde il codice di una funzionalità in molte cartelle.
- Slice completamente isolate con regole duplicate: favorisce divergenze nel consumo di risorse e nell'assegnazione delle persone.
- Microservizi: aggiungono comunicazione e distribuzione senza un'esigenza del gioco locale.

## Conseguenze

Il motore può essere eseguito da UI e strumenti di bilanciamento. Occorre verificare le dipendenze e limitare le astrazioni ai confini effettivi, evitando interfacce create soltanto per replicare ogni classe.

## Verifica

La futura soluzione deve compilare il Core senza dipendenze da Avalonia, Windows Forms o SQLite. Un test o uno strumento deve poter avanzare la stessa partita senza avviare il desktop.

## Collegamenti

- [Architettura](../ARCHITECTURE.md).
- [Vertical Slice Architecture, Jimmy Bogard](https://www.jimmybogard.com/vertical-slice-architecture/).
