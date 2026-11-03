# ADR 0021 — Saldo ricerca libero e spesa volontaria

**Data:** 2026-11-03. **Stato: Accettato per la semantica; implementazione soltanto nel Core isolato.**

## Contesto

L'utente ha scelto esplicitamente che i ricercatori producano punti accumulabili senza dover selezionare una tecnologia o mantenere un progetto di ricerca attivo. La tecnologia viene sbloccata quando il giocatore lo decide, pagando i punti disponibili.

## Decisione

- Un unico saldo non negativo di punti ricerca appartiene alla colonia. La produzione non dipende da una tecnologia selezionata.
- I punti sono conoscenza intangibile: non compaiono fra le 16 risorse materiali e non consumano spazio nei Magazzini.
- Quando sarà disponibile la Scuola, i ricercatori assegnati accumuleranno punti con il tempo di simulazione effettivamente avanzato; pausa e chiusura non genereranno punti.
- L'acquisto di una tecnologia sarà un'azione esplicita, atomica: controllo prerequisiti, verifica saldo, spesa e sblocco permanente. Nessuno sblocco automatico al raggiungimento della soglia.
- Conservare le frazioni in millesimi di punto per evitare errori cumulativi; salvataggi e migrazioni verranno progettati prima dell'attivazione in campagna.

## Cosa NON è ancora approvato

Costo e durata della Scuola, posti dei ricercatori, velocità di produzione (0,1 punto/s è soltanto un benchmark candidato), prezzi delle tecnologie, elenco definitivo delle ricerche, prezzi progressivi, moltiplicatori di taglia, condizione di vittoria a 23 tipi. ADR 0018–0020 restano *Proposti* per questi aspetti.

## Implementazione iniziale

In **v60-E1-RC1** sono presenti `ResearchPoints` e `ResearchAccounting` nel Core, indipendenti dai cataloghi e da `GameSession`. Non sono ancora collegati a Scuola, interfaccia, `civil-growth-14` o SQLite. I test del nuovo codice richiedono validazione su Windows; il gate v59-RC1 rimane l'ultima conferma.

## Conseguenze

Una stessa partita può accumulare conoscenza e poi spenderla in un settore diverso senza perdere progresso. Il futuro snapshot dovrà salvare saldo e tecnologie sbloccate senza replicare un progetto di ricerca attivo. I comandi dovranno essere idempotenti rispetto ai doppi clic o ripristini dei salvataggi.
