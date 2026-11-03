# ADR 0023 — E3: Scuola e albero tecnologico come prototipo Core isolato

- **Data:** 2026-11-03
- **Stato:** Proposto per meccaniche numeriche / implementazione sperimentale isolata
- **Decisione preesistente vincolante:** ADR 0021, saldo ricerca libero e acquisti volontari.

## Contesto

E1 ha introdotto prezzi progressivi e saldo puro, E2 ha aggiunto livelli e requisiti di ricerca. Manca la fonte di punti e il collegamento tra acquisto tecnologia e sblocco. La versione stabile `civil-growth-13` deve continuare a usare identiche regole e schema del salvataggio.

## Soluzione di prototipazione

Separare (a) la formazione/assegnazione dei ricercatori, (b) il saldo e l'acquisto delle tecnologie e (c) la proiezione della produzione E2. Ricercatori e studenti sono sempre abitanti riservati dalla stessa popolazione, non entità generate; gli attivi sono un sottoinsieme dei qualificati. I punti non occupano magazzini né vengono spesi automaticamente. Le transazioni restituiscono valori immutabili e fallimenti espliciti.

Il catalogo di prova rende acquistabili **nel solo prototipo** otto tecnologie produttive; Logistica e Navigazione restano non acquistabili anche nel prototipo. Le regole di costo e formazione restano parametriche e non costituiscono un'approvazione dei valori proposti.

Un `ResearchPrototypeCheckpoint` testabile con JSON è un DTO isolato, **non sostituisce** gli snapshot di gioco e non è una migrazione SQLite.

## Conseguenze e gate

Vantaggi: testare la catena Scuola → punti → tecnologia → tier senza cambiare i salvataggi v13; distinguere formazione, disponibilità e assegnazioni; validare una parte degli invarianti demografici prima del collegamento alla sessione.

Rischi: a integrazione occorrerà un unico contatore abitanti riservati, salvataggi con backup, UI scuola/ricerca e benchmark anti-softlock. Rimborsi Cibo degli studenti e perdita della qualifica al ritorno civile sono **politiche candidate** e vanno approvate separatamente. Anche 1 posto di formazione, 2 ricercatori, 120 s, 10 Cibo e 0,1 punti/s non sono approvati.

**Gate obbligatorio:** prima i test Windows della v60, poi contratto v14 e migrazione su copie; non alterare `civil-growth-13` in un rilascio intermedio.
