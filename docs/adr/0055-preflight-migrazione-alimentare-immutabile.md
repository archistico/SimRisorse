# ADR 0055 — Preflight immutabile della migrazione alimentare

**Data:** 2026-11-04 · **Stato:** Accettato per F1-D05a, NON autorizza l'attivazione del nuovo gameplay.

## Contesto

La v14 salva `food`, `fish` e `bread` nell'inventario cittadino SQLite v3; il nuovo modello ha scorte grezze nel Deposito e Cibo pronto distinto. Migrare direttamente durante un tick o riscrivere il checkpoint esistente introduce rischio di duplicazioni, perdita di stock sovracapienti e blocco di partite storiche.

## Decisione

Preparare una mappa pura `V14FoodStockMigration` dal checkpoint v14 validato a `CityAfterTransfer` e `FoodDepotStock`, verificare la ricostruzione byte-logica delle quantità (milliesimi) e calcolare l'eventuale sovracapacità **senza clipping**. Integrare soltanto un metodo di preview su `V14SqliteSaveStore` che chiama `Load()` in sola lettura. Il checkpoint attuale non viene migrato né modificato. La versione 3 non viene riutilizzata per il nuovo stato.

## Conseguenze

Pro: verifiche prima delle scritture, nessuna perdita involontaria, backup e v13 protetti, prima verifica di conservazione eseguibile senza GUI. Contro: passaggio incrementale; la riforma non è ancora giocabile. L'attivazione reale richiederà un nuovo formato e nuova versione delle regole, persistenza atomica/rollback e integrazione contemporanea delle filiere, con gate Windows cumulativo.

## Non decisioni

Questa ADR non definisce la capacità iniziale del Deposito, il suo costo, i rendimenti delle cinque ricette, la politica di gestione della sovracapacità nel gameplay o la retention dei backup. Saranno definite e testate prima di una v15 giocabile, senza approvare coefficienti economici per implicito.
