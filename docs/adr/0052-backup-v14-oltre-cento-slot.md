# ADR 0052 — Backup v14 oltre i 100 slot, senza eliminazioni implicite

Data: 2026-11-04. Stato: approvazione tecnica RC42, gate Windows pendente.

## Problema

Il salvataggio v14 falliva dopo il centesimo backup per un limite artificiale del nome file, pur disponendo di spazio e di un checkpoint valido.

## Decisione

Conservare i primi 100 nomi per compatibilità. Dopo l'esaurimento, generare per ogni salvataggio un nome timestamp UTC + GUID sotto lo stesso prefisso v14, con `VACUUM INTO` prima della promozione. Non eliminare, sostituire, riordinare o spostare automaticamente alcun file di backup. Rifiutare salvaggi invalidi prima della copia.

## Conseguenze

L'operazione Salva non viene più bloccata dal solo numero di copie. Rimangono possibili errori per spazio disco, permessi e problemi I/O; il consumo di disco crescerà fino a un'eventuale politica di conservazione esplicitamente approvata. La v13 conserva i suoi file separati. [Prove](../validation/F1_D03_RC42_TEST_PLAN.md).
